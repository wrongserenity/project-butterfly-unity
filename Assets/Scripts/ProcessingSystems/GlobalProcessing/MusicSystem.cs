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
    List<float> fixedBattleMusicParameters;
    int mostEpicEnemyAmount = GlobalVariables.default_music_max_enemy_amount;

    float curBattleMusicParameterValue = 0f;

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        battleSystem = gameManager.battleSystem;

        battleSystem.OnBattleActivityChanged += TryUpdateBattleMusicParameter;
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
        fixedBattleMusicParameters = settings.fixedBattleMusicParameters;
        mostEpicEnemyAmount = settings.mostEpicEnemyAmount;
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

        float mappedCurEnemyAmount = MapCurEnemyAmount(battleSystem.CalculateEnemiesCount());

        float newFixedParameter = mappedCurEnemyAmount.Equals(0f) ? 0f : GetFixedParameterValue(mappedCurEnemyAmount);
        if (newFixedParameter.Equals(curBattleMusicParameterValue))
            return;

        Debug.Log("Music: " + newFixedParameter + " - " + battleSystem.CalculateEnemiesCount());
        UpdateBattleMusicParameter(newFixedParameter);
    }

    private float MapCurEnemyAmount(int curEnemyAmount)
    {
        float result = 0f;


        if (mostEpicEnemyAmount != 0)
            result = (float)curEnemyAmount / (float)mostEpicEnemyAmount;

        return Mathf.Clamp(result, 0f, 1f);
    }

    private float GetFixedParameterValue(float mappedValue)
    {
        float result = 0f;
        if (mappedValue.Equals(0f) || mappedValue.Equals(1f))
            return mappedValue;

        if (fixedBattleMusicParameters.Count == 0)
            return result;

        for (int i = 1; i < fixedBattleMusicParameters.Count; i++)
        {
            if (mappedValue > fixedBattleMusicParameters[i] || mappedValue.Equals(fixedBattleMusicParameters[i]))
                continue;
            else
            {
                result = fixedBattleMusicParameters[i - 1];
                break;
            }
        }
        return result;
    }

    private void UpdateBattleMusicParameter(float newParamValue)
    {
        battleEventInstance.setParameterByName(battleMusicParameterName, newParamValue);
        curBattleMusicParameterValue = newParamValue;
    }
}
