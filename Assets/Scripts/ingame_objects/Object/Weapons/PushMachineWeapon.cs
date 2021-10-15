using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushMachineWeapon : Weapon
{
    List<GameObject> attackBlocks = new List<GameObject>() { };
    Color startColor;
    int fadeCount = 0;

    AudioSource push;

    void Start()
    {
        base.Start();
        hitBox = GetComponent<BoxCollider>();
        BuildCooldownList(GlobalVariables.push_machine_weapon_cooldown);
        damage = GlobalVariables.push_machine_weapon_damage;
        pushForce = GlobalVariables.push_machine_push_force;

        deprivationWeaponPath = "Prefabs/Weapons/GravityBomb/GravityBomb";

        push = gameObject.transform.Find("sounds").transform.Find("push").GetComponent<AudioSource>();

    }

    void SetBLocksList()
    {
        GameObject blocks = GetOwner().transform.Find("Model").transform.Find("Capsule").transform.Find("blocks").gameObject;
        for (int i = 0; i < blocks.transform.childCount; i++)
        {
            attackBlocks.Add(blocks.transform.GetChild(i).gameObject);
        }
        startColor = attackBlocks[0].GetComponent<MeshRenderer>().material.color;
    }

    public override void Attack()
    {
        if (attackBlocks.Count == 0 && GetOwner().title == "pushmachine")
        {
            SetBLocksList();
        }

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

    public void PlayerAttack()
    {
        Vector3 throwCenter = GetOwner().transform.position + GetOwner().GetComponent<Player>().dir_v.normalized * GlobalVariables.gravity_bomb_throw_distance;
        foreach (Collider col in Physics.OverlapSphere(throwCenter, GlobalVariables.gravity_bomb_impact_radius))
        {
            if (col.gameObject.tag == "Enemy")
            {
                Enemy enemy = col.gameObject.GetComponent<Enemy>();
                enemy.GetImpulse(throwCenter - enemy.transform.position, GlobalVariables.gravity_bomb_impulse_force);
            }
        }
        
    }

    public override void Using()
    {
        base.Using();
        int hit = DamageAllInHitbox(false, damage);
        // $attack_sprite/animation.play("enemy_attack")
        if (hit == 3)
        {
            push.Play();
        }
        else if (hit == 2)
        {
            push.Play();
        }
        else if (hit == 1)
        {
            gameManager.player.stateMachine.AddState("parrySoundReq");
        }
        else
        {
            // $"sound/whoosh".play()
        }
    }

    public override void StateAnimate(int state)
    {
        if (state == 1)
            StartCoroutine(FadeInPushMachineWeapon(GlobalVariables.push_machine_weapon_cooldown[1] * 0.8f));
        if (state == 3)
            StartCoroutine(FadeOutPushMachineWeapon(GlobalVariables.push_machine_weapon_cooldown[3] * 0.8f));
    }


    public IEnumerator FadeOutPushMachineWeapon(float duration)
    {
        Color whiteColor = Color.white;
        ChangeColorOn(whiteColor.r, whiteColor.g, whiteColor.b, 1.0f);
        float curAlpha = 1.0f;
        float step = 1.0f / (duration / fadeStepSec);
        while (curAlpha > 0.0f)
        {
            curAlpha -= step;
            ChangeColorOn(whiteColor.r, whiteColor.g, whiteColor.b, curAlpha);
            yield return new WaitForSeconds(fadeStepSec);
        }
        ChangeColorOn(startColor.r, startColor.g, startColor.b, startColor.a);
    }

    public IEnumerator FadeInPushMachineWeapon(float duration)
    {
        Color redColor = Color.red;
        ChangeColorOn(redColor.r, redColor.g, redColor.b, 0.0f);
        float curAlpha = 0.0f;
        float step = 1.0f / (duration / fadeStepSec);
        while (curAlpha < 1.0f)
        {
            curAlpha += step;
            ChangeColorOn(redColor.r, redColor.g, redColor.b, curAlpha);
            yield return new WaitForSeconds(fadeStepSec);
        }
    }

    void ChangeColorOn(float r, float g, float b, float a)
    {
        foreach (GameObject go in attackBlocks)
        {
            Color temp = new Color(r, g, b, a);
            go.GetComponent<MeshRenderer>().material.color = temp;
            go.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", temp);
        }
    }
}

