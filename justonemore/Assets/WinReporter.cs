using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinReporter : MonoBehaviour, IPlayerCollideable
{

    public void DidCollide(Player player)
    {
        controller.DidTouchWin(player);
    }

    public JustOneMoreController controller;

    void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponentInParent<Player>();
        if (player != null)
        {
            controller.DidTouchWin(player);
        }
    }
}
