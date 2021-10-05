using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RheasSword : Weapon
{
    Cooldown cooldown;
    AudioSource attack;
    AudioSource whoosh;




    void Start()
    {
        base.Start();
        hitBox = GetComponent<BoxCollider>();
        cooldown = gameManager.cooldownSystem.AddCooldown(this, GlobalVariables.player_weapon_cooldown);
        damage = GlobalVariables.player_weapon_damage;
        pushForce = GlobalVariables.player_weapon_push_force;

        attack = gameObject.transform.Find("sounds").transform.Find("attack").GetComponent<AudioSource>();
        whoosh = gameObject.transform.Find("sounds").transform.Find("whoosh").GetComponent<AudioSource>();

        attackSprite = transform.Find("sprites").transform.Find("attack_sprite").gameObject;
    }

    public override void PlayerAttack()
    {
        if (cooldown.Try())
        {       
            StartCoroutine(FadeOut(GlobalVariables.player_weapon_cooldown / 2));

            int hit = DamageAllInHitbox(true);
            if (hit == 3)
            {
                attack.Play();
            }
            else if (hit == 2)
            {
                // block attack sound
            }
            else if (hit == 1)
            {
                // parry sound
            }
            else
            {
                whoosh.Play();
            }
        }
    }
}
