using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileSpawner : MonoBehaviour {

    public List<TileDefinition> tileDefinitions = new List<TileDefinition>();


   

    public void Spawn()
    {
        Vector3 platformPosition = new Vector3(-14, 0, 26);
        int platformWidth = 4;
        int platformHeight = 4;
        float tileWidth = 1f;
        float tileHeight = 1f;

        for (var x = 0; x < platformWidth; x++)
        {
            for (var z = 0; z < platformHeight; z++)
            {

                Vector3 spawnLocation = new Vector3();
                spawnLocation.x = platformPosition.x + ((float)x * tileWidth);
                spawnLocation.z = platformPosition.z + ((float)z * tileHeight);
                spawnLocation.y = platformPosition.y;

                var tileDefinition = GetTileDefinition("dirt");
                var clone = Instantiate(tileDefinition.prefab);
                clone.transform.position = spawnLocation;
                clone.SetActive(true);
                var tile = clone.GetComponent<Tile>();

                tile.Spawned(x * 0.1f + z * 0.1f);
            }
        }

    }



    TileDefinition GetTileDefinition(string tileName)
    {
        foreach (var tileDefinition in tileDefinitions){
            if (tileDefinition.tileName == tileName)
            {
                return tileDefinition;
            }
        }
        throw new System.Exception("Tile not found " + tileName + "!");
    }

    // Use this for initialization
    void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}


[System.Serializable]
public class TileDefinition {
    public string tileName;
    public GameObject prefab;
}