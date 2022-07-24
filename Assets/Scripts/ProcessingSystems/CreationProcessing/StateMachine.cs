using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    List<string> stateList = new List<string>() { };

    Hashtable environmentConditions = new Hashtable();
    public static string REVERBERATION_PARAMETER_NAME = "reverberation";
    public static float REVERBERATION_DEFAULT_VALUE = 0f;


    public SoundSystem soundSystem;
    List<string> availableSounds = new List<string>() { };

    // Start is called before the first frame update
    void Start()
    {
        foreach (AudioSource sound in soundSystem.GetComponentsInChildren<AudioSource>())
        {
            availableSounds.Add(sound.name);
        }

        InitDefaultEnvironmentConditions();
    }

    void InitDefaultEnvironmentConditions()
    {
        environmentConditions.Clear();
        environmentConditions.Add(REVERBERATION_PARAMETER_NAME, REVERBERATION_DEFAULT_VALUE);
    }

    public void SetEnvironmentConditionValue(string parameterName, object value=null)
    {
        if (parameterName == REVERBERATION_PARAMETER_NAME)
        {
            object temp_value = value ?? REVERBERATION_DEFAULT_VALUE;
            if (temp_value != environmentConditions[parameterName])
            {
                environmentConditions[parameterName] = temp_value;
                soundSystem.UpdateParameterByName("walking", "Room", (float)temp_value);
            }
        }
    }

    public void AddState(string state)
    {
        if (!stateList.Contains(state))
        {
            stateList.Add(state);
            if (availableSounds.Contains(state))
            {
                soundSystem.AddToPlaylist(state);
            }
        }
    }

    public void RemoveState(string state)
    {
        int index = stateList.FindIndex(x => x == state);
        if (index >= 0)
        {
            stateList.Remove(state);
            if (availableSounds.Contains(state))
            {
                soundSystem.RemoveFromPlaylist(state);
            }
        }
    }

    public bool IsActive(string state)
    {
        return stateList.Contains(state);
    }
}
