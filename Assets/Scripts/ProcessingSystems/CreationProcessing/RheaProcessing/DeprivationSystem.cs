using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeprivationSystem : MonoBehaviour
{
    SphereCollider sphereCollider;
    Cooldown cooldown;
    GameManager gameManager;

    public bool CheckReadyEnemy()
    {
        foreach (Collider col in Physics.OverlapSphere(sphereCollider.bounds.center, sphereCollider.radius))
        {
            if (col.gameObject.tag == "Enemy")
                return true;
        }
        return false;
    }

    public void DeprivateClothestWeapon()
    {
        if (!cooldown.in_use)
        {

            List<Enemy> readyEnemies = new List<Enemy>() { };
            foreach (Collider col in Physics.OverlapSphere(sphereCollider.bounds.center, sphereCollider.radius))
            {
                if (col.gameObject.tag == "Enemy" && col.gameObject.GetComponent<Enemy>().isReadyToDeprivate)
                    readyEnemies.Add(col.gameObject.GetComponent<Enemy>());
            }


            float distance = 0f;
            int clothestEnemyIndex = -1;
            if (readyEnemies.Count > 0)
            {
                distance = readyEnemies[0].GetDistanceToPlayer();
                clothestEnemyIndex = 0;
            }
            for (int i = 1; i < readyEnemies.Count; i++)
            {
                float tempDist = readyEnemies[i].GetDistanceToPlayer();
                if (distance > tempDist)
                {
                    distance = tempDist;
                    clothestEnemyIndex = i;
                }
            }
            //Debug.Log("clothest index " + clothestEnemyIndex);
            if (clothestEnemyIndex >= 0)
            {
                readyEnemies[clothestEnemyIndex].GiveWeaponToPlayer();
                cooldown.Try();
            }
        }
    }



    // Start is called before the first frame update
    void Start()
    {
        sphereCollider = GetComponent<SphereCollider>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        cooldown = gameManager.cooldownSystem.AddCooldown(this, GlobalVariables.player_deprivation_cooldown);
    }
}
