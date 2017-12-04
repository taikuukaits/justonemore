using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerTileSpawn : MonoBehaviour, IPlayerCollideable
{

    public TileSpawner tileSpawner;
    private bool spawned = false;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnContact() {
        if (!spawned)
        {
            spawned = true;
            tileSpawner.Spawn();
        }
    }

    public void DidCollide(Player player)
    {
        OnContact();
    }

    void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponentInParent<Player>();
        if (player != null)
        {
            OnContact();
        }
    }
}
