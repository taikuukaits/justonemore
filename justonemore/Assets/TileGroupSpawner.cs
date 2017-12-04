using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGroupSpawner : PlayerTriggerable {

    public int requiredPlayers = 1;

    public override void OnPlayerTrigger(Player player)
    {

        JustOneMoreController controller = FindObjectOfType<JustOneMoreController>();
        if (controller.GetPlayerCount() >= requiredPlayers)
        {
            GetComponentInParent<TileGroup>().Spawn();

            Destroy(this.gameObject);
        }

    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        this.transform.Rotate(Vector3.up * Time.deltaTime * 320f, Space.World);
	}
}
