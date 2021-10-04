using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushMachineWeapon : Weapon
{
    void Start()
    {
        base.Start();
        hitBox = GetComponent<BoxCollider>();
        BuildCooldownList(GlobalVariables.push_machine_weapon_cooldown);
        damage = GlobalVariables.push_machine_weapon_damage;
        pushForce = GlobalVariables.push_machine_push_force;
    }

    public override void EnemyAttack()
    {
        base.EnemyAttack();
        Collider[] cols = Physics.OverlapBox(hitBox.bounds.center, hitBox.bounds.extents, hitBox.transform.rotation);
        foreach (Collider col in cols)
        {
            GameObject go = col.gameObject;
            if (go.tag == "Player")
            {
                AttackRequest();
            }
        }
    }

    public override void Using()
    {
        base.Using();
        // $attack_sprite/animation.play("enemy_attack")
        if (DamageAllInHitbox(false))
        {
            // $"sounds/attack".play()
        }
        else
        {
            // $"sound/whoosh".play()
        }

    }
}
