using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityBomb : Weapon
{
    GameObject aimSprite;

    private void Start()
    {
        base.Start();
        GameObject go = Resources.Load("Prefabs/Weapons/GravityBomb/GravityBombAim") as GameObject;
        aimSprite = Instantiate(go, new Vector3(0f, 0f, 0f), new Quaternion(0f, 0f, 0f, 1.0f));
        //aimSprite.transform.localScale = new Vector3(GlobalVariables.gravity_bomb_impact_radius, 1f, GlobalVariables.gravity_bomb_impact_radius);
    }


    private void FixedUpdate()
    {
        Vector3 mousePoint = gameManager.player.GetVeiwPoint();
        if (mousePoint.magnitude > GlobalVariables.gravity_bomb_throw_distance)
            mousePoint = mousePoint.normalized * GlobalVariables.gravity_bomb_throw_distance + gameManager.player.transform.position;
        else
            mousePoint = mousePoint + gameManager.player.transform.position;
        aimSprite.transform.position = mousePoint;
    }
    // Start is called before the first frame update
    public override void Attack()
    {
        Impact(1);
    }

    public override void AlternateAttack()
    {
        Impact(-1);
    }

    public void Impact(int impactDirection)
    {
        Vector3 center = aimSprite.transform.position;
        foreach (Collider col in Physics.OverlapSphere(center, GlobalVariables.gravity_bomb_impact_radius))
        {
            if (col.gameObject.tag == "Enemy")
            {
                Enemy enemy = col.gameObject.GetComponent<Enemy>();
                enemy.GetImpulse(center - enemy.transform.position, impactDirection * GlobalVariables.gravity_bomb_impulse_force / (center - enemy.transform.position).magnitude);
            }
        }
    }

    public void VisualEffect()
    {

    }
}
