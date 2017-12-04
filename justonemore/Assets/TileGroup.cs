using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGroup : MonoBehaviour {

    public string groupName;
    public bool startsDisabled;
    private bool spawned = false;

    public void Spawn()
    {
        if (spawned) return;
        spawned = true;

        var tiles = GetComponentsInChildren<Tile>(); 

        float minX = float.MaxValue;
        float minZ = float.MaxValue;
        foreach (var tile in tiles)
        { 
            minX = Mathf.Min(minX, tile.transform.position.x);
            minZ = Mathf.Min(minZ, tile.transform.position.z);
        }

        foreach (var tile in tiles)
        {
            float x = tile.transform.position.x - minX;
            float z = tile.transform.position.z - minZ;
            tile.Spawned(x * 0.1f + z * 0.1f);
        }

        foreach (var onSpawn in GetComponentsInChildren<IOnTileGroupSpawned>(includeInactive: true))
        {
            onSpawn.OnTileGroupSpawned();
        }

    }

    void Despawn()
    {
        var tiles = GetComponentsInChildren<Tile>();
        foreach (var tile in tiles)
        {
            tile.Despawn();
        }
    }

	// Use this for initialization
	void Start () {
		if (startsDisabled)
        {
            Despawn();
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
