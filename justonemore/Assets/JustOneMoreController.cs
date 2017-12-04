using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JustOneMoreController : MonoBehaviour {

    public DanceCameraController danceController;

    public Camera mainCamera;
    public OrbitInput orbitInput;
    public OrbitFollow orbitFollow;
    public GameObject playerPrefab;

    private List<Transform> deactivatedPlayerPositions = new List<Transform>();
    private Dictionary<Transform, Player> playerPositions = new Dictionary<Transform, Player>();
    private Dictionary<Player, Transform> positionPlayers = new Dictionary<Player, Transform>();
    private Dictionary<Player, Transform> activePlayerPositions = new Dictionary<Player, Transform>();

    private List<Player> deadPlayers = new List<Player>();

    private List<Player> currentPlayers = new List<Player>();
    private List<Collider> playerColliders = new List<Collider>();
    
    public void RegisterDeactivatedPlayerTransform(Transform newTransform)
    {
        deactivatedPlayerPositions.Add(newTransform);
        ReSpawnDeactivatedPlayerPosition(newTransform);
    }

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
        player.Scale();

        var becomePlayerOnContact = player.gameObject.AddComponent<BecomePlayerOnContact>();
        becomePlayerOnContact.playerToActivate = player;
        becomePlayerOnContact.controller = this;
        return player;
    }


    public void DidTouchWin(Player player)
    {
        //if (currentPlayers.Count > 2)
        //{
        danceController.Dance(currentPlayers, player, orbitInput, orbitFollow, mainCamera);
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



    
}
