using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class MusicSystem : MonoBehaviour
{
    GameManager gameManager;
    BattleSystem battleSystem;

    FMOD.Studio.EventInstance ambientEventInstance;

    FMOD.Studio.EventInstance battleEventInstance;
    string battleMusicParameterName = "";
    List<float> parameterByEnemyCount;

    float curBattleMusicParameterValue = 0f;

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        battleSystem = gameManager.battleSystem;

        battleSystem.OnBattleActivityChanged += TryUpdateBattleMusicParameter;
    }

    public float GetCurBattleMusicParameterValue()
    {
        return curBattleMusicParameterValue;
    }

    public void UpdateMusicSettings(MusicSettings settings)
    {
        TryImmediatelyStopInstance(ambientEventInstance);
        ambientEventInstance = RuntimeManager.CreateInstance(settings.ambientMusicEvent);
        ambientEventInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));

        TryImmediatelyStopInstance(battleEventInstance);
        battleEventInstance = RuntimeManager.CreateInstance(settings.battleMusicEvent);
        battleEventInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));

        battleMusicParameterName = settings.battleMusicParameterName;
        parameterByEnemyCount = settings.parameterByEnemyCount;
    }

    public void StartAllMusic()
    {
        ambientEventInstance.start();
        battleEventInstance.start();
    }

    void TryImmediatelyStopInstance(FMOD.Studio.EventInstance instance)
    {
        if (instance.isValid())
            instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }

    private void TryUpdateBattleMusicParameter()
    {
        if (!battleEventInstance.isValid())
            return;

        int enemyCount = battleSystem.CalculateEnemiesCount();
        float newParameter = 0f;

        int listSize = parameterByEnemyCount.Count;
        if (enemyCount < listSize)
            newParameter = parameterByEnemyCount[enemyCount];
        else if (listSize > 0)
            newParameter = parameterByEnemyCount[listSize - 1];

        if (newParameter.Equals(curBattleMusicParameterValue))
            return;

        Debug.Log("Music: " + newParameter + " - " + enemyCount);
        UpdateBattleMusicParameter(newParameter);
    }

    private void UpdateBattleMusicParameter(float newParamValue)
    {
        battleEventInstance.setParameterByName(battleMusicParameterName, newParamValue);
        curBattleMusicParameterValue = newParamValue;
    }
}
