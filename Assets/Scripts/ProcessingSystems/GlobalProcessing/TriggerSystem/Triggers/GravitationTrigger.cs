using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravitationTrigger : Trigger
{
    GameManager gameManager;

    Collider area;
    public Vector3 direction = new Vector3(1f, 0f, 0f);

    Cooldown gravityTick;
    float pushForce = 0f;


    void Start()
    {
        isIterative = false;

        area = transform.GetComponent<Collider>();

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        gameManager.triggerSystem.NewTriggerToLevel(this);

        gravityTick = gameManager.cooldownSystem.AddCooldown(this, GlobalVariables.gravity_trigger_tick_cooldown);
        pushForce = GlobalVariables.gravity_trigger_push_force;
    }

    public override bool CheckCondition()
    {
        if (gravityTick.Try())
        {
            Collider[] cols = Physics.OverlapBox(area.bounds.center, area.bounds.extents, area.transform.rotation);
            foreach (Collider col in cols)
            {
                if (col.tag == "Player")
                {
                    Player player = col.GetComponent<Player>();
                    if (player != null)
                        player.GetImpulse(direction, pushForce);
                        return true;
                }
            }
        }
        return base.CheckCondition();
    }
}
