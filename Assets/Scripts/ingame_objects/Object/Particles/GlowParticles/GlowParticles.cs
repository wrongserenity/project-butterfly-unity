using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlowParticles : MonoBehaviour
{
    Cooldown lifeTimer;
    int iterator = 1;
    Vector3 needPos;
    public GlowingObject spawnerObject;

    GameManager gameManager;
    public Vector3 NextDestinationPosition(Vector3 playerPos)
    {
        Vector3 rotated = Quaternion.AngleAxis(-10, Vector3.up) * (transform.position - playerPos);
        needPos = playerPos + rotated * 0.95f;
        return needPos;
    }


    public Vector3 GetDirection(Vector3 playerPos)
    {
        if (iterator == 0)
        {
            NextDestinationPosition(playerPos);
            iterator = 1;
        }
        iterator--;
        return (needPos - transform.position).normalized;
    }

    private void FixedUpdate()
    {
        if (gameManager == null || lifeTimer == null)
        {
            gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            lifeTimer = gameManager.cooldownSystem.AddCooldown(this, GlobalVariables.xiton_particle_life_time);
            lifeTimer.Try();
        }
        transform.position = transform.position + 10f * Time.deltaTime * GetDirection(gameManager.player.transform.position);

        if (!lifeTimer.in_use)
        {
            spawnerObject.DeleteParticle(gameObject);
            Destroy(this.gameObject);
        }
    }
}
