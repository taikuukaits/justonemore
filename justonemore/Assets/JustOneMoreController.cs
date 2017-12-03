using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JustOneMoreController : MonoBehaviour {

    public Camera mainCamera;
    public OrbitInput orbitInput;
    public OrbitFollow orbitFollow;
    public GameObject playerPrefab;

    public List<Transform> deactivatedPlayerPositions = new List<Transform>();
    public Dictionary<Transform, Player> playerPositions = new Dictionary<Transform, Player>(); 
    public Dictionary<Player, Transform> positionPlayers = new Dictionary<Player, Transform>(); 

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
            if (!playerPositions.ContainsKey(deactivatedPlayerPosition) || playerPositions[deactivatedPlayerPosition] == null)
            {
                Player newPlayer = NewDeactivedPlayer(deactivatedPlayerPosition.position);
                playerPositions[deactivatedPlayerPosition] = newPlayer;
                positionPlayers[newPlayer] = deactivatedPlayerPosition;
            }
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


    public void DidTouchWin()
    {
        //if (currentPlayers.Count > 2)
        //{
            StartCoroutine(LevelWon());
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

    public IEnumerator LevelWon()
    {
        foreach (var currentPlayer in currentPlayers)
        {
            currentPlayer.SetInputEnabled(false);
            currentPlayer.Dance();
        }

        orbitInput.enabled = false;
        orbitFollow.enabled = false;
        orbitFollow.SetAngle(0);
        
        float circumferenceSpeed = 5f;
        
        //ugh this is fucked!
        Transform followTarget = orbitFollow.FollowTransform;
        Transform targetTransform = currentPlayers.First().transform;
        Vector3 target = targetTransform.position;
        Vector3 cameraStartPosition = mainCamera.transform.position;
        Vector3 forward = targetTransform.forward;
        forward.y = 0;
        Vector3 cameraTargetPosition = target + forward.normalized * 5f + Vector3.up * 3f;

        Vector3 midpoint = cameraStartPosition + ((cameraTargetPosition - cameraStartPosition) / 2f);

        float radiusMid = Vector3.Distance(new Vector3(midpoint.x, 0, midpoint.z), new Vector3(cameraStartPosition.x, 0, cameraStartPosition.z));
        float circumferenceMid = Mathf.PI * 2f * radiusMid;
        float timeToGoRoundMid = circumferenceMid / circumferenceSpeed;
        float degreesPerSecondMid = 360f / timeToGoRoundMid;
        float diameterSwirl = Vector3.Distance(new Vector3(target.x, 0, target.z), new Vector3(mainCamera.transform.position.x, 0, mainCamera.transform.position.z));
        float circumferenceSwirl = Mathf.PI * diameterSwirl;
        float timeToGoRoundSwirl = circumferenceSwirl / circumferenceSpeed;
        float degreesPerSecondSwirl = 360f / timeToGoRoundSwirl;
        //gotta go 180 degrees
        float totalStartTime = 180f / degreesPerSecondMid;
        float startTime = 0;
        float speed = 1f;
        while (startTime < totalStartTime)
        {
            startTime += Time.deltaTime;
            mainCamera.transform.RotateAround(midpoint, Vector3.up, degreesPerSecondSwirl * Time.deltaTime);

            Vector3 cam = mainCamera.transform.position;
            cam.y = Mathf.Lerp(cameraStartPosition.y, cameraTargetPosition.y, startTime / totalStartTime);
            mainCamera.transform.position = cam;

            Vector3 direction = transform.position - target;
            Quaternion toRotation = Quaternion.FromToRotation(transform.forward, direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, speed * Time.time);

            yield return new WaitForEndOfFrame();
        }


        float cameraMoveTime = 2f;
        float currentCameraMoveTime = 0f;

        //while (currentCameraMoveTime < cameraMoveTime)
        // {
        //m//ainCamera.transform.RotateAround(target, Vector3.up, degreesPerSecond * Time.deltaTime);
        //yield return new WaitForEndOfFrame();
        //}



        float cooldownTime = 2f;
        float coolDownPerSecond = degreesPerSecondSwirl / cooldownTime;
        float time = 0f;
        float totalTime = 12f;

        
        while (time < totalTime)
        {
            mainCamera.transform.RotateAround(target, Vector3.up, degreesPerSecondSwirl * Time.deltaTime);
            //mainCamera.transform.position -= mainCamera.transform.forward * Time.deltaTime * 1f;

            if (time > totalTime - cooldownTime)
            {
                degreesPerSecondSwirl -= coolDownPerSecond * Time.deltaTime;
            }
            time += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(5f);


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
