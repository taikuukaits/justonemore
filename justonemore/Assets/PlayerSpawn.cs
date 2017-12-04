using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawn : MonoBehaviour, IOnTileGroupSpawned
{
    void Start()
    {
        this.gameObject.SetActive(false);
    }

    public void OnTileGroupSpawned()
    {
        BecomeAvailable();
    }

    public void BecomeAvailable()
    {
        if (this.gameObject.active) return;
        FindObjectOfType<JustOneMoreController>().RegisterPlayerSpawn(this.transform);
        this.gameObject.SetActive(true);
    }
}
