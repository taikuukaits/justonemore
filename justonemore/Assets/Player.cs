using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Player : MonoBehaviour {

    public GameObject leaderIcon;
    public PlayerAnimator animator;
    public Transform followTarget;
    public Transform danceTarget;
    public PlayerCharacterController controller;
    public GameObject model;
    public DOTweenAnimation scaleAnimation;

    public PlayerInput input;

    public Vector3 velocity;
    public Vector3 basevelocity;
    public Vector3 cachevelocity;
    
    // Use this for initialization
    void Start () {
		
	}

    public void BecomeLeader()
    {
        leaderIcon.SetActive(true);
    }

    public void DiscardLeader() {
        leaderIcon.SetActive(false);
    }

    // Update is called once per frame
    void Update () {
        velocity = controller.KinematicCharacterMotor.Velocity;	
        basevelocity = controller.KinematicCharacterMotor.BaseVelocity;	
        cachevelocity = controller.CacheVelocity;	
	}

    public void Scale()
    {
        model.transform.localScale = Vector3.zero;
        scaleAnimation.enabled = true;
    }

    public void Died()
    {
        animator.Died();

    }

    public void Dance()
    {
        animator.Dance();

    }
    public void StopDance()
    {
        animator.StopDance();

    }

    public void SetInputEnabled(bool enabled)
    {
        input.enabled = enabled;
        controller.SetDesiredDirection(Vector3.zero);
    }

    public void OverrideDirection(Vector3 direction)
    {
        controller.SetDesiredDirection(direction);
    }

    public void SetIgnoreColliders(List<Collider> colliders)
    {
        controller.SetIgnoreColliders(colliders);
    }
}
