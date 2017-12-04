using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JustOneMoreController : MonoBehaviour {

    public DanceCameraController danceController;

    public Camera mainCamera;
    public OrbitInput orbitInput;
    public OrbitFollow orbitFollow;
    public CameraMoveToLocation cameraMove;
    public GameObject playerPrefab;

    private List<Transform> playerPositions = new List<Transform>();
    private Dictionary<Transform, Player> playerToPositions = new Dictionary<Transform, Player>();
    private Dictionary<Player, Transform> positionToPlayers = new Dictionary<Player, Transform>();
    private Dictionary<Player, Transform> activePlayerToPositions = new Dictionary<Player, Transform>();

    private Player leader;
    private List<Player> deadPlayers = new List<Player>();
    private List<Player> currentPlayers = new List<Player>();
    private List<Collider> playerColliders = new List<Collider>();

    public PlayerSpawn FirstPlayerSpawn;
    private Transform RespawnPlayerTransform;

    public int GetPlayerCount()
    {
        return currentPlayers.Count;
    }

    public void RegisterPlayerSpawn(Transform newTransform)
    {
        playerPositions.Add(newTransform);
        Player player = RespawnPlayerPosition(newTransform);
   }

    // Use this for initialization
    void Start () {
        playerPrefab.SetActive(false);

        FirstPlayerSpawn.BecomeAvailable();

        Player firstPlayer = playerToPositions[FirstPlayerSpawn.transform];

        ActivatePlayer(firstPlayer);

        RespawnPlayerTransform = FirstPlayerSpawn.transform;
        //StartCoroutine(LevelWon());
    }

    void RespawnPlayers()
    {
        foreach (var playerSpawnPosition in playerPositions)
        {
            Player player = RespawnPlayerPosition(playerSpawnPosition);
            if (playerSpawnPosition == RespawnPlayerTransform)
            {
                ActivatePlayer(player);
                //ChangeTarget();
            }
        }
    }
    
    Player RespawnPlayerPosition(Transform playerSpawnPosition)
    {
        if (!playerToPositions.ContainsKey(playerSpawnPosition) || playerToPositions[playerSpawnPosition] == null)
        {
            Player newPlayer = NewPlayer(playerSpawnPosition.position);
            playerToPositions[playerSpawnPosition] = newPlayer;
            positionToPlayers[newPlayer] = playerSpawnPosition;
            return newPlayer;
        }

        return playerToPositions[playerSpawnPosition];
    }

    public void ActivatePlayer(Player player)
    {
        if (positionToPlayers.ContainsKey(player))
        {
            var playerSpawnPosition = positionToPlayers[player];
            positionToPlayers.Remove(player);
            playerToPositions.Remove(playerSpawnPosition);
            activePlayerToPositions[player] = playerSpawnPosition;
        }

        var become = player.GetComponent<BecomePlayerOnContact>();
        if (become != null) {
            Destroy(become);
        }

        Collider[] colliders = player.GetComponentsInChildren<Collider>();
        playerColliders.AddRange(colliders);
        currentPlayers.Add(player);

        player.GetComponentInChildren<PlayerInput>().enabled = true;

        foreach (var currentPlayer in currentPlayers)
        {
            currentPlayer.SetIgnoreColliders(playerColliders);
        }

        if (leader != null) leader.DiscardLeader();
        leader = player;
        orbitFollow.SetFollowTarget(leader.followTarget);
        leader.BecomeLeader();
    }

    public void ToggleLeader()
    {
        if (leader != null) leader.DiscardLeader();

        int leaderPos = currentPlayers.IndexOf(leader);
        leaderPos++;
        if (leaderPos >= currentPlayers.Count) leaderPos = 0;
        
        leader = currentPlayers[leaderPos];
        orbitFollow.SetFollowTarget(leader.followTarget);
        leader.BecomeLeader();
    }

    Player InstantiatePlayer(Vector3 position)
    {
        GameObject newPlayerGO = Instantiate(playerPrefab, position, playerPrefab.transform.rotation);
        newPlayerGO.SetActive(true);

        Player player = newPlayerGO.GetComponent<Player>();
        return player;
        
    }

    Player NewActivePlayer(Vector3 position)
    {
        Player player = InstantiatePlayer(position);
        player.SetInputEnabled(true);
        ActivatePlayer(player);
        return player;
    }

    Player NewPlayer(Vector3 position)
    {
        Player player = InstantiatePlayer(position);
        player.SetInputEnabled(false);
        player.Scale();
        player.DiscardLeader();

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

    public void Disband()
    {
        foreach (Player player in currentPlayers)
        {
            if (leader != player)
            {
                player.SetInputEnabled(false);
                var becomePlayerOnContact = player.gameObject.AddComponent<BecomePlayerOnContact>();
                becomePlayerOnContact.playerToActivate = player;
                becomePlayerOnContact.controller = this;
            }
        }
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetButtonDown("SwitchLeader"))
        {
            ToggleLeader();
        }
        if (Input.GetButtonDown("Disband"))
        {
            Disband();
        }

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

            if (activePlayerToPositions.ContainsKey(deadPlayer))
            {
                Transform deadPlayerTransform = activePlayerToPositions[deadPlayer];
                activePlayerToPositions.Remove(deadPlayer);
                RespawnPlayerPosition(deadPlayerTransform);
            }

        }

        deadPlayers.RemoveAll(player => completelyDeadPlayers.Contains(player));
        foreach (Player deadPlayer in completelyDeadPlayers)
        {
            Destroy(deadPlayer.gameObject);
        }

        if (currentPlayers.Count == 0)
        {
            RespawnPlayers();
        }
        else
        {
            if (!currentPlayers.Contains(leader))
            {
                if (leader != null) leader.DiscardLeader();
                leader = currentPlayers.First();
                leader.BecomeLeader();
                orbitFollow.SetFollowTarget(leader.followTarget);
            }
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



    bool changingTarget = false;
    IEnumerator ChangeTarget()
    {
        if (!changingTarget)
        {
            changingTarget = true;

            orbitFollow.enabled = false;
            Vector3 offset = orbitFollow.GetCurrentOffset();
            Transform newTargetTransform = leader.followTarget;
            Vector3 newPosition = newTargetTransform.position;

            Vector3 newTarget = newPosition + offset;

            yield return StartCoroutine(cameraMove.MoveToPositionCo(newTarget));

            orbitFollow.enabled = true;
            orbitFollow.SetFollowTarget(newTargetTransform);
            changingTarget = false;
        }
    }
}
