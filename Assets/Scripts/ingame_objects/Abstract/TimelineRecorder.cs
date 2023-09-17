using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class TimelineRecorder : MonoBehaviour
{
    [SerializeField] bool isNeedCurEmotionUpdate = true;

    [SerializeField] int timelineRecordingRate = 30;
    [SerializeField] string saveFilePath = "Assets/Recorded/";
    [SerializeField] string recordedFileName = "filename";
    [SerializeField] string fileType = ".csv";

    [SerializeField] Player player;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] MusicSystem musicSystem;

    [SerializeField] PythonConnector pythonConnector;

    List<TimelineElement> timelineList = new List<TimelineElement>() { };
    TimelineElement curTimelineElement = new TimelineElement();
    float timelineRecordStartTime = 0f;

    float timeFromLastTimelineRecordingUpdate = 0f;
    float timelineRecordingUpdateTime = 0.33f;

    TimelineActionData actionData = new TimelineActionData();

    bool isRecording = false;

    public void StartRecordingWithParameters(float updateDeltaTime, float startTime, string dataFileName)
    {
        timelineRecordingUpdateTime = updateDeltaTime;
        timelineRecordStartTime = startTime;

        recordedFileName = dataFileName;

        player.OnCreationDamaged += () => UpdateActionTime("damaged");
        player.OnPlayerAttack += () => UpdateActionTime("attack");
        player.OnCreationDeath += () => UpdateActionTime("death");

        isRecording = true;
    }

    public void StopRecordingAndSave(GameManager gameManager)
    {
        isRecording = false;

        player.OnCreationDamaged -= () => UpdateActionTime("damaged");
        player.OnPlayerAttack -= () => UpdateActionTime("attack");
        player.OnCreationDeath -= () => UpdateActionTime("death");

        ParseAndSaveTimeline(saveFilePath + recordedFileName + fileType);
    }

    public void Process()
    {
        if (IsNeedToUpdateTimeline())
        {
            UpdateTimeline();

            if (isNeedCurEmotionUpdate)
                UpdatePythonConnector();
        }
    }

    bool IsNeedToUpdateTimeline()
    {
        timeFromLastTimelineRecordingUpdate += Time.deltaTime;
        if (timeFromLastTimelineRecordingUpdate > timelineRecordingUpdateTime)
        {
            timeFromLastTimelineRecordingUpdate -= timelineRecordingUpdateTime;
            return true;
        }

        return false;
    }

    void UpdateTimeline()
    {
        UpdateCurTimelineElement();

        timelineList.Add(curTimelineElement.Copy());
        curTimelineElement.Clear();
    }

    void UpdatePythonConnector()
    {
        pythonConnector.RequestMessageSend(timelineList[^1].Parse());
    }

    void UpdateCurTimelineElement()
    {
        curTimelineElement.timeline = Time.time - timelineRecordStartTime;

        curTimelineElement.inputDirMove = player.GetCurrentMovementInputDirection();
        curTimelineElement.inputDirView = player.GetCurrentViewInputDirection(player.GetCurPointToLook());

        curTimelineElement.curHp = player.cur_hp;
        curTimelineElement.curEnergy = player.cur_energy;
        curTimelineElement.curXiton = player.curXitonCharge;

        curTimelineElement.curEnemiesCountClose = battleSystem.GetFirstLineEnemiesCount();
        curTimelineElement.curEnemiesCountDistant = battleSystem.CalculateEnemiesCount();
        curTimelineElement.curBattleMusicParameterValue = musicSystem.GetCurBattleMusicParameterValue();

        curTimelineElement.timeFromLastActionData = actionData.SubAllVariables(Time.time);
    }

    public void UpdateActionTime(string actionName)
    {
        if (isRecording)
            actionData.Set(actionName, Time.time);
    }

    void ParseAndSaveTimeline(string filePath)
    {
        using (StreamWriter writer = new StreamWriter(filePath, false))
        {
            foreach (string line in ParceTimeline())
            {
                writer.WriteLine(line);
            }
        }
    }

    List<string> ParceTimeline()
    {
        List<string> parcedList = new List<string>() { };
        List<Dictionary<string, float>> parsedDicts = new List<Dictionary<string, float>>() { };

        foreach (TimelineElement element in timelineList)
        {
            parsedDicts.Add(element.Parse());
        }

        List<string> parameterNames = new List<string>(parsedDicts[0].Keys);
        parcedList.Add(string.Join(",", parameterNames));
        foreach (Dictionary<string, float> parsedDict in parsedDicts)
        {
            parcedList.Add(GetJoinedListWithOrder(parsedDict, parameterNames));
        }

        return parcedList;
    }

    string GetJoinedListWithOrder(Dictionary<string, float> dict, List<string> order)
    {
        List<string> orderedVariables = new List<string>() { };
        foreach (string name in order)
        {
            orderedVariables.Add(dict.ContainsKey(name) ? dict[name].ToString("0.000000").Replace(",", ".") : "");
        }
        return string.Join(",", orderedVariables);
    }
}

static class TimelineConst
{
    public const float DEFAULT_EMPTY_FLOAT = -1f;
    public const int DEFAULT_EMPTY_INT = -1;
}

class TimelineElement
{
    public float timeline = TimelineConst.DEFAULT_EMPTY_FLOAT;

    public Vector3 inputDirMove;
    public Vector3 inputDirView;

    public float curHp = TimelineConst.DEFAULT_EMPTY_FLOAT;
    public float curEnergy = TimelineConst.DEFAULT_EMPTY_FLOAT;
    public float curXiton = TimelineConst.DEFAULT_EMPTY_FLOAT;

    public int curEnemiesCountClose = TimelineConst.DEFAULT_EMPTY_INT;
    public int curEnemiesCountDistant = TimelineConst.DEFAULT_EMPTY_INT;
    public float curBattleMusicParameterValue = TimelineConst.DEFAULT_EMPTY_FLOAT;

    public TimelineActionData timeFromLastActionData = new TimelineActionData();

    public TimelineElement Copy()
    {
        TimelineElement copy = new TimelineElement();

        copy.timeline = this.timeline;

        copy.inputDirMove = this.inputDirMove;
        copy.inputDirView = this.inputDirView;

        copy.curHp = this.curHp;
        copy.curEnergy = this.curEnergy;
        copy.curXiton = this.curXiton;

        copy.curEnemiesCountClose = this.curEnemiesCountClose;
        copy.curEnemiesCountDistant = this.curEnemiesCountDistant;
        copy.curBattleMusicParameterValue = this.curBattleMusicParameterValue;

        copy.timeFromLastActionData = this.timeFromLastActionData.Copy();

        return copy;
    }

    public void Clear()
    {
        timeline = TimelineConst.DEFAULT_EMPTY_FLOAT;

        inputDirMove = Vector3.zero;
        inputDirView = Vector3.zero;

        curHp = TimelineConst.DEFAULT_EMPTY_FLOAT;
        curEnergy = TimelineConst.DEFAULT_EMPTY_FLOAT;
        curXiton = TimelineConst.DEFAULT_EMPTY_FLOAT;

        curEnemiesCountClose = TimelineConst.DEFAULT_EMPTY_INT;
        curEnemiesCountDistant = TimelineConst.DEFAULT_EMPTY_INT;
        curBattleMusicParameterValue = TimelineConst.DEFAULT_EMPTY_FLOAT;
    }

    public Dictionary<string, float> Parse()
    {
        Dictionary<string, float> parsed = new Dictionary<string, float>() { };

        parsed.Add("timeline", timeline);

        parsed.Add("inputDirMoveX", inputDirMove.x);
        parsed.Add("inputDirMoveZ", inputDirMove.z);

        parsed.Add("inputDirViewX", inputDirView.x);
        parsed.Add("inputDirViewZ", inputDirView.z);


        parsed.Add("curHP", curHp);
        parsed.Add("curEnergy", curEnergy);
        parsed.Add("curXiton", curXiton);

        parsed.Add("curEnemiesCountClose", (float)curEnemiesCountClose);
        parsed.Add("curEnemiesCountDistant", (float)curEnemiesCountDistant);
        parsed.Add("curBattleMusicParameterValue", curBattleMusicParameterValue);

        foreach (KeyValuePair<string, float> pair in timeFromLastActionData.ParseWithPRefix("timeFromLast"))
        {
            parsed.Add(pair.Key, pair.Value);
        }

        return parsed;
    }
}

class TimelineActionData
{
    Dictionary<string, float> actions = new Dictionary<string, float>() { };

    public TimelineActionData(Dictionary<string, float> actions_ = null)
    {
        if (actions_ != null)
        {
            this.actions = new Dictionary<string, float>(actions_);
            return;
        }

        actions.Add("attack", TimelineConst.DEFAULT_EMPTY_FLOAT);
        actions.Add("damaged", TimelineConst.DEFAULT_EMPTY_FLOAT);
        actions.Add("kill", TimelineConst.DEFAULT_EMPTY_FLOAT);
        actions.Add("death", TimelineConst.DEFAULT_EMPTY_FLOAT);
        actions.Add("energySpending", TimelineConst.DEFAULT_EMPTY_FLOAT);
        actions.Add("energyCollecting", TimelineConst.DEFAULT_EMPTY_FLOAT);
        actions.Add("xitonSpending", TimelineConst.DEFAULT_EMPTY_FLOAT);
        actions.Add("xitonCharging", TimelineConst.DEFAULT_EMPTY_FLOAT);
        actions.Add("pickup", TimelineConst.DEFAULT_EMPTY_FLOAT);
        actions.Add("checkpoint", TimelineConst.DEFAULT_EMPTY_FLOAT);
        actions.Add("shifted", TimelineConst.DEFAULT_EMPTY_FLOAT);
        actions.Add("rewinded", TimelineConst.DEFAULT_EMPTY_FLOAT);
        actions.Add("depricatingWeapon", TimelineConst.DEFAULT_EMPTY_FLOAT);
    }

    public TimelineActionData Copy()
    {
        return new TimelineActionData(actions);
    }

    public void Set(string valueName, float value)
    {
        if (actions.ContainsKey(valueName))
        {
            actions[valueName] = value;
        }
        else
            Debug.Log("TimelineActionData: cannot find action - " + valueName);
    }

    public void Clear()
    {
        foreach (string variableName in new List<string>(actions.Keys))
        {
            actions[variableName] = TimelineConst.DEFAULT_EMPTY_FLOAT;
        }
    }

    public TimelineActionData SubAllVariables(float value)
    {
        TimelineActionData result = new TimelineActionData();
        foreach (string variableName in new List<string>(actions.Keys))
        {
            float resultValue = -1f;
            if (actions[variableName] > 0)
            {
                resultValue = value - actions[variableName];
                if (resultValue < 0)
                {
                    resultValue = TimelineConst.DEFAULT_EMPTY_FLOAT;
                }
            }

            result.Set(variableName, resultValue);
        }

        return result;
    }

    public Dictionary<string, float> ParseWithPRefix(string prefix)
    {
        Dictionary<string, float> parsed = new Dictionary<string, float>() { };
        foreach (KeyValuePair<string, float> actionPair in actions)
        {
            parsed.Add(prefix + actionPair.Key, actionPair.Value);
        }
        return parsed;
    }
}
