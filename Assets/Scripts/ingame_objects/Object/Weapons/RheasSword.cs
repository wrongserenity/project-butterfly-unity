using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RheasSword : Weapon
{
    Cooldown cooldown;
    public Cooldown parry_window_cooldown;


    GameObject blockSphere;
    public Cooldown parryDamageScaleCooldown;
    float parryDamageScale = GlobalVariables.player_weapon_parry_scaling;


    AudioSource attack;
    AudioSource whoosh;
    AudioSource attackXiton;

    List<Color> particlesColors = new List<Color>() { };

    int xitonDamageScalingCost = GlobalVariables.player_weapon_xiton_damage_scaling_cost;
    float xitonDamageScaling = GlobalVariables.player_weapon_xiton_scaling;

    GameObject particleObject;

    Gradient gradient;
    GradientColorKey[] colorKey;
    GradientAlphaKey[] alphaKey;
        


    void Start()
    {
        base.Start();
        hitBox = GetComponent<BoxCollider>();
        cooldown = gameManager.cooldownSystem.AddCooldown(this, GlobalVariables.player_weapon_cooldown);
        damage = GlobalVariables.player_weapon_damage;
        pushForce = GlobalVariables.player_weapon_push_force;

        attack = gameObject.transform.Find("sounds").transform.Find("attack").GetComponent<AudioSource>();
        whoosh = gameObject.transform.Find("sounds").transform.Find("whoosh").GetComponent<AudioSource>();
        attackXiton = gameObject.transform.Find("sounds").transform.Find("attackXiton").GetComponent<AudioSource>();

        attackSprite = transform.Find("sprites").transform.Find("attack_sprite").gameObject;

        blockSphere = transform.Find("BlockSphere").gameObject;
        parry_window_cooldown = gameManager.cooldownSystem.AddCooldown(this, GlobalVariables.player_parry_window_duration);


        particleObject = transform.Find("particlesContainer").gameObject;
        particlesColors.Add(Color.yellow);

        parryDamageScaleCooldown = gameManager.cooldownSystem.AddCooldown(this, GlobalVariables.player_parry_damage_boost_time);




    }

    public override void Attack()
    {
        if (cooldown.Try())
        {
            int hit;
            bool isXitonHit = false;
            StartCoroutine(FadeOut(GlobalVariables.player_weapon_cooldown / 2));
            if (GetOwner().tag == "Player" && GetOwner().GetComponent<Player>().curXitonCharge > xitonDamageScalingCost)
            {
                hit = DamageAllInHitbox(true, Mathf.FloorToInt(damage * xitonDamageScaling * (parryDamageScaleCooldown.in_use ? parryDamageScale : 1)));
                if (hit > 0)
                {
                    GetOwner().GetComponent<Player>().XitonTransfer(-xitonDamageScalingCost);
                    isXitonHit = true;
                }
            }
            else
                hit = DamageAllInHitbox(true, Mathf.FloorToInt(damage * (parryDamageScaleCooldown.in_use ? parryDamageScale : 1)));

            if (hit == 3)
            {
                if (isXitonHit)
                {
                    attackXiton.Play();
                }
                ParticlesSpawn(2 * ((parryDamageScaleCooldown.in_use ? parryDamageScale : 1) - (isXitonHit ? 0 : 1)));
                attack.Play();

                if (parryDamageScaleCooldown.in_use)
                {
                    gameManager.dataRecorder.AddTo("parry_damage", Mathf.FloorToInt(damage * (parryDamageScale - 1)));
                    gameManager.CancelTimeScaleFor();
                }
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

    public override void AlternateAttack()
    {
        if (GetOwner().tag == "Player")
        {
            if (parry_window_cooldown.Try())
            {
                GetOwner().GetComponent<Player>().stateMachine.AddState("blockSoundReq");
                GetOwner().GetComponent<Player>().stateMachine.AddState("parrying");
            }
            else if (!GetOwner().GetComponent<Player>().stateMachine.IsActive("blocking"))
            {
                GetOwner().GetComponent<Player>().stateMachine.AddState("blocking");
            }
            blockSphere.GetComponent<MeshRenderer>().enabled = true;
        }
    }

    public void FixedUpdate()
    {
        if (GetOwner().tag == "Player")
        {
            if (!GetOwner().GetComponent<Player>().stateMachine.IsActive("parrying") && !GetOwner().GetComponent<Player>().stateMachine.IsActive("blocking"))
            {
                blockSphere.GetComponent<MeshRenderer>().enabled = false;

            }
        }
    }

    public void ParticlesSpawn(float multiplier)
    {
        for (int i = 0; i < 10*multiplier; i++)
        {
            GameObject go = Resources.Load("Prefabs/Staff/XitonParticleLine") as GameObject;
            GameObject particle = Instantiate(go, particleObject.transform.position, particleObject.transform.rotation).gameObject;
            particle.GetComponent<ParticleSystemRenderer>().material.SetColor("_EmissionColor", particlesColors[Random.Range(0, particlesColors.Count)]);
        }
    }
}
