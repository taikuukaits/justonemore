using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerTrigger : MonoBehaviour, IPlayerCollideable
{
    public PlayerTriggerEvent OnPlayerTrigger;

    private void Trigger(Player player)
    {
        if (player != null && OnPlayerTrigger != null)
        {
            OnPlayerTrigger.Invoke(player);
        }
    }
    public void DidCollide(Player player)
    {
        Trigger(player);
    }

    void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponentInParent<Player>();
        Trigger(player);
    }
}

[System.Serializable]
public class PlayerTriggerEvent : UnityEvent<Player> {

}