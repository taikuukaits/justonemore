using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawn : MonoBehaviour, IOnTileGroupSpawned
{
    public Player.PlayerType playerType;

    [System.NonSerialized]
    public Player lastSpawnedPlayer;

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
        if (this.gameObject.activeSelf) return;
        FindObjectOfType<JustOneMoreController>().RegisterPlayerSpawn(this);
        this.gameObject.SetActive(true);
    }
}
