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
    public GameObject levelContainer;

    public void ReloadCurrentLevel()
    {
        if (levelContainer.transform.childCount != 1)
            Debug.Log("There should be only one level");
        else
        {
            levelContainer.transform.GetChild(0).GetComponent<Level>().FastReload();
            battleSystem.Reload();
            dataRecorder.LevelLoaded();
        }

    }

    public string Ping()
    {
        return "GameManager is found";
    }
}
