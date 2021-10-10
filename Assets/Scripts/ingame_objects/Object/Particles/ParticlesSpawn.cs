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

    int iterator = 10;

    // Start is called before the first frame update
    void Start()
    {
        spawnCollider = GetComponent<Collider>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        ownColor = GetComponent<MeshRenderer>().material.color;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (iterator == 0)
        {
            GameObject go = Resources.Load("Prefabs/Staff/XitonParticles") as GameObject;
            particles.Add(Instantiate(go, GetRandomPointInside(), new Quaternion(0f, 0f, 0f, 1.0f)).gameObject);
            iterator = 10;
            particles[particles.Count - 1].transform.SetParent(particleObject.transform, false);
            ParticleSystem.MainModule settings = particles[particles.Count - 1].GetComponent<ParticleSystem>().main;
            settings.startColor = ownColor;

            if (particles.Count > 300)
            {
                for(int i = 0; i < particles.Count - 300; i++)
                    Destroy(particles[i]);
                particles.RemoveRange(0, particles.Count - 300);
            }
        }
        foreach(GameObject particle in particles)
        {
            
            particle.transform.position = particle.transform.position + particle.GetComponent<GlowParticles>().GetDirection(gameManager.player.transform.position) * 5f * Time.deltaTime;
            
            //particle.GetComponent<CharacterController>().Move(5f * direction * Time.deltaTime);
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
