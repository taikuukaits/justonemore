using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public PlayerAnimator animator;
    public Transform followTarget;
    public PlayerCharacterController controller;
    public PlayerInput input;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Died()
    {
        animator.Died();

    }

    public void SetInputEnabled(bool enabled)
    {
        input.enabled = enabled;
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
