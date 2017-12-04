using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGroupSpawner : PlayerTriggerable {

    public int requiredPlayers = 1;

    public GameObject placeholder;
    public Transform iconSpawnPosition;

    public List<GameObject> icons;

    public override void OnPlayerTrigger(Player player)
    {

        JustOneMoreController controller = FindObjectOfType<JustOneMoreController>();
        if (controller.GetTeamCount() >= requiredPlayers)
        {
            GetComponentInParent<TileGroup>().Spawn(this.transform.position);

            Destroy(this.gameObject);
        }

    }

    // Use this for initialization
    void Start () {

        GameObject icon = icons[requiredPlayers - 1];
        icon.SetActive(true);
        Destroy(placeholder);

	}
	
	
}
