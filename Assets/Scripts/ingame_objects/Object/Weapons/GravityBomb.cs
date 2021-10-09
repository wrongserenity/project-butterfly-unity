using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityBomb : Weapon
{
    // Start is called before the first frame update
    public override void Attack()
    {
        Vector3 throwCenter = GetOwner().transform.position + GetOwner().GetComponent<Player>().dir_v.normalized * GlobalVariables.gravity_bomb_throw_distance;
        foreach (Collider col in Physics.OverlapSphere(throwCenter, GlobalVariables.gravity_bomb_impact_radius))
        {
            if (col.gameObject.tag == "Enemy")
            {
                Enemy enemy = col.gameObject.GetComponent<Enemy>();
                enemy.GetImpulse(throwCenter - enemy.transform.position, GlobalVariables.gravity_bomb_impulse_force / (throwCenter - enemy.transform.position).magnitude);
            }
        }
    }

    public override void AlternateAttack()
    {
        Vector3 throwCenter = GetOwner().transform.position + GetOwner().GetComponent<Player>().dir_v.normalized * GlobalVariables.gravity_bomb_throw_distance;
        foreach (Collider col in Physics.OverlapSphere(throwCenter, GlobalVariables.gravity_bomb_impact_radius))
        {
            if (col.gameObject.tag == "Enemy")
            {
                Enemy enemy = col.gameObject.GetComponent<Enemy>();
                enemy.GetImpulse(throwCenter - enemy.transform.position, -GlobalVariables.gravity_bomb_impulse_force / (throwCenter - enemy.transform.position).magnitude);
            }
        }
    }
}
