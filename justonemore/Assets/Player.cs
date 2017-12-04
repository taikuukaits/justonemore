using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[System.Serializable]
public class PlayerTypeDefinition {
    public Player.PlayerType type;
    public GameObject model;
}

public class Player : MonoBehaviour {

    public GameObject leaderIcon;
    public GameObject teamIcon;
    public PlayerAnimator animator;
    public Transform followTarget;
    public Transform danceTarget;
    public PlayerCharacterController controller;
    public GameObject model;
    public DOTweenAnimation scaleAnimation;

    public PlayerInput input;

    public bool isLeader = false;
    public bool isOnTeam = false;
    public bool hasEverBeenOnTeam = false;
    bool dancing = false;

    public enum PlayerType {
        BusinessMan,
        Doctor,
        StreetMan,
        Punk,
        Woman,
        Hobo,
        Trucker
    }

    [System.NonSerialized]
    public PlayerSpawn playerSpawnBirthplace;

    public List<PlayerTypeDefinition> playerTypeDefinitions = new List<PlayerTypeDefinition>();
    public void SetPlayerType(PlayerType type)
    {
        foreach (var playerTypeDefinition in playerTypeDefinitions)
        {
            if (playerTypeDefinition.type == type)
            {
                playerTypeDefinition.model.SetActive(true);
            }
            else
            {
                playerTypeDefinition.model.SetActive(false);
            }
        }
    }
    
    // Use this for initialization
    void Start () {
        RefreshTeamAndLeader();

    }

    public void BecomeLeader()
    {
        isLeader = true;
        RefreshTeamAndLeader();
    }

    public void DiscardLeader() {
        isLeader = false;
        RefreshTeamAndLeader();
    }


    public void Scale()
    {
        model.transform.localScale = Vector3.zero;
        scaleAnimation.enabled = true;
    }

    public void Died()
    {
        animator.Died();

    }

    public void Dance()
    {
        animator.Dance();
        dancing = true;
    }
    public void StopDance()
    {
        animator.StopDance();

    }

    public void SetInputEnabled(bool enabled)
    {
        input.enabled = enabled;
        controller.SetDesiredDirection(Vector3.zero);
    }

    public void OverrideDirection(Vector3 direction)
    {
        controller.SetDesiredDirection(direction);
    }

    public void SetIgnoreColliders(List<Collider> colliders)
    {
        controller.SetIgnoreColliders(colliders);
    }

    public void JoinTeam()
    {
        hasEverBeenOnTeam = true;
        isOnTeam = true;
        RefreshTeamAndLeader();
        
    }

    public void DisbandTeam()
    {
        isOnTeam = false;
        RefreshTeamAndLeader();
        
    }

    public void RefreshTeamAndLeader()
    {
        if (!dancing)
        {
            if (isOnTeam)
            {
                animator.Idle();
            }
            else if (hasEverBeenOnTeam)
            {
                animator.ImpatientIdle();

            }
            else
            {
                animator.Idle();
            }
        }
        teamIcon.SetActive(isOnTeam && !isLeader);
        leaderIcon.SetActive(isLeader);
    }
}
