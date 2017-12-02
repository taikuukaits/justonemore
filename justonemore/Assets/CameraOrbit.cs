using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrbit : MonoBehaviour {

    public GameObject player;
    public Transform playerTransform;


    public float distance = 12f;
    public float currentHorizontalAngle = 0;

    private float offsetClampMagnitude;

	// Use this for initialization
	void Start () {
		Cursor.lockState = CursorLockMode.Locked;
        offsetClampMagnitude = Vector3.Distance(this.transform.position, this.player.transform.position);
    }
	
	// Update is called once per frame
	void LateUpdate () {
        float xMov = Input.GetAxis("Mouse X");
        float yMov = Input.GetAxis("Mouse Y");

        currentHorizontalAngle += xMov;

        //https://answers.unity.com/questions/766738/rotatearound-in-certain-radius.html
        Vector3 offset = transform.position - player.transform.position;
        Vector3 offsetClamped = offset.normalized * offsetClampMagnitude; //i think i want to keep the exact distance and sometimes it is shrinking // i also think i might want to do more to maintain rotation when moving
        this.transform.position = player.transform.position + offsetClamped; 

        transform.RotateAround(player.transform.position, Vector3.up, xMov);
        //transform.RotateAround(player.transform.position, transform.right, yMov); //forward for drunk effect


        Vector3 start = player.transform.position;
        start.y = transform.position.y;
        start.x = Mathf.Cos(currentHorizontalAngle) * distance;
        start.z = Mathf.Sin(currentHorizontalAngle) * distance;
        //transform.position = start;

        transform.LookAt(player.transform);

    }
}
