using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundSystem : MonoBehaviour
{
    public List<string> playlist;


    void Start()
    {
        foreach(AudioSource sound in GetComponentsInChildren<AudioSource>())
        {
            print(sound.name);        
        }
    }

    public void AddToPlaylist(string soundName)
    {
        if (!playlist.Contains(soundName))
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
        foreach(AudioSource sound in GetComponentsInChildren<AudioSource>())
        {
            if (sound.loop)
            {
                if (playlist.Contains(sound.name) && !sound.isPlaying)
                {
                    sound.Play();
                }
                if (sound.isPlaying && !playlist.Contains(sound.name))
                {
                    sound.Stop();
                }
            }
        }
    }

    public void PlayOnce(string soundName)
    {
        bool isPlayed = false;
        foreach (AudioSource sound in GetComponentsInChildren<AudioSource>())
        {
            if (sound.name == soundName)
            {
                isPlayed = true;
                sound.Play();
            }
        }
        if (!isPlayed)
            print("not played");
        else
            print("played");
    }
}
