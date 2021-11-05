using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlowingObject : MonoBehaviour
{
    Vector3 fromPlayerOffset = new Vector3(0f, 0f, 0f);
    Collider spawnCollider;
    List<GameObject> particles = new List<GameObject>() { };
    public GameObject particleObject;

    Cooldown stepTimeCharge;
    Cooldown rechargeDelay;
    float stepCharge = GlobalVariables.glowing_object_step_charge;
    float stepUsing = GlobalVariables.glowing_object_step_using;
    float curCharge = 1.0f;
    Color ownColor;

    bool isUsing = false;
    bool isBlockedToCharge = false;

    GameManager gameManager;

    int iterator = 2;

    // Start is called before the first frame update
    void Start()
    {
        spawnCollider = GetComponent<Collider>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        ownColor = GetComponent<MeshRenderer>().material.GetColor("_EmissionColor");

        stepTimeCharge = gameManager.cooldownSystem.AddCooldown(this, GlobalVariables.glowing_object_step_time_charge);
        rechargeDelay = gameManager.cooldownSystem.AddCooldown(this, GlobalVariables.glowing_object_recharge_delay);
    }

    private void FixedUpdate()
    {
        if (!gameManager.player.stateMachine.IsActive("charging"))
            isUsing = false;
        if (curCharge < 1.0 && !stepTimeCharge.in_use && !isUsing)
        {
            stepTimeCharge.Try();
            if(!rechargeDelay.in_use)
                ChargeTransfer(stepCharge);
            isUsing = false;
        }
    }


    public void DeleteParticle(GameObject go)
    {
        particles.Remove(go);
    }

    void ChargeTransfer(float value)
    {
        curCharge += value;
        GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", ownColor * curCharge);
        if (curCharge < stepUsing)
        {
            isBlockedToCharge = true;
            rechargeDelay.Update();
        }
        if (curCharge + stepCharge > 1.0f)
            isBlockedToCharge = false;
    }

    public bool SpawnParticlesRequest()
    {
        if (iterator == 0)
        {
            iterator = 3;
            if (curCharge - stepUsing > 0.0f && !isBlockedToCharge)
            {
                GameObject go = Resources.Load("Prefabs/Staff/XitonParticles") as GameObject;
                particles.Add(Instantiate(go, GetRandomPointInside(), new Quaternion(0f, 0f, 0f, 1.0f)).gameObject);
                particles[particles.Count - 1].transform.SetParent(particleObject.transform, false);
                particles[particles.Count - 1].GetComponent<GlowParticles>().spawnerObject = this;
                particles[particles.Count - 1].GetComponent<ParticleSystemRenderer>().material.SetColor("_EmissionColor", ownColor);

                
                //ParticleSystem.MainModule settings = particles[particles.Count - 1].GetComponent<ParticleSystem>().main;
                //settings.startColor = ownColor;

                if (particles.Count > 30)
                {
                    for (int i = 0; i < particles.Count - 30; i++)
                        Destroy(particles[i]);
                    particles.RemoveRange(0, particles.Count - 30);
                }
                ChargeTransfer(-stepUsing);
                isUsing = true;
            }
            else
                isUsing = false;
            
        }
        iterator--;
        return isUsing;
    }

    Vector3 GetRandomPointInside()
    {
        Vector3 center = spawnCollider.bounds.center;
        Vector3 extents = spawnCollider.bounds.extents;
        return new Vector3(Random.Range(center.x - extents.x, center.x + extents.x), Random.Range(center.y - extents.y, center.y + extents.y), Random.Range(center.z - extents.z, center.z + extents.z));
    }
}
