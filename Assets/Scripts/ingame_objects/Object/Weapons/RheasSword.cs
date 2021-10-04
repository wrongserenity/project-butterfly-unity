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

        attack = gameObject.transform.Find("attack").GetComponent<AudioSource>();
        whoosh = gameObject.transform.Find("whoosh").GetComponent<AudioSource>();

    }

    public override void PlayerAttack()
    {
        if (cooldown.Try())
        {
            //$"attack_sprite/animation".play("player_attack")
            if (DamageAllInHitbox(true))
            {
                attack.Play();
            }
            else
            {
                whoosh.Play();
            }
        }
    }
    
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
