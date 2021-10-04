using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SamuraiSword : Weapon
{
    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        hitBox = GetComponent<BoxCollider>();
        BuildCooldownList(GlobalVariables.samurai_weapon_cooldown);
        damage = GlobalVariables.samurai_weapon_damage;
        pushForce = GlobalVariables.melee_enemy_push_force;
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
