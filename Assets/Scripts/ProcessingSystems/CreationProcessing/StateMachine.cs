using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    List<string> stateList = new List<string>() { };
    public SoundSystem soundSystem;
    List<string> availableSounds = new List<string>() { };

    // Start is called before the first frame update
    void Start()
    {
        foreach (AudioSource sound in soundSystem.GetComponentsInChildren<AudioSource>())
        {
            availableSounds.Add(sound.name);
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
