using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class MusicSettings : MonoBehaviour
{
    [Header("Ambient")]
    public EventReference ambientMusicEvent;

    [Header("Battle")]
    public EventReference battleMusicEvent;
    public string battleMusicParameterName = "";
    public List<float> parameterByEnemyCount = new List<float>() { };

}
