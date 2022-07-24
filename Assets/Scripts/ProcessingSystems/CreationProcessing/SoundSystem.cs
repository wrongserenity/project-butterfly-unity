using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class SoundSystem : MonoBehaviour
{
    public List<string> playlist;
    Dictionary<string, StudioEventEmitter> emitters = new Dictionary<string, StudioEventEmitter>();


    void Start()
    {
        foreach(StudioEventEmitter emitter in GetComponentsInChildren<StudioEventEmitter>())
        {
            Debug.Log("in sound system: " + emitter.name);
            emitters.Add(emitter.name, emitter);
        }
    }

    public void AddToPlaylist(string soundName)
    {
        if (!playlist.Contains(soundName) && emitters.ContainsKey(soundName))
        {
            playlist.Add(soundName);
        }
    }

    public void RemoveFromPlaylist(string soundName)
    {
        int index = playlist.FindIndex(x => x == soundName);
        if (index >= 0)
        {
            playlist.Remove(soundName);
        }
    }

    public void Check()
    {
        foreach(KeyValuePair<string, StudioEventEmitter> emitter in emitters)
        {
            if (playlist.Contains(emitter.Key) && !emitter.Value.IsPlaying())
                emitter.Value.Play();
            else if (emitter.Value.IsPlaying() && !playlist.Contains(emitter.Key))
                emitter.Value.Stop();
        }
    }

    public void PlayOnce(string soundName)
    {
        //bool isPlayed = false;
        if (emitters.ContainsKey(soundName))
            emitters[soundName]?.Play();
        //if (!isPlayed)
        //    print("not played");
        //else
        //    print("played " + soundName);
    }

    public void UpdateParameterByName(string eventName, string parameterName, float value)
    {
        if (emitters.ContainsKey(eventName))
        {
            emitters[eventName].SetParameter(parameterName, value);
            Debug.Log("New parameter value: " + value);
        }
    }
}
