using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlesSpawn : MonoBehaviour
{
    Vector3 fromPlayerOffset = new Vector3(0f, 0f, 0f);
    Collider spawnCollider;
    List<GameObject> particles = new List<GameObject>() { };
    public GameObject particleObject;

    Color ownColor;

    GameManager gameManager;

    int iterator = 2;

    // Start is called before the first frame update
    void Start()
    {
        spawnCollider = GetComponent<Collider>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        ownColor = GetComponent<MeshRenderer>().material.color;
    }

    // Update is called once per frame
    

    public void DeleteParticle(GameObject go)
    {
        particles.Remove(go);
    }

    public void SpawnParticlesRequest()
    {
        if (iterator == 0)
        {
            GameObject go = Resources.Load("Prefabs/Staff/XitonParticles") as GameObject;
            particles.Add(Instantiate(go, GetRandomPointInside(), new Quaternion(0f, 0f, 0f, 1.0f)).gameObject);
            iterator = 2;
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
        }
        iterator--;
    }

    Vector3 GetRandomPointInside()
    {
        Vector3 center = spawnCollider.bounds.center;
        Vector3 extents = spawnCollider.bounds.extents;
        return new Vector3(Random.Range(center.x - extents.x, center.x + extents.x), Random.Range(center.y - extents.y, center.y + extents.y), Random.Range(center.z - extents.z, center.z + extents.z));
    }
}
