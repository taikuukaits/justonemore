using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DanceCameraController : MonoBehaviour {

    public FireworkController fireworkController;

    public static float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n)
    {
        return Mathf.Atan2(
            Vector3.Dot(n, Vector3.Cross(v1, v2)),
            Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
    }


    public float AngleOnXZ(Vector3 p1, Vector3 p2)
    {
        return Mathf.Atan2(p2.z - p1.z, p2.x - p1.x) * 180 / Mathf.PI;
    }

    public void Dance(List<Player> currentPlayers, Player byPlayer, OrbitInput orbitInput, OrbitFollow orbitFollow, Camera camera)
    {
        StartCoroutine(DanceCo(currentPlayers, byPlayer, orbitInput, orbitFollow, camera));
    }

    public IEnumerator DanceCo(List<Player> currentPlayers, Player byPlayer, OrbitInput orbitInput, OrbitFollow orbitFollow, Camera camera)
    {

        foreach (var currentPlayer in currentPlayers)
        {
            currentPlayer.SetInputEnabled(false);
        }

        orbitInput.enabled = false;
        orbitFollow.enabled = false;
        orbitFollow.SetAngle(0);

        float circumferenceSpeed = 15f;

        Vector3 oldTarget = orbitFollow.FollowTransform.position;
        Transform targetTransform = byPlayer.danceTarget;
        Vector3 target = targetTransform.position;
        Vector3 forward = targetTransform.forward;

        Vector3 cameraStartPosition = camera.transform.position;
        Vector3 cameraEndPosition = target + forward.normalized * 5f + Vector3.up * 0.25f;
        { //get into cameraEndPosition, looking at the Dance Target
            Vector3 midpoint = cameraStartPosition + ((cameraEndPosition - cameraStartPosition) / 2f);

            float startAngle = AngleOnXZ(midpoint, cameraStartPosition);
            float endAngle = AngleOnXZ(midpoint, cameraEndPosition);

            float angleToMove = startAngle - endAngle;

            float radius = Vector3.Distance(new Vector3(midpoint.x, 0, midpoint.z), new Vector3(cameraStartPosition.x, 0, cameraStartPosition.z));
            float circumference = Mathf.PI * 2f * radius;

            float distanceToMove = circumference * (Mathf.Abs(angleToMove) / 360f);
            float timeToMove = distanceToMove / circumferenceSpeed;
            Quaternion startRotation = camera.transform.rotation;
            float currentTime = 0f;
            float lastDegree = 0f;
            while (currentTime < timeToMove)
            {
                currentTime += Time.deltaTime;
                float nextDegree = DOVirtual.EasedValue(0f, angleToMove, currentTime / timeToMove, Ease.OutCubic);
                camera.transform.RotateAround(midpoint, Vector3.up, nextDegree - lastDegree);
                lastDegree = nextDegree;

                float clamp = Mathf.Clamp(currentTime, 0f, timeToMove - 0.25f);

                Vector3 cam = camera.transform.position;
                cam.y = DOVirtual.EasedValue(cameraStartPosition.y, cameraEndPosition.y, clamp / (timeToMove - 0.2f), Ease.InOutCubic); //subtract a bit so it finishes early?
                camera.transform.position = cam;

                camera.transform.LookAt(target + ((oldTarget - target) * (1 - DOVirtual.EasedValue(0, 1f, clamp / (timeToMove - 0.25f), Ease.OutCubic))));

                yield return new WaitForEndOfFrame();
            }

            Debug.Log(angleToMove / timeToMove);
        }

        yield return new WaitForSeconds(0.1f);

        foreach (var currentPlayer in currentPlayers)
        {
            currentPlayer.Dance();
            yield return new WaitForSeconds(0.25f);
        }

        fireworkController.Spawn(byPlayer.danceTarget.position);


        {//spin around the guy
            //translate circumferenceSpeed into degreesPerSecond
            float radius = Vector3.Distance(new Vector3(target.x, 0, target.z), new Vector3(camera.transform.position.x, 0, camera.transform.position.z));
            float circumference = Mathf.PI * 2f * radius;
            float timeToTravelCircumference = circumference / 10f;

            float totalDegrees = 360f * 2f; //two revolutions
            float timeToTravelDegree = timeToTravelCircumference / 360f; //time per degree
            float totalTime = timeToTravelDegree * totalDegrees; //degree * time per degree

            float currentTime = 0f;
            float lastDegree = 0f;
            while (currentTime < totalTime)
            {
                currentTime += Time.deltaTime;

                float nextDegree = DOVirtual.EasedValue(0f, totalDegrees, currentTime / totalTime, Ease.InOutQuad);
                float degreeDelta = nextDegree - lastDegree;
                lastDegree = nextDegree;

                camera.transform.RotateAround(target, Vector3.up, degreeDelta);

                if (currentTime / totalTime > 0.25f)
                {
                    camera.transform.position -= camera.transform.forward * Time.deltaTime * 2f;
                    camera.transform.LookAt(target);
                }

                yield return new WaitForEndOfFrame();
            }



        }

        if (false)
        {

            orbitFollow.SetInputs(1, new Vector2(0, 0));
            orbitInput.enabled = true;
            orbitFollow.enabled = true;
            orbitFollow.SetAngle(45f);

            foreach (var currentPlayer in currentPlayers)
            {
                currentPlayer.StopDance();
                currentPlayer.SetInputEnabled(true);
            }
        }
    }
}
