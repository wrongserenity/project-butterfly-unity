using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeprivationSystem : MonoBehaviour
{
    SphereCollider sphereCollider;

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
        print("deprivating...");
        List<Enemy> readyEnemies = new List<Enemy>() { };
        foreach (Collider col in Physics.OverlapSphere(sphereCollider.bounds.center, sphereCollider.radius))
        {
            if (col.gameObject.tag == "Enemy" && col.gameObject.GetComponent<Enemy>().isReadyToDeprivate)
                readyEnemies.Add(col.gameObject.GetComponent<Enemy>());
        }
        print("ready enemy count: " + readyEnemies.Count);
        
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
        print("clothest index " + clothestEnemyIndex);
        if (clothestEnemyIndex >= 0) 
            readyEnemies[clothestEnemyIndex].GiveWeaponToPlayer();
    }



    // Start is called before the first frame update
    void Start()
    {
        sphereCollider = GetComponent<SphereCollider>();
    }
}
