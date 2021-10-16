using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Firethrower : Weapon
{
    ParticleSystem fireParticles;
    Cooldown cooldown;
    List<Light> lightSources = new List<Light>() { };

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
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isActivated)
        {
            if (cooldown.Try())
            {
                DamageAllInHitbox(true, damage);
            }
        }
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
    }

    void Deactivate()
    {
        foreach (Light light in lightSources)
            light.enabled = false;
        fireParticles.Stop();
    }
}
