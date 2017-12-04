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

    private List<PlayerSpawn> playerSpawns = new List<PlayerSpawn>();

    private Player leader;
    private List<Player> currentTeam = new List<Player>();

    private List<Player> currentPlayers = new List<Player>();
    private List<Player> deadPlayers = new List<Player>();
    private List<Collider> playerColliders = new List<Collider>();

    public PlayerSpawn FirstPlayerSpawn;
    private PlayerSpawn LeaderPlayerSpawn;

    public int GetTeamCount()
    {
        return currentTeam.Count;
    }

    public void RegisterPlayerSpawn(PlayerSpawn newSpawn)
    {
        playerSpawns.Add(newSpawn);
        Player player = RespawnPlayerSpawn(newSpawn);
   }

    // Use this for initialization
    void Start () {
        playerPrefab.SetActive(false);

        FirstPlayerSpawn.BecomeAvailable();

        Player firstPlayer = FirstPlayerSpawn.lastSpawnedPlayer;

        JoinPlayer(firstPlayer);

        LeaderPlayerSpawn = FirstPlayerSpawn;
        //StartCoroutine(LevelWon());
    }

    void RespawnPlayers()
    {
        foreach (var playerSpawn in playerSpawns)
        {
            Player player = RespawnPlayerSpawn(playerSpawn);
            if (playerSpawn == LeaderPlayerSpawn)
            {
                JoinPlayer(player);
                //ChangeTarget();
            }
        }
    }
    
    Player RespawnPlayerSpawn(PlayerSpawn playerSpawn)
    {
        if (playerSpawn.lastSpawnedPlayer == null)
        {
            Player newPlayer = NewPlayer(playerSpawn.transform.position);
            newPlayer.playerSpawnBirthplace = playerSpawn;
            playerSpawn.lastSpawnedPlayer = newPlayer;
            newPlayer.SetPlayerType(playerSpawn.playerType);
            currentPlayers.Add(newPlayer);
            return newPlayer;
        }

        return playerSpawn.lastSpawnedPlayer;
    }

    public void JoinPlayer(Player player)
    {
        if (currentTeam.Contains(player)) return;

        var become = player.GetComponent<BecomePlayerOnContact>();
        if (become != null) {
            Destroy(become);
        }

        Collider[] colliders = player.GetComponentsInChildren<Collider>();
        playerColliders.AddRange(colliders);
        currentTeam.Add(player);

        player.GetComponentInChildren<PlayerInput>().enabled = true;

        foreach (var currentPlayer in currentPlayers)
        {
            currentPlayer.SetIgnoreColliders(playerColliders);
        }

        player.JoinTeam();

        if (leader == null)
        {
            leader = player;
            LeaderPlayerSpawn = leader.playerSpawnBirthplace;
            orbitFollow.SetFollowTarget(leader.followTarget);
            leader.BecomeLeader();
        }
    }

    public void ToggleLeader()
    {
        if (leader != null) leader.DiscardLeader();

        int leaderPos = currentTeam.IndexOf(leader);
        if (leaderPos == -1) Debug.Log("WHAT");
        leaderPos++;
        if (leaderPos >= currentTeam.Count) leaderPos = 0;
        
        leader = currentTeam[leaderPos];
        LeaderPlayerSpawn = leader.playerSpawnBirthplace;
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
        danceController.Dance(currentTeam, player, orbitInput, orbitFollow, mainCamera);
        //}
    }

    public void Disband()
    {
        foreach (Player player in currentTeam)
        {
            if (leader != player)
            {
                player.SetInputEnabled(false);
                var becomePlayerOnContact = player.gameObject.AddComponent<BecomePlayerOnContact>();
                becomePlayerOnContact.playerToActivate = player;
                becomePlayerOnContact.controller = this;
                player.DisbandTeam();
            }
        }

        currentTeam.Clear();
        currentTeam.Add(leader);
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

        foreach (var deadPlayer in newlyDeadPlayers)
        {
            PlayerSpawn playerSpawn = deadPlayer.playerSpawnBirthplace;
            playerSpawn.lastSpawnedPlayer = null;
            deadPlayer.playerSpawnBirthplace = null;
            RespawnPlayerSpawn(playerSpawn);
        }

        currentTeam.RemoveAll(player => newlyDeadPlayers.Contains(player));
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

        if (currentTeam.Count == 0)
        {
            RespawnPlayers();
        }
        else
        {
            if (!currentTeam.Contains(leader))
            {
                if (leader != null) leader.DiscardLeader();
                leader = currentTeam.First();
                LeaderPlayerSpawn = leader.playerSpawnBirthplace;
                leader.BecomeLeader();
                orbitFollow.SetFollowTarget(leader.followTarget);
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            foreach (var currentPlayer in currentTeam)
            {
                currentPlayer.SetInputEnabled(false);
            }

        }
        if (Input.GetMouseButtonUp(1))
        {
            foreach (var currentPlayer in currentTeam)
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

            foreach (var currentPlayer in currentTeam)
            {
                float x = currentPlayer.transform.position.x;
                float z = currentPlayer.transform.position.z;
                left = Mathf.Min(x, left);
                right = Mathf.Max(x, right);
                up = Mathf.Max(z, up);
                down = Mathf.Min(z, down);
            }

            Vector3 center = new Vector3(left + ((right - left) / 2), 0, down + ((up - down) / 2));

            foreach (var currentPlayer in currentTeam)
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
