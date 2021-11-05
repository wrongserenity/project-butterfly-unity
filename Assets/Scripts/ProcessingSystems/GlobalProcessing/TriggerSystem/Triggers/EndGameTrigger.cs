using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGameTrigger : Trigger
{
    GameManager gameManager;
    Collider area;

    bool isActivated = false;

    void Start()
    {
        isIterative = false;

        area = GetComponent<Collider>();

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        gameManager.triggerSystem.NewTriggerToLevel(this);
    }

    public override bool CheckCondition()
    {
        Collider[] cols = Physics.OverlapBox(area.bounds.center, area.bounds.extents, area.transform.rotation);
        foreach (Collider col in cols)
        {
            if (col.tag == "Player")
            {
                Player player = col.GetComponent<Player>();
                if (player.actionObj != this)
                {
                    if (!isActivated)
                    {
                        gameManager.NextLevel();
                        isActivated = true;
                    }
                    return true;
                }

            }
        }
        return base.CheckCondition();
    }
}
