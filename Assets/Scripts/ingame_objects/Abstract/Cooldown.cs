using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Cooldown
{
    public Cooldown(object master_, double timeout_)
    {
        master = master_;   
        timeout = timeout_;
        last_timestamp = 0.0f;
        in_use = false;
    }
    public object master;
    public double timeout;
    public double last_timestamp;

    public bool in_use;

    public void Update()
    {
        this.last_timestamp = Time.time;
        this.in_use = true;
    }

    public void Release()
    {
        this.in_use = false;
    }

    public bool Try()
    {
        if (in_use)
        {
            return false;
        }
        else
        {
            Update();
            return true;
        }
    }
}
