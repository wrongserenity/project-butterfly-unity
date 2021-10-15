using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraBehaviour : MonoBehaviour
{
    public GameManager gameManager;

    float speed_vel = GlobalVariables.camera_speed;
    float shaking_duration = GlobalVariables.camera_shaking_duration;

    float LinearCoef = GlobalVariables.LINEAR_COEF;
    Vector3 vel = new Vector3(0f, 0f, 0f);
    Cooldown shaking;

    float shaking_amplitude = GlobalVariables.camera_shaking_amplitude;
    float force_soft_coef = GlobalVariables.camera_force_soft_coef;
    bool is_shaking = false;

    float distance_offset = GlobalVariables.camera_distance_offset;
    float critical_distance = GlobalVariables.camera_critical_distance;

    Player player;
    public CharacterController controller;


    Volume volume;

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        shaking = gameManager.cooldownSystem.AddCooldown(this, GlobalVariables.camera_shaking_duration);
        player = gameManager.player;
        volume = GetComponent<Volume>();
    }

    void FixedUpdate()
    {

        if (is_shaking)
        {
            ShakingProcessing();
            if (!shaking.in_use)
            {
                is_shaking = false;
                shaking_amplitude = 0;
            }
        }

        Vector3 player_pos = player.transform.position;
        player_pos.y += distance_offset;


        if ((player_pos - transform.position).magnitude < critical_distance)
        {
            Vector3 movement_direction = (player_pos - transform.position) * speed_vel;
            vel += (movement_direction - vel) * LinearCoef;
            controller.Move(vel * Time.unscaledDeltaTime);
        }
        else
        {
            transform.position = player_pos;
        }
    }

    public void Shake(float force)
    {
        if (force * force_soft_coef > shaking_amplitude)
        {
            shaking_amplitude = force * force_soft_coef;
        }
        shaking.Try();
        is_shaking = true;
    }

    void ShakingProcessing()
    {
        vel += new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * shaking_amplitude;
        shaking_amplitude *= LinearCoef;
    }

    public void ChangeCrhomaticAberrationIntencity(float value)
    {
        ChromaticAberration ChrAberration;
        VolumeProfile profile = GetComponent<Volume>().profile;
        profile.TryGet(out ChrAberration);
        ChrAberration.intensity.Override(value);
    }

    public void ChangeChromaticAberrationIntencityFor(float value, float duration)
    {
        StartCoroutine(ChChAbInFor(value, duration));
    }

    IEnumerator ChChAbInFor(float value, float duration)
    {
        ChangeCrhomaticAberrationIntencity(value);
        yield return new WaitForSeconds(duration);
        ChangeCrhomaticAberrationIntencity(0f);
    }
}

