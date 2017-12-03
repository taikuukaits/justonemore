using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using UnityEngine.Events;

public class PlayerCharacterController : BaseCharacterController
{

    [Header("Stable Movement")]
    public float MaxStableMoveSpeed = 10f; // Max speed when stable on ground
    public float StableMovementSharpness = 15; // Sharpness of the acceleration when stable on ground
    public float OrientationSharpness = 10; // Sharpness of rotations when stable on ground

    [Header("Air Movement")]
    public float MaxAirMoveSpeed = 10f; // Max speed for air movement
    public float AirAccelerationSpeed = 5f; // Acceleration when in air
    public float Drag = 0.1f; // Air drag

    [Header("Jumping")]
    public bool AllowJumpingWhenSliding = false; // Is jumping allowed when we are sliding down a surface, even if we are not "stable" on it?
    public float JumpSpeed = 10f; // Strength of the jump impulse
    public float JumpPreGroundingGraceTime = 0f; // Time before landing that jump inputs will be remembered and applied at the moment of landing
    public float JumpPostGroundingGraceTime = 0f; // Time after getting un-grounded that jumping will still be allowed


    private Vector3 _internalVelocityAdd = Vector3.zero;
    private bool _jumpRequested = false;
    private bool _jumpConsumed = false;
    private bool _jumpedThisFrame = false;
    private Vector3 _jumpDirection = Vector3.up;
    private float _timeSinceJumpRequested = Mathf.Infinity;
    private float _timeSinceLastAbleToJump = 0f;
    private Vector3 desiredDirection;


    [Header("Jumping Floatiness")]
    public float jumpPressedRiseGravityMultiplier = 5f;
    public float jumpReleasedRiseGravityMultiplier = 10f;
    public float jumpPressedFallGravityMultiplier = 7f;
    public float jumpReleasedFallGravityMultiplier = 20f;

    private List<Collider> ignoreColliders = new List<Collider>();
    public void SetIgnoreColliders(List<Collider> colliders)
    {
        ignoreColliders = colliders;

    }
    public GameObject playerCamera;

    /// <summary>
    /// Asks if the character should probe for ground on this character update (return true or false). 
    /// Note that if ground probing finds valid ground, the character will automatically snap to the
    /// ground surface.
    /// </summary>
    override public bool MustUpdateGrounding()
    {
        return true;
    }

    public bool GetJumpedThisFrame()
    {
        return _jumpedThisFrame;
    }

    /// <summary>
    /// Asks what the character's rotation should be on this character update. 
    /// Modify the "currentRotation" to change the character's rotation.
    /// </summary>
    override public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        currentRotation = desiredRotation;
    }

    private Quaternion desiredRotation = Quaternion.identity;
    public void SetDesiredRotation(Quaternion desiredRotation)
    {
        this.desiredRotation = desiredRotation;
    }

    /// <summary>
    /// Asks what the character's velocity should be on this character update. 
    /// Modify the "currentVelocity" to change the character's velocity.
    /// </summary>
    public Vector3 CacheVelocity;
    override public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        
        Vector3 targetMovementVelocity = Vector3.zero;
        if (KinematicCharacterMotor.IsStableOnGround)
        {
            // Reorient velocity on slope
            currentVelocity = KinematicCharacterMotor.GetDirectionTangentToSurface(currentVelocity, KinematicCharacterMotor.GroundNormal) * currentVelocity.magnitude;

            // Calculate target velocity
            Vector3 inputRight = Vector3.Cross(desiredDirection, KinematicCharacterMotor.CharacterUp);
            Vector3 reorientedInput = Vector3.Cross(KinematicCharacterMotor.GroundNormal, inputRight).normalized * desiredDirection.magnitude;
            targetMovementVelocity = reorientedInput * MaxStableMoveSpeed;

                currentVelocity = targetMovementVelocity;
            // Independant movement Velocity
            if (targetMovementVelocity.magnitude > 0)
            {
            }
            else
            {
                //currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1 - Mathf.Exp(-StableMovementSharpness * deltaTime));
            }
        }
        else
        {
            // Add move input
            if (desiredDirection.sqrMagnitude > 0f)
            {
                targetMovementVelocity = desiredDirection * MaxAirMoveSpeed;
                Vector3 velocityDiff = Vector3.ProjectOnPlane(targetMovementVelocity - currentVelocity, Physics.gravity);
                currentVelocity += velocityDiff * AirAccelerationSpeed * deltaTime;
            }

            // Gravity
            currentVelocity += Physics.gravity * deltaTime;

            // Drag
            //currentVelocity *= (1f / (1f + (Drag * deltaTime)));

        }

        // Handle jumping
        _jumpedThisFrame = false;
        _timeSinceJumpRequested += deltaTime;
        if (_jumpRequested)
        {
            // See if we actually are allowed to jump
            if (!_jumpConsumed && ((AllowJumpingWhenSliding ? KinematicCharacterMotor.FoundAnyGround : KinematicCharacterMotor.IsStableOnGround) || (JumpPostGroundingGraceTime > 0 && _timeSinceLastAbleToJump <= JumpPostGroundingGraceTime)))
            {
                // Calculate jump direction before ungrounding
                Vector3 jumpDirection = KinematicCharacterMotor.CharacterUp;
                if (KinematicCharacterMotor.FoundAnyGround && !KinematicCharacterMotor.IsStableOnGround)
                {
                    //jumpDirection = KinematicCharacterMotor.GroundNormal;
                }

                // Makes the character skip ground probing/snapping on its next update. 
                // If this line weren't here, the character would remain snapped to the ground when trying to jump. Try commenting this line out and see.
                KinematicCharacterMotor.ForceUnground();

                // Add to the return velocity and reset jump state
                currentVelocity += (jumpDirection * JumpSpeed);// - Vector3.Project(currentVelocity, KinematicCharacterMotor.CharacterUp);
                _jumpRequested = false;
                _jumpConsumed = true;
                _jumpedThisFrame = true;
            }

        }

        //huge help: https://www.youtube.com/watch?v=7KiK0Aqtmzc
        //fast fall
        if (!KinematicCharacterMotor.IsStableOnGround)
        {
            if (currentVelocity.y < -0.1f) //if we are falling
            {
                if (jumpPressed)
                {
                    currentVelocity += Physics.gravity * jumpPressedFallGravityMultiplier * deltaTime;
                }
                else
                {
                    currentVelocity += Physics.gravity * jumpReleasedFallGravityMultiplier * deltaTime;
                }
            }
            else //rising
            {
                if (!jumpPressed)
                {
                    currentVelocity += Physics.gravity * jumpReleasedRiseGravityMultiplier * deltaTime;
                }
                else
                {
                    currentVelocity += Physics.gravity * jumpPressedRiseGravityMultiplier * deltaTime;
                }
            }
        }

        CacheVelocity = currentVelocity;

    }


    private bool jumpPressed = false;
    private bool jumpPressedCache = false;
    public void SetJumpPressed(bool jumpPressed)
    {
        if (jumpPressedCache != jumpPressed)
        {
            jumpPressedCache = jumpPressed;
            StartCoroutine(jumpCo());
        }

    }

    IEnumerator jumpCo()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        bool jumpPressed = jumpPressedCache;
        if (jumpPressed && !this.jumpPressed)
        {
            _timeSinceJumpRequested = 0f;
            _jumpRequested = true;
        }
        this.jumpPressed = jumpPressed;
    }


    public void SetDesiredDirection(Vector3 desiredDirection)
    {
        this.desiredDirection = Vector3.ClampMagnitude(desiredDirection, 1f);
    }

    /// <summary>
    /// Gives you a callback for before the character update begins, if you 
    /// want to do anything to start off the update.
    /// </summary>
    override public void BeforeCharacterUpdate(float deltaTime) {

    }

    /// <summary>
    /// Gives you a callback for when the character update has reached its end, if you 
    /// want to do anything to finalize the update.
    /// </summary>
    override public void AfterCharacterUpdate(float deltaTime)
    {
        // Handle jump-related values
        {
            // Handle jumping pre-ground grace period
            if (_jumpRequested && _timeSinceJumpRequested > JumpPreGroundingGraceTime)
            {
                _jumpRequested = false;
            }

            if (AllowJumpingWhenSliding ? KinematicCharacterMotor.FoundAnyGround : KinematicCharacterMotor.IsStableOnGround)
            {
                // If we're on a ground surface, reset jumping values
                if (!_jumpedThisFrame)
                {
                    _jumpConsumed = false;
                }
                _timeSinceLastAbleToJump = 0f;
            }
            else
            {
                // Keep track of time since we were last able to jump (for grace period)
                _timeSinceLastAbleToJump += deltaTime;
            }
        }

        // Grounding considerations
        if (KinematicCharacterMotor.IsStableOnGround && !KinematicCharacterMotor.WasStableOnGround)
        {
            //OnLanded();
        }
        else if (!KinematicCharacterMotor.IsStableOnGround && KinematicCharacterMotor.WasStableOnGround)
        {
            //OnLeaveStableGround();
        }

        if (AllowJumpingWhenSliding ? KinematicCharacterMotor.FoundAnyGround : KinematicCharacterMotor.IsStableOnGround)
        {
            _jumpConsumed = false;
            _timeSinceLastAbleToJump = 0f;
        }
        else
        {
            _timeSinceLastAbleToJump += deltaTime;
        }
    }

    /// <summary>
    /// Asks if a given collider should be considered for character collisions.
    /// Useful for ignoring specific colliders in specific situations.
    /// </summary>
    override public bool IsColliderValidForCollisions(Collider coll)
    {
            //return !ignoreColliders.Contains(coll);
        if (jumpPressedCache || jumpPressed || !KinematicCharacterMotor.IsStableOnGround || KinematicCharacterMotor.Velocity.y > 0)
        {
            return !ignoreColliders.Contains(coll);
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// Asks if the character can stand stable on a given collider.
    /// </summary>
    override public bool CanBeStableOnCollider(Collider coll)
    {
        return true;
    }

    /// <summary>
    /// Gives you a callback for ground probing hits
    /// </summary>
    override public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, bool isStableOnHit)
    {

    }

    /// <summary>
    /// Gives you a callback for character movement hits
    /// </summary>
    override public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, bool isStableOnHit)
    {
        IPlayerCollideable[] collideables = hitCollider.gameObject.GetComponentsInChildren<IPlayerCollideable>();
        if (collideables.Length > 0)
        {
            Player player = GetComponentInParent<Player>();
            foreach (var collideable in collideables)
            {
                collideable.DidCollide(player);
            }
        }
    }


}
