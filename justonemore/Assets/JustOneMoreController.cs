using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JustOneMoreController : MonoBehaviour {

    public Camera mainCamera;
    public OrbitFollow follow;
    public GameObject playerPrefab;

    private List<Player> deadPlayers = new List<Player>();

    private List<Player> currentPlayers = new List<Player>();
    private List<Collider> playerColliders = new List<Collider>();
    
    // Use this for initialization
    void Start () {
        playerPrefab.SetActive(false);

        SpawnPlayers();
    }

    void SpawnPlayers ()
    {
        NewPlayer(playerPrefab.transform.position);
        NewPlayer(playerPrefab.transform.position + Vector3.left);
        NewPlayer(playerPrefab.transform.position + Vector3.left * 2);
        NewPlayer(playerPrefab.transform.position - Vector3.left);
        NewPlayer(playerPrefab.transform.position - Vector3.left * 2);
        NewPlayer(playerPrefab.transform.position + Vector3.forward);
        NewPlayer(playerPrefab.transform.position + Vector3.forward * 2);
        NewPlayer(playerPrefab.transform.position - Vector3.forward);
        NewPlayer(playerPrefab.transform.position - Vector3.forward * 2);
    }

    void NewPlayer(Vector3 position)
    {
        GameObject player = Instantiate(playerPrefab, position, playerPrefab.transform.rotation);
        player.SetActive(true);

        Player newPlayerGO = player.GetComponent<Player>();
        Collider[] colliders = player.GetComponentsInChildren<Collider>();
        playerColliders.AddRange(colliders);
        currentPlayers.Add(newPlayerGO);
        follow.SetFollowTarget(newPlayerGO.followTarget);

        foreach (var currentPlayer in currentPlayers)
        {
            currentPlayer.SetIgnoreColliders(playerColliders);
        }
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
            if (deadPlayer.transform.position.y < -30)
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
        }
        else
        {
            follow.SetFollowTarget(currentPlayers.First().followTarget);
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
