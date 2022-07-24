using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using System;

[AddComponentMenu("FMOD Studio/FMOD Studio Event Emitter")]
public class TestClass : MonoBehaviour
{
    public bool isActive = false;

    public EventReference walkEvent;
    public string walkEventParameterName = "";
    FMOD.Studio.EventInstance walkInstance;

    public EventReference attackEvent;
    public string attackEventParameterName = "";
    FMOD.Studio.EventInstance attackInstance;

    float parameter = 0f;

    const float EPS = 0.5f;



    void Start()
    {
        if (isActive)
        {
            attackInstance = RuntimeManager.CreateInstance(attackEvent);
            attackInstance.setParameterByName(attackEventParameterName, parameter);
            attackInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));

            walkInstance = RuntimeManager.CreateInstance(walkEvent);
            walkInstance.setParameterByName(walkEventParameterName, parameter);
            walkInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
            walkInstance.start();
        }
       
    }

    void Update()
    {
        if (isActive)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                attackInstance.setParameterByName(attackEventParameterName, parameter);
                attackInstance.start();
            }

            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            if (horizontal * horizontal > EPS || vertical * vertical > EPS)
            {
                walkInstance.setParameterByName(walkEventParameterName, parameter);
                walkInstance.setPaused(false);
            }
            else
                walkInstance.setPaused(true);

            float del = Input.mouseScrollDelta.y;
            if (del*del > EPS)
            {
                parameter += Input.mouseScrollDelta.y;
                parameter = Mathf.Clamp(parameter, 0, 1);
                Debug.Log(parameter);
            }
        }
    }
}
