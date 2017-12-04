using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeactivatedPlayerSpawn : MonoBehaviour, IOnTileGroupSpawned
{

    void Start()
    {
        this.gameObject.SetActive(false);
    }

    public void OnTileGroupSpawned()
    {
        FindObjectOfType<JustOneMoreController>().RegisterDeactivatedPlayerTransform(this.transform);
    }
}
