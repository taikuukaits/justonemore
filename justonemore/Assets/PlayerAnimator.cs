using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

public class PlayerAnimator : MonoBehaviour {

    public Animator animator;
    public KinematicCharacterMotor motor;
    public PlayerCharacterController controller;
    // Use this for initialization
    void Start () {
		
	}

    bool cached = false;

	// Update is called once per frame
	void Update () {
        animator.SetFloat("Speed_f", motor.Velocity.magnitude);
        animator.SetBool("Jump_b", motor.Velocity.y > 0);
        animator.SetBool("Grounded", !(motor.Velocity.y > 0) || motor.IsStableOnGround);

    }

    public void Dance()
    {
        animator.SetInteger("Animation_int", 4);

    }

    public void StopDance()
    {
        animator.SetInteger("Animation_int", 0);

    }

    public void Died()
    {
        animator.SetBool("Death_b", true);

    }
}
