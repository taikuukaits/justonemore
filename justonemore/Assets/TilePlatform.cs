using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilePlatform : MonoBehaviour {

    public float delay = 0f;
    public int priority = 0;

    public bool flip = false;
    public Vector3 GetTileOrigin(Tile[] tiles)
    {
        if (flip)
        {
            float maxX = float.MinValue;
            float maxZ = float.MinValue;
            foreach (var tile in tiles)
            {
                maxX = Mathf.Max(maxX, tile.transform.position.x);
                maxZ = Mathf.Max(maxZ, tile.transform.position.z);
            }
            return new Vector3(maxX, 0, maxZ);
        }
        else
        {
            float minX = float.MaxValue;
            float minZ = float.MaxValue;
            foreach (var tile in tiles)
            {
                minX = Mathf.Min(minX, tile.transform.position.x);
                minZ = Mathf.Min(minZ, tile.transform.position.z);
            }
            return new Vector3(minX, 0, minZ);
        }
    }

}
