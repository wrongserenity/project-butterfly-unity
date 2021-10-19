using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Firethrower : Weapon
{
    ParticleSystem fireParticles;
    Cooldown cooldown;
    List<Light> lightSources = new List<Light>() { };

    float fuel = 1.0f;
    float fuel_consumption;

    AudioSource fireSound;

    bool isActivated = false;
    
    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        hitBox = GetComponent<BoxCollider>();
        damage = GlobalVariables.firethrower_damage;
        pushForce = GlobalVariables.firethrower_push_force;
        cooldown = gameManager.cooldownSystem.AddCooldown(this, GlobalVariables.firethrower_cooldown);
        fireParticles = transform.Find("Fire").GetComponent<ParticleSystem>();
        fireSound = gameObject.transform.Find("sounds").transform.Find("fire").GetComponent<AudioSource>();

        for (int i = 0; i < transform.Find("Fire").childCount; i++)
        {
            var main = transform.Find("Fire").GetChild(i).GetComponent<ParticleSystem>().main;
            main.customSimulationSpace = GetOwner().transform;
        }
        for (int i = 0; i < transform.Find("Light").childCount; i++)
        {
            lightSources.Add(transform.Find("Light").GetChild(i).GetComponent<Light>());
        }
        Deactivate();

        fuel_consumption = fuel / (GlobalVariables.firethrower_fuel_duration_sec / GlobalVariables.firethrower_cooldown);
        gameManager.player.interfaceObject.ShowAdditional();

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isActivated)
        {
            if (fuel > 0.00001)
            {
                if (cooldown.Try())
                {
                    fuel -= fuel_consumption;
                    gameManager.player.interfaceObject.BarAnimation("additional", "changed", 0f);
                    gameManager.player.interfaceObject.RefreshAdditionalData(fuel);
                    DamageAllInHitbox(true, damage);
                }
            }
            else
            {
                UntieWeapon();
                DestroyWeapon();
            }
            
        }
    }

    public override void DestroyWeapon()
    {
        base.DestroyWeapon();
        Deactivate();
        Destroy(fireParticles);
        Destroy(this);
    }

    public override Weapon UntieWeapon()
    {
        GiveWeaponTo(null);
        gameManager.player.deprivatedWeapon = null;
        gameManager.player.cur_deprivated_weapon_path = "";
        gameManager.player.interfaceObject.HideAdditional();
        return this;
    }


    public override void Attack()
    {
        isActivated = !isActivated;
        if (isActivated)
            Activate();
        else
            Deactivate();
    }

    public override void AlternateAttack()
    {
        Attack();
    }

    void Activate()
    {
        foreach (Light light in lightSources)
            light.enabled = true;
        fireParticles.Play();
        fireSound.Play();
    }

    void Deactivate()
    {
        foreach (Light light in lightSources)
            light.enabled = false;
        fireParticles.Stop();
        fireSound.Stop();
    }
}
