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

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        float interact = Input.GetAxis("Interact");

        orbitFollow.SetInputs(1, new Vector2(horizontal, vertical));

        if (vertical > 0)
        {
            playerController.SetDirection(PlayerCharacterController.Direction.Forward);
        }
        else if (vertical < 0)
        {
            playerController.SetDirection(PlayerCharacterController.Direction.Backward);
        }
        else
        {
            playerController.SetDirection(PlayerCharacterController.Direction.None);
        }
    }
}
