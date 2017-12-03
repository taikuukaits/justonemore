using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class JustOneMoreController : MonoBehaviour {

    public Camera mainCamera;
    public OrbitInput orbitInput;
    public OrbitFollow orbitFollow;
    public GameObject playerPrefab;

    public List<Transform> deactivatedPlayerPositions = new List<Transform>();
    public Dictionary<Transform, Player> playerPositions = new Dictionary<Transform, Player>(); 
    public Dictionary<Player, Transform> positionPlayers = new Dictionary<Player, Transform>(); 
    public Dictionary<Player, Transform> activePlayerPositions = new Dictionary<Player, Transform>();

    private List<Player> deadPlayers = new List<Player>();

    private List<Player> currentPlayers = new List<Player>();
    private List<Collider> playerColliders = new List<Collider>();
    
    // Use this for initialization
    void Start () {
        playerPrefab.SetActive(false);

        SpawnPlayers();
        ReSpawnDeactivatedPlayers();

        //StartCoroutine(LevelWon());
    }

    void ReSpawnDeactivatedPlayers()
    {
        foreach (var deactivatedPlayerPosition in deactivatedPlayerPositions)
        {
            ReSpawnDeactivatedPlayerPosition(deactivatedPlayerPosition);
        }
    }

    void ReSpawnDeactivatedPlayerPosition(Transform deactivatedPlayerPosition)
    {
        if (!playerPositions.ContainsKey(deactivatedPlayerPosition) || playerPositions[deactivatedPlayerPosition] == null)
        {
            Player newPlayer = NewDeactivedPlayer(deactivatedPlayerPosition.position);
            playerPositions[deactivatedPlayerPosition] = newPlayer;
            positionPlayers[newPlayer] = deactivatedPlayerPosition;
        }
    }

    void SpawnPlayers ()
    {
        NewActivePlayer(playerPrefab.transform.position);
        //NewPlayer(playerPrefab.transform.position + Vector3.left);
        //NewPlayer(playerPrefab.transform.position + Vector3.left * 2);
        //NewPlayer(playerPrefab.transform.position - Vector3.left);
        //NewPlayer(playerPrefab.transform.position - Vector3.left * 2);
        //NewPlayer(playerPrefab.transform.position + Vector3.forward);
        //NewPlayer(playerPrefab.transform.position + Vector3.forward * 2);
        //NewPlayer(playerPrefab.transform.position - Vector3.forward);
        //NewPlayer(playerPrefab.transform.position - Vector3.forward * 2);
    }

    public void ActivatePlayer(Player player)
    {
        if (positionPlayers.ContainsKey(player))
        {
            var deactivatedPlayerPosition = positionPlayers[player];
            positionPlayers.Remove(player);
            playerPositions.Remove(deactivatedPlayerPosition);
            activePlayerPositions[player] = deactivatedPlayerPosition;
        }

        var become = player.GetComponent<BecomePlayerOnContact>();
        if (become != null) {
            Destroy(become);
        }

        Collider[] colliders = player.GetComponentsInChildren<Collider>();
        playerColliders.AddRange(colliders);
        currentPlayers.Add(player);
        orbitFollow.SetFollowTarget(player.followTarget);

        player.GetComponentInChildren<PlayerInput>().enabled = true;

        foreach (var currentPlayer in currentPlayers)
        {
            currentPlayer.SetIgnoreColliders(playerColliders);
        }
    }

    Player NewPlayer(Vector3 position)
    {
        GameObject newPlayerGO = Instantiate(playerPrefab, position, playerPrefab.transform.rotation);
        newPlayerGO.SetActive(true);

        Player player = newPlayerGO.GetComponent<Player>();
        return player;
        
    }

    Player NewActivePlayer(Vector3 position)
    {
        Player player = NewPlayer(position);
        player.SetInputEnabled(true);
        ActivatePlayer(player);
        return player;
    }

    Player NewDeactivedPlayer(Vector3 position)
    {
        Player player = NewPlayer(position);
        player.SetInputEnabled(false);

        var becomePlayerOnContact = player.gameObject.AddComponent<BecomePlayerOnContact>();
        becomePlayerOnContact.playerToActivate = player;
        becomePlayerOnContact.controller = this;
        return player;
    }


    public void DidTouchWin(Player player)
    {
        //if (currentPlayers.Count > 2)
        //{
            StartCoroutine(LevelWon(player));
        //}
    }

    // Update is called once per frame
    void Update () {
        List<Player> newlyDeadPlayers = new List<Player>();
        foreach (var currentPlayer in currentPlayers)
        {
            if (currentPlayer.transform.position.y < -10)
            {
                newlyDeadPlayers.Add(currentPlayer);
                
            }
        }

        currentPlayers.RemoveAll(player => newlyDeadPlayers.Contains(player));
        deadPlayers.AddRange(newlyDeadPlayers);

        List<Player> completelyDeadPlayers = new List<Player>();
        foreach (Player deadPlayer in deadPlayers)
        {
            if (deadPlayer.transform.position.y < -50)
            {
                completelyDeadPlayers.Add(deadPlayer);
            }

            if (activePlayerPositions.ContainsKey(deadPlayer))
            {
                Transform deadPlayerTransform = activePlayerPositions[deadPlayer];
                activePlayerPositions.Remove(deadPlayer);
                ReSpawnDeactivatedPlayerPosition(deadPlayerTransform);
            }

        }

        deadPlayers.RemoveAll(player => completelyDeadPlayers.Contains(player));
        foreach (Player deadPlayer in completelyDeadPlayers)
        {
            Destroy(deadPlayer.gameObject);
        }

        if (currentPlayers.Count == 0)
        {
            SpawnPlayers();
            ReSpawnDeactivatedPlayers();
        }
        else
        {
            orbitFollow.SetFollowTarget(currentPlayers.First().followTarget);
        }

        if (Input.GetMouseButtonDown(1))
        {
            foreach (var currentPlayer in currentPlayers)
            {
                currentPlayer.SetInputEnabled(false);
            }

        }
        if (Input.GetMouseButtonUp(1))
        {
            foreach (var currentPlayer in currentPlayers)
            {
                currentPlayer.SetInputEnabled(true);
            }

        }

        if (Input.GetMouseButton(1))
        {
            float left = float.MaxValue;
            float right = float.MinValue;
            float down = float.MaxValue;
            float up = float.MinValue;

            foreach (var currentPlayer in currentPlayers)
            {
                float x = currentPlayer.transform.position.x;
                float z = currentPlayer.transform.position.z;
                left = Mathf.Min(x, left);
                right = Mathf.Max(x, right);
                up = Mathf.Max(z, up);
                down = Mathf.Min(z, down);
            }

            Vector3 center = new Vector3(left + ((right - left) / 2), 0, down + ((up - down) / 2));

            foreach (var currentPlayer in currentPlayers)
            {
                currentPlayer.OverrideDirection(center - currentPlayer.transform.position);
            }
        }

    }

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

    public IEnumerator LevelWon(Player byPlayer)
    {
        foreach (var currentPlayer in currentPlayers)
        {
            currentPlayer.SetInputEnabled(false);
            //currentPlayer.Dance();
        }

        orbitInput.enabled = false;
        orbitFollow.enabled = false;
        orbitFollow.SetAngle(0);
        
        float circumferenceSpeed = 15f;

        Vector3 oldTarget = orbitFollow.FollowTransform.position;
        Transform targetTransform = byPlayer.danceTarget;
        Vector3 target = targetTransform.position;
        Vector3 forward = targetTransform.forward;

        Vector3 cameraStartPosition = mainCamera.transform.position;
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
            Quaternion startRotation = mainCamera.transform.rotation;
            float currentTime = 0f;
            float lastDegree = 0f;
            while (currentTime < timeToMove)
            {
                currentTime += Time.deltaTime;
                float nextDegree = DOVirtual.EasedValue(0f, angleToMove, currentTime / timeToMove, Ease.OutCubic);
                mainCamera.transform.RotateAround(midpoint, Vector3.up, nextDegree - lastDegree);
                lastDegree = nextDegree;

                float clamp = Mathf.Clamp(currentTime, 0f, timeToMove - 0.25f);

                Vector3 cam = mainCamera.transform.position;
                cam.y = DOVirtual.EasedValue(cameraStartPosition.y, cameraEndPosition.y, clamp / (timeToMove - 0.2f), Ease.InOutCubic); //subtract a bit so it finishes early?
                mainCamera.transform.position = cam;

                mainCamera.transform.LookAt(target + ((oldTarget - target) * (1- DOVirtual.EasedValue(0, 1f, clamp / (timeToMove - 0.25f), Ease.OutCubic))));

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

        {//spin around the guy
            //translate circumferenceSpeed into degreesPerSecond
            float radius = Vector3.Distance(new Vector3(target.x, 0, target.z), new Vector3(mainCamera.transform.position.x, 0, mainCamera.transform.position.z));
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

                mainCamera.transform.RotateAround(target, Vector3.up, degreeDelta);

                if (currentTime / totalTime > 0.25f)
                {
                    mainCamera.transform.position -= mainCamera.transform.forward * Time.deltaTime * 2f;
                    mainCamera.transform.LookAt(target);
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
