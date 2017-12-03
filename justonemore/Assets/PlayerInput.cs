using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour {

    public PlayerCharacterController playerController;
    public OrbitFollow orbitFollow;

    /*
         Quaternion left;
    Quaternion right;
    Quaternion forward;
    Quaternion backward;
    left = Quaternion.Euler(0,0,0);
        right = Quaternion.Euler(180,0,0);
        forward = Quaternion.Euler(90,0,0);
        backward = Quaternion.Euler(270,0,0);
     */

    // Use this for initialization
    void Start () {

	}
	
	// Update is called once per frame
	void Update () {

        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        float lookHorizontal = Input.GetAxis("Mouse X");
        float lookVertical = Input.GetAxis("Mouse Y");

        float interact = Input.GetAxis("Interact");
        bool jump = Input.GetButton("Jump");

        float orbitDamp = 0.5f;
        orbitFollow.SetInputs(1, new Vector2(lookHorizontal * orbitDamp, lookVertical * orbitDamp));

        playerController.SetJumpPressed(jump);

        Vector3 direction = orbitFollow.transform.forward * moveVertical;
        direction += orbitFollow.transform.right * moveHorizontal;
        playerController.SetDesiredDirection(direction);
    }
}
