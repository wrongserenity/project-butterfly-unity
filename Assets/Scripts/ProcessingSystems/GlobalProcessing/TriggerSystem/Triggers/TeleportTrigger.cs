using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportTrigger : Trigger
{
    GameManager gameManager;
    List<Collider> portals = new List<Collider>() {null, null };

    Cooldown cooldown;

    
    void Start()
    {
        isIterative = false;

        if (portals[0] == null)
            portals[0] = gameObject.transform.Find("first").GetComponent<BoxCollider>();
        if (portals[1] == null)
            portals[1] = gameObject.transform.Find("second").GetComponent<BoxCollider>();

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        gameManager.triggerSystem.NewTriggerToLevel(this);
    }

    public override bool CheckCondition()
    {
        for (int i = 0; i < 2; i++)
        {
            Collider[] cols = Physics.OverlapBox(portals[i].bounds.center, portals[i].bounds.extents, portals[i].transform.rotation);
            foreach (Collider col in cols)
            {
                if (col.tag == "Player")
                {
                    Player player = col.GetComponent<Player>();
                    if (player.actionObj != this)
                        player.ChangeUseMode(this);
                    return true;
                }
            }
        }
        return base.CheckCondition();
    }

    public override void Activate()
    {
        for (int i = 0; i < 2; i++)
        {
            Collider[] cols = Physics.OverlapBox(portals[i].bounds.center, portals[i].bounds.extents, portals[i].transform.rotation);
            foreach (Collider col in cols)
            {
                if (col.tag == "Player")
                {
                    Player player = col.GetComponent<Player>();
                    player.teleportTriggerRequest.Add(portals[(i + 1) % 2].bounds.center);
                }
            }
        }
    }

}
