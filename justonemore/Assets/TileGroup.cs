using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TileGroup : MonoBehaviour {

    public string groupName;
    public bool startsDisabled;
    private bool spawned = false;

    public void Spawn(Vector3 spawnLocation)
    {
        if (spawned) return;
        spawned = true;

        List<TilePlatform> platforms = new List<TilePlatform>(GetComponentsInChildren<TilePlatform>());
        platforms = platforms.OrderBy(platform => platform.priority).ToList();

        float platformDelay = 0f;
        foreach (var platform in platforms)
        {
            var tiles = platform.GetComponentsInChildren<Tile>();

            Vector3 origin = platform.GetTileOrigin(tiles);

            foreach (var tile in tiles)
            {
                float x = Mathf.Abs(tile.transform.position.x - origin.x);
                float z = Mathf.Abs(tile.transform.position.z - origin.z);
                tile.Spawned(platformDelay + x * 0.1f + z * 0.1f);
            }

            foreach (var onSpawn in GetComponentsInChildren<IOnTileGroupSpawned>(includeInactive: true))
            {
                onSpawn.OnTileGroupSpawned();
            }

            platformDelay += platform.delay;

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
