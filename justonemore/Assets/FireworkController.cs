using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireworkController : MonoBehaviour {

    public List<GameObject> fireworkPrefabs = new List<GameObject>();

    private float range = 3f;

    public void Spawn(Vector3 position)
    {
        StartCoroutine(SpawnCo(position));
    }

    public IEnumerator SpawnCo(Vector3 position)
    {
        position += Vector3.up * 2f;
        int toSpawn = Random.Range(10, 15);
        while (toSpawn > 0)
        {
            toSpawn--;

            float delay = Random.Range(1f, 8f);
            StartCoroutine(SpawnFireworkCo(position, delay));

        }

        yield return null;
    }

    public IEnumerator SpawnFireworkCo(Vector3 position, float delay)
    {

        yield return new WaitForSeconds(delay);
        GameObject nextPrefab = fireworkPrefabs[Random.Range(0, fireworkPrefabs.Count)];
        GameObject clone = Instantiate(nextPrefab);
        Vector3 point = Random.insideUnitCircle;
        point.y = 0;
        clone.transform.position = position + (point * range);

    }
}
