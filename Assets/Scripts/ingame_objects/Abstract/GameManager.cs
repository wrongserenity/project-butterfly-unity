using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public CooldownSystem cooldownSystem;
    public DataRecorder dataRecorder;
    public Player player;
    public CameraBehaviour mainCamera;
    public BattleSystem battleSystem;
    public TriggerSystem triggerSystem;
    public MusicSystem musicSystem;

    public string Ping()
    {
        return "GameManager is found";
    }
}
