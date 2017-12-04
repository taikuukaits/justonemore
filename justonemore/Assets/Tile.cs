using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Tile : MonoBehaviour {

    public void Despawn()
    {
        this.transform.localScale = Vector3.zero;

    }

    public void Spawned(float delay)
    {
        StartCoroutine(SpawnedCo(delay));
        //StartCoroutine(Spin());
    }
    IEnumerator SpawnedCo(float delay)
    {
        yield return new WaitForSeconds(delay);

        this.transform.DOScale(Vector3.one, 1f);
        Vector3 position = this.transform.localPosition;
        float y = position.y;
        position.y -= 1;
        this.transform.localPosition = position;
        this.transform.DOLocalMoveY(y, 2f).SetEase(Ease.OutElastic);
    }

    IEnumerator Spin()
    {
        float currentTime = 0;
        float totalTime = 3f;
        float totalRotations = 1f;
        float totalDegrees = totalRotations * 360f;

        var startingRotation = this.transform.rotation;

        float lastDegree = 0;
        while (currentTime < totalTime)
        {
            currentTime += Time.deltaTime;
            float nextDegree = DOVirtual.EasedValue(0f, totalDegrees, Mathf.Clamp01(currentTime / totalTime), Ease.Linear);
            float deltaDegree = nextDegree - lastDegree;
            lastDegree = nextDegree;

            this.transform.RotateAround(transform.position, Vector3.up, deltaDegree);
            yield return new WaitForEndOfFrame();
        }

        Debug.Log("spawned");
    }

	// Use this for initialization
	void Start () {

    }

    // Update is called once per frame
    void Update () {
	}
}
