using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitInput : MonoBehaviour {

    public OrbitFollow orbitFollow;


    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void LateUpdate () {
        float lookHorizontal = Input.GetAxis("Mouse X");
        float lookVertical = Input.GetAxis("Mouse Y");

        float orbitDamp = 0.1f;
        orbitFollow.SetInputs(1, new Vector2(lookHorizontal * orbitDamp, lookVertical * orbitDamp));
        
    }
}
