using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointTrigger : Trigger
{
    GameManager gameManager;
    Collider area;
    bool isTriggered = false;

    // Start is called before the first frame update
    void Start()
    {
        isIterative = false;

        area = GetComponent<Collider>();

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        gameManager.triggerSystem.NewTriggerToLevel(this);
    }

    public override bool CheckCondition()
    {
        if (!isTriggered)
        {
            Collider[] cols = Physics.OverlapBox(area.bounds.center, area.bounds.extents, area.transform.rotation);
            foreach (Collider col in cols)
            {
                if (col.tag == "Player")
                {
                    Player player = col.GetComponent<Player>();
                    if (player.actionObj != this)
                    {
                        isTriggered = true;
                        gameManager.UpdateCheckPoint(area.bounds.center);
                        return true;
                    }

                }
            }
        }
        return base.CheckCondition();
    }

    public override void ReloadTrigged()
    {
        isTriggered = false;
    }
}
