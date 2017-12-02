using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMotor : MonoBehaviour { // XZ plane

    public Rigidbody rigidBody;
    public bool goLeft = false;
    public bool goRight = false;
    public bool goUp = false;
    public bool goDown = false;

    // Use this for initialization
    void Start () {
    }
	
	// Update is called once per frame
	void Update () {

        float x = 0;
        if (goLeft)
        {
            x = 1;
        }else if (goRight)
        {
            x = -1;
        }

        float z = 0;
        if (goUp)
        {
            z = 1;
        }else if (goDown)
        {
            z = -1;
        }

        rigidBody.velocity = new Vector3(x, 0, z);

    }
}
