using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SamuraiSword : Weapon
{
    AudioSource attack;
    AudioSource whoosh;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        hitBox = GetComponent<BoxCollider>();
        BuildCooldownList(GlobalVariables.samurai_weapon_cooldown);
        damage = GlobalVariables.samurai_weapon_damage;
        pushForce = GlobalVariables.melee_enemy_push_force;

        attack = gameObject.transform.Find("sounds").transform.Find("attack").GetComponent<AudioSource>();
        whoosh = gameObject.transform.Find("sounds").transform.Find("whoosh").GetComponent<AudioSource>();

        attackSprite = transform.Find("sprites").transform.Find("attack_sprite").gameObject;
    }

    public override void Attack()
    {
        base.Attack();
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
        int hit = DamageAllInHitbox(false);
        // $attack_sprite/animation.play("enemy_attack")
        if (hit == 3)
        {
            attack.Play();
        }else if(hit == 2)
        {
            // block attack sound
        }
        else if (hit == 1)
        {
            gameManager.player.stateMachine.AddState("parrySoundReq");
        }
        else
        {
            whoosh.Play();
        }

    }
}
