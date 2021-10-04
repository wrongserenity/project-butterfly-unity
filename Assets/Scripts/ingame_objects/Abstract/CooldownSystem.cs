using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CooldownSystem : MonoBehaviour
{
    float cur_time;
    public List<Cooldown> candidates = new List<Cooldown>();


    public Cooldown AddCooldown(object master, double timeout_)
    {
        Cooldown cooldown = new Cooldown(master, timeout_);
        candidates.Add(cooldown);
        return cooldown;

    }

    void Update()
    {
        cur_time = Time.time;
        foreach (Cooldown cooldown in candidates)
        {
            if (cur_time - cooldown.last_timestamp > cooldown.timeout && cooldown.in_use)
            {
                cooldown.Release();
                cooldown.last_timestamp = cur_time;
            }
        }
    }

    void Start()
    {
        cur_time = 0f;

    }
}
