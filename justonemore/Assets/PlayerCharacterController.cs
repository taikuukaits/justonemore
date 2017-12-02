using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

public class PlayerCharacterController : BaseCharacterController
{

    public enum Direction
    {
        None,
        Forward, 
        Backward,
        Left, 
        Right
    }

    private Direction currentDirection;
    public void SetDirection(Direction newDirection)
    {
        currentDirection = newDirection;
    }

    public GameObject camera;

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
        Vector3 euler = camera.transform.rotation.eulerAngles;
        Quaternion locked = Quaternion.Euler(0, euler.y, 0);
        currentRotation = locked;
    }

    /// <summary>
    /// Asks what the character's velocity should be on this character update. 
    /// Modify the "currentVelocity" to change the character's velocity.
    /// </summary>
    override public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        Vector3 desired = DesiredVelocity();

        currentVelocity = desired + Physics.gravity;
    }

    private Vector3 DesiredVelocity()
    {
        float speed = 5f;

        if (currentDirection == Direction.Forward)
        {
            return (transform.forward).normalized * speed;
        }
        else if (currentDirection == Direction.Backward)
        {
            return (-transform.forward).normalized * speed;
        }
        else if (currentDirection == Direction.Left)
        {

        }
        else if (currentDirection == Direction.Right)
        {

        }

        return Vector3.zero;

        
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
