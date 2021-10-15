using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityBomb : Weapon
{
    GameObject aimSprite;
    GameObject sphere;
    bool isUsed = false;

    private void Start()
    {
        base.Start();
        GameObject go = Resources.Load("Prefabs/Weapons/GravityBomb/GravityBombAim") as GameObject;
        aimSprite = Instantiate(go, new Vector3(0f, 0f, 0f), new Quaternion(0f, 0f, 0f, 1.0f));
        //aimSprite.transform.localScale = new Vector3(GlobalVariables.gravity_bomb_impact_radius, 1f, GlobalVariables.gravity_bomb_impact_radius);
        deprivationWeaponPath = "Prefabs/Weapons/GravityBomb/GravityBomb";
    }


    private void FixedUpdate()
    {
        if (!isUsed)
        {
            Vector3 mousePoint = gameManager.player.GetVeiwPoint();
            if (mousePoint.magnitude > GlobalVariables.gravity_bomb_throw_distance)
                mousePoint = mousePoint.normalized * GlobalVariables.gravity_bomb_throw_distance + gameManager.player.transform.position;
            else
                mousePoint = mousePoint + gameManager.player.transform.position;
            aimSprite.transform.position = mousePoint;
        }
    }
    
    // 1 - in, -1 - out
    public override void Attack()
    {
        StartCoroutine(Impact(GlobalVariables.gravity_bomb_impact_duration, 1, aimSprite.transform.position));
    }

    public override void AlternateAttack()
    {
        StartCoroutine(Impact(GlobalVariables.gravity_bomb_impact_duration, -1, aimSprite.transform.position));
    }


    public IEnumerator Impact(float duration, int impactDirection, Vector3 center)
    {
        isUsed = true;
        float impactRadius = GlobalVariables.gravity_bomb_impact_radius;

        float timeStep = GlobalVariables.gravity_bomb_impact_time_step;
        float sizeStep = impactRadius / (duration / timeStep);
        float startSize = 0.5f * (1 + impactDirection) * impactRadius;
        float curDur = duration;

        UntieWeapon();

        GameObject go = Resources.Load("Prefabs/Weapons/GravityBomb/ImpactSphere") as GameObject;
        sphere = Instantiate(go, new Vector3(0f, 0f, 0f), new Quaternion(0f, 0f, 0f, 1.0f));
        sphere.transform.localScale = new Vector3(startSize * 2, startSize * 2, startSize * 2); // because sphere with size=1 have radius=0.5
        sphere.transform.position = center;
        aimSprite.transform.position = center;
        aimSprite.transform.localScale = new Vector3(impactRadius * 2, impactRadius * 2, impactRadius * 2);

        while (curDur > 0)
        {
            foreach (Collider col in Physics.OverlapSphere(center, GlobalVariables.gravity_bomb_impact_radius))
            {
                if (col.gameObject.tag == "Enemy")
                {
                    Enemy enemy = col.gameObject.GetComponent<Enemy>();
                    enemy.GetImpulse(center - enemy.transform.position, impactDirection * GlobalVariables.gravity_bomb_impulse_force / (center - enemy.transform.position).magnitude);
                }
            }
            sphere.transform.localScale = sphere.transform.localScale - impactDirection * new Vector3(sizeStep*2, sizeStep*2, sizeStep*2);
            curDur -= timeStep;
            yield return new WaitForSeconds(timeStep);
        }
        DestroyWeapon();
    }

    public override void DestroyWeapon()
    {
        base.DestroyWeapon();
        Destroy(aimSprite);
        Destroy(sphere);
        Destroy(this);
    }
    public override Weapon UntieWeapon()
    {
        GiveWeaponTo(null);
        gameManager.player.deprivatedWeapon = null;
        gameManager.player.cur_deprivated_weapon_path = "";
        return this;
    }

    public IEnumerator PhysicalImpact(float duration, int impactDirection, Vector3 center)
    {
        float step = GlobalVariables.gravity_bomb_impact_time_step;
        float curDur = duration;

        while (curDur > 0)
        {
            foreach (Collider col in Physics.OverlapSphere(center, GlobalVariables.gravity_bomb_impact_radius))
            {
                if (col.gameObject.tag == "Enemy")
                {
                    Enemy enemy = col.gameObject.GetComponent<Enemy>();
                    enemy.GetImpulse(center - enemy.transform.position, impactDirection * GlobalVariables.gravity_bomb_impulse_force / (center - enemy.transform.position).magnitude);
                }
            }
            curDur -= step;
            yield return new WaitForSeconds(step);
        }
    }

    public IEnumerator VisualEffect(float duration, int impactDirection, Vector3 center)
    {
        float impactRadius = GlobalVariables.gravity_bomb_impact_radius;

        float timeStep = GlobalVariables.gravity_bomb_impact_time_step;
        float sizeStep = impactRadius / (duration / timeStep);
        float startSize = 0.5f * (1 + impactDirection) * impactRadius;

        float curDur = duration;
        GameObject go = Resources.Load("Prefabs/Weapons/GravityBomb/ImpactSphere") as GameObject;
        GameObject sphere = Instantiate(go, new Vector3(0f, 0f, 0f), new Quaternion(0f, 0f, 0f, 1.0f));
        sphere.transform.localScale = new Vector3(startSize * 2, startSize * 2, startSize * 2); // because sphere with size=1 have radius=0.5
        sphere.transform.position = center;

        while (curDur > 0)
        {
            sphere.transform.localScale = sphere.transform.localScale - impactDirection * new Vector3(sizeStep, sizeStep, sizeStep);
            curDur -= timeStep;
            yield return new WaitForSeconds(timeStep);
        }
        Destroy(sphere);
    }
}
