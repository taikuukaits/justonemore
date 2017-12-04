using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTriggerable : MonoBehaviour, IPlayerCollideable
{

    public virtual void OnPlayerTrigger(Player player)
    {
        
    }

    public void DidCollide(Player player)
    {
        OnPlayerTrigger(player);
    }

    void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponentInParent<Player>();
        OnPlayerTrigger(player);
    }
}
