using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

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

    /// <summary>
    /// Asks what the character's rotation should be on this character update. 
    /// Modify the "currentRotation" to change the character's rotation.
    /// </summary>
    override public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        Vector3 euler = playerCamera.transform.rotation.eulerAngles;
        Quaternion locked = Quaternion.Euler(0, euler.y, 0);
        currentRotation = locked;
    }

    /// <summary>
    /// Asks what the character's velocity should be on this character update. 
    /// Modify the "currentVelocity" to change the character's velocity.
    /// </summary>
    private bool wantsToMoveCached = false;
    private float timeSinceWantToMoveChange = 0;

    private float currentJumpRiseTime = 0f;
    private float currentJumpFallTime = 0f;
    private Vector3 currentJumpVelocity= new Vector3(0, 0f, 0);

    public float jumpAccelTime = 0.2f;
    public Vector3 jumpAccel = new Vector3(0, 2f, 0);
    public Vector3 jumpAntiGravity = new Vector3(0, 3f, 0);
    private Vector3 jumpVelocity = new Vector3(0, 10f, 0);

    private float minimumJumpRiseTime = 0.4f;
    private float maximumJumpRiseTime = 0.4f;

    public float pressedRisingAccel = 0;
    public float releasedRisingAccel = 0;
    public float pressedFallingVelocity = 3f;

    enum JumpState
    {
        Grounded,
        PressedRising,
        ReleasedRising, //support minimum required jump height
        PressedFalling,
        ReleasedFalling
    }
    private JumpState jumpState = JumpState.ReleasedFalling;

    void ProcessJumpTimeAndState(float deltaTime)
    {
        if (KinematicCharacterMotor.IsStableOnGround)
        {
            jumpState = JumpState.Grounded;
        }

        if (jumpState == JumpState.Grounded && jumpPressed)
        {
            currentJumpRiseTime = 0;
            currentJumpFallTime = 0;
            currentJumpVelocity = jumpVelocity;
            jumpState = JumpState.PressedRising;
        }
        else if (jumpState == JumpState.PressedRising && !jumpPressed)
        {
            jumpState = JumpState.ReleasedRising;
        }
        else if (jumpState == JumpState.PressedFalling && !jumpPressed)
        {
            jumpState = JumpState.ReleasedFalling;
        }
        else if (jumpState == JumpState.ReleasedFalling)
        {
            //nothing
        }

        if (jumpState == JumpState.PressedRising && currentJumpRiseTime > maximumJumpRiseTime)
        {
            jumpState = JumpState.PressedFalling;
        }
        else if (jumpState == JumpState.ReleasedRising && currentJumpRiseTime > minimumJumpRiseTime)
        {
            jumpState = JumpState.ReleasedFalling;
        }

        if (jumpState == JumpState.PressedRising || jumpState == JumpState.ReleasedRising)
        {
            currentJumpRiseTime += deltaTime;
        }
        else if (jumpState == JumpState.PressedFalling || jumpState == JumpState.ReleasedFalling)
        {
            currentJumpFallTime += deltaTime;
        }

    }

    void ProcessJumpVelocity(float deltaTime)
    {
        if (jumpState == JumpState.Grounded)
        {
            currentJumpVelocity = Vector3.zero;
        }
        else if (jumpState == JumpState.PressedRising)
        {
            currentJumpVelocity -= pressedFallingVelocity;
        }
        else if (jumpState == JumpState.ReleasedRising)
        {
            currentJumpVelocity += ReleasedRisingAccelCurve.Evaluate(currentJumpRiseTime) * Vector3.up;
        }
        else if (jumpState == JumpState.ReleasedFalling)
        {
            currentJumpVelocity = new Vector3(0, pressedFallingVelocity, 0);
        }
        else if (jumpState == JumpState.PressedFalling)
        {
            currentJumpVelocity = Vector3.zero;
        }
    }

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

            bool wantsToMove = desiredDirection.magnitude <= 0.01;

            if (wantsToMove != wantsToMoveCached)
            {
                timeSinceWantToMoveChange = 0;
            }
            else
            {
                //

            }

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
        }

        ProcessJumpTimeAndState(deltaTime);
        ProcessJumpVelocity(deltaTime);

        currentVelocity += jumpVelocity;

    }

    private bool jumpPressed = false;
    private float jumpPressTime = 0;
    private bool jumping = false;



    public void SetJumpPressed(bool value)
    {
        jumpPressed = value;
    }


    public void SetDesiredDirection(Vector3 desiredDirection)
    {
        this.desiredDirection = desiredDirection.normalized;
    }

    /// <summary>
    /// Gives you a callback for before the character update begins, if you 
    /// want to do anything to start off the update.
    /// </summary>
    override public void BeforeCharacterUpdate(float deltaTime)
    {

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
        return true;
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

    }

}
