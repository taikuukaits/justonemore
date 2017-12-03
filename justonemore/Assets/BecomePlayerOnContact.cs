using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BecomePlayerOnContact : MonoBehaviour, IPlayerCollideable
{

    public JustOneMoreController controller;
    public Player playerToActivate;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void DidCollide(Player player)
    {
        controller.ActivatePlayer(playerToActivate);
    }
}
