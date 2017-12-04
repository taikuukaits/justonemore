using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CameraMoveToLocation : MonoBehaviour {

    public float speed = 20f;

    public IEnumerator MoveToPositionCo(Vector3 position)
    {
        Vector3 startingPosition = transform.position;
        Vector3 endingPosition = position;
        float distance = Vector3.Distance(startingPosition, endingPosition);
        float time = distance / speed;

        float currentTime = 0;
        while (currentTime < time)
        {
            currentTime += Time.deltaTime;
            this.transform.position = Vector3.Lerp(startingPosition, endingPosition, DOVirtual.EasedValue(0f, 1f, currentTime/time, Ease.InOutCubic));
            yield return new WaitForEndOfFrame();
        }
    }
}
