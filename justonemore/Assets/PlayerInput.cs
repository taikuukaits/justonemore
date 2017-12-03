using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour {

    public PlayerCharacterController playerController;
    public Camera camera;
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

        if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");



        float interact = Input.GetAxis("Interact");
        bool jump = Input.GetButton("Jump");

        playerController.SetJumpPressed(jump);

        Vector3 direction = camera.transform.forward * moveVertical;
        direction += camera.transform.right * moveHorizontal;
        playerController.SetDesiredDirection(direction);

        Vector3 euler = camera.transform.rotation.eulerAngles;
        Quaternion locked = Quaternion.Euler(0, euler.y, 0);
        playerController.SetDesiredRotation(locked);
    }
}
