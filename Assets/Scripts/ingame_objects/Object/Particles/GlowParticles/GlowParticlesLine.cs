using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlowParticlesLine : MonoBehaviour
{
    Cooldown lifeTimer;

    GameManager gameManager;



    private void FixedUpdate()
    {
        if (gameManager == null || lifeTimer == null)
        {
            gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            lifeTimer = gameManager.cooldownSystem.AddCooldown(this, 1f);
            lifeTimer.Try();
        }

        if (!lifeTimer.in_use)
        {
            Destroy(this.gameObject);
        }
    }
}
