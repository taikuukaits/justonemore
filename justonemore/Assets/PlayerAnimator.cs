using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

public class PlayerAnimator : MonoBehaviour {

    public Animator animator;
    public KinematicCharacterMotor motor;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        animator.SetFloat("Speed_f", motor.Velocity.magnitude);
	}
}
