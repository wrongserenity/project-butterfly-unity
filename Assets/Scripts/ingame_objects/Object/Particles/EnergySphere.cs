using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergySphere : Trigger
{
    GameManager gameManager;
    Player player;
    AudioSource collectSound;

    Cooldown lifeTime;
    Cooldown noMoveTime;

    bool isActivated = false;
    bool isDestroyed = false;

    float maxDistance = GlobalVariables.energy_sphere_max_collect_distance;
    float minDistance = GlobalVariables.energy_sphere_min_collect_distance;
    float speed = GlobalVariables.energy_sphere_speed;

    int energy = GlobalVariables.enemy_power_price;

    // Start is called before the first frame update
    void Start()
    {
        isIterative = false;

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        player = gameManager.player;
        gameManager.triggerSystem.NewTriggerToLevel(this);

        lifeTime = gameManager.cooldownSystem.AddCooldown(this, GlobalVariables.energy_sphere_life_time);
        noMoveTime = gameManager.cooldownSystem.AddCooldown(this, GlobalVariables.energy_sphere_no_move_time * Random.Range(0.8f, 1.2f));

        collectSound = transform.Find("Sounds").Find("collectSound").GetComponent<AudioSource>();

        lifeTime.Try();
        noMoveTime.Try();
    }

    public override bool CheckCondition()
    {
        if (!noMoveTime.in_use && GetDistanceToPlayer() < maxDistance)
        {
            isActivated = true;
            return true;
        }
        return base.CheckCondition();
    }

    float GetDistanceToPlayer()
    {
        return (transform.position - player.transform.position).magnitude;
    }

    public void FixedUpdate()
    {
        if (isActivated && !isDestroyed)
        {
            MoveToPlayer();
            if(GetDistanceToPlayer() <= minDistance)
            {
                player.EnergyTransfer(energy);
                collectSound.Play();
                StartCoroutine(selfDestroy());
            }
        }
    }

    void MoveToPlayer()
    {
        transform.position += (player.transform.position - transform.position).normalized * Time.deltaTime * speed;
    }

    IEnumerator selfDestroy()
    {
        isDestroyed = true;
        gameObject.transform.Find("Sphere").GetComponent<MeshRenderer>().enabled = false;
        gameManager.triggerSystem.Delete(this);
        yield return new WaitForSeconds(collectSound.clip.length);  
        Destroy(gameObject);
    }
}
