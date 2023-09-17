using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class DataRecorder : MonoBehaviour
{
    [SerializeField]
    bool isLogging = false;

    GameManager gameManager;

    [SerializeField] string dataSavingName = "player_recorded_data";

    public GameObject dataRepMenu;
    Text dataRepText;

    bool isPlayerResultsWriting = GlobalVariables.is_writing;

    // first - falling down death, second - from enemies' damage
    List<int> deathCounter = new List<int>() { 0, 0 };

    // 0 - push, 1 - samu, 2 - sword
    List<int> totalKilledEnemyCounter = new List<int>() { 0, 0, 0 };

    int totalRestoredHp = 0;
    int totalTimesShifted = 0;
    int totalSpentOnRewind = 0;
    int totalEnergySpent = 0;
    int totalEnergyCollected = 0;

    int totalXitonCharged = 0;
    int totalXitonSpent = 0;
    int totalDamageBlocked = 0;
    int totalParryTimes = 0;
    int totalDamageByParry = 0;
    List<int> totalDeprivatedWeapon = new List<int>() { 0, 0 }; // 0-flamethrower, 1-gravitybomb


    float oneLifePassingTime = 0.0f;
    float totalPassingTime = 0.0f;

    int iter = 20;
    List<Vector3> movementTrace = new List<Vector3>() { };
    List<Vector3> curLifeTrace = new List<Vector3>() { };

    // Timeline list
    [SerializeField] int timelineRecordingRate = 30;

    bool isTimelineRecording = true;

    Player player;

    TimelineRecorder timelineRecorder = new TimelineRecorder();
    [SerializeField] GameObject timelineRecordingScreen;

    [SerializeField] 
    PythonConnector pythonConnector;

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        player = gameManager.player;

        dataRepText = dataRepMenu.transform.Find("Text").GetComponent<Text>();
        dataRepMenu.SetActive(false);

        timelineRecorder.SetPythonConnector(pythonConnector);
    }

    void FixedUpdate()
    {
        if (isPlayerResultsWriting)
        {
            ProcessPlayerResultsWriting();
        }

        if (isTimelineRecording)
        {
            timelineRecorder.Process();
        }
    }

    void ProcessPlayerResultsWriting()
    {
        if (iter == 0)
        {
            float delta_ = Time.deltaTime;
            totalPassingTime += delta_ * 20;
            oneLifePassingTime += delta_ * 20;

            if (GameObject.FindGameObjectWithTag("Player") != null)
            {
                curLifeTrace.Add(GameObject.FindGameObjectWithTag("Player").transform.position);
            }
            iter = 20;
        }
        iter--;
    }

    public void PlayerKilled(bool isByFalling)
    {
        if (isByFalling)
        {
            deathCounter[0]++;
        }
        else
        {
            deathCounter[1]++;
        }
        LevelLoaded();
    }

    public void LevelLoaded()
    {
        oneLifePassingTime = 0.0f;
        movementTrace.AddRange(curLifeTrace);
        curLifeTrace.Clear();
    }

    /// calling from last_level loading
    public void StoreData()
    {
        isPlayerResultsWriting = false;
        movementTrace.AddRange(curLifeTrace);
        dataRepMenu.SetActive(true);
        dataRepText.text = totalPassingTime + "\n" +
           "\n" +
            deathCounter[1] + "\n" +
            deathCounter[0] + "\n" +
            "\n" +
            totalKilledEnemyCounter[0] + "\n" +
            totalKilledEnemyCounter[1] + "\n" +
            totalKilledEnemyCounter[2] + "\n" +
            "\n" +
            totalRestoredHp + "\n" +
            totalTimesShifted + "\n" +
            totalParryTimes + "\n" +
            totalDamageByParry + "\n" +
            totalDamageBlocked + "\n" +
            "\n" +
            totalDeprivatedWeapon[0] + "\n" +
            totalDeprivatedWeapon[1] + "\n" +
            "\n" +
            totalEnergySpent + "\n" +
            totalSpentOnRewind + "\n" +
            totalEnergyCollected + "\n" +
            "\n" +
            totalXitonCharged + "\n" +
            totalXitonSpent
            ;
    }

    public void AddTo(string variable, int value)
    {
        if (variable == "restored_hp")
            totalRestoredHp += value;
        else if (variable == "shifted")
        {
            totalTimesShifted += value;
            timelineRecorder.UpdateActionTime("shifted");
        }
        else if (variable == "spent_on_rewind")
        {
            totalSpentOnRewind += value;
            timelineRecorder.UpdateActionTime("rewinded");
        }
        else if (variable == "energy_spent")
        {
            timelineRecorder.UpdateActionTime("energySpending");
            totalEnergySpent += value;
        }
        else if (variable == "energy_collected")
        {
            timelineRecorder.UpdateActionTime("energyCollecting");
            totalEnergyCollected += value;
        }
        else if (variable == "push_killed")
        {
            timelineRecorder.UpdateActionTime("kill");
            totalKilledEnemyCounter[0] += value;
        }
        else if (variable == "samu_killed")
        {
            timelineRecorder.UpdateActionTime("kill");
            totalKilledEnemyCounter[1] += value;
        }
        else if (variable == "sword_killed")
        {
            timelineRecorder.UpdateActionTime("kill");
            totalKilledEnemyCounter[2] += value;
        }
        else if (variable == "parry_times")
            totalParryTimes += value;
        else if (variable == "xiton_charged")
        {
            timelineRecorder.UpdateActionTime("xitonCharging");
            totalXitonCharged += value;
        }
        else if (variable == "xiton_spent")
        {
            timelineRecorder.UpdateActionTime("xitonSpending");
            totalXitonSpent += value;
        }
        else if (variable == "depr_flamethrower")
        {
            timelineRecorder.UpdateActionTime("depricatingWeapon");
            totalDeprivatedWeapon[0] += value;
        }
        else if (variable == "depr_gravitybomb")
        {
            timelineRecorder.UpdateActionTime("depricatingWeapon");
            totalDeprivatedWeapon[1] += value;
        }
        else if (variable == "damage_blocked")
            totalDamageBlocked += value;
        else if (variable == "parry_damage")
            totalDamageByParry += value;
        else
            Debug.Log("ERROR: data writing exeption - " + variable);
    }

    public void StartTimelineRecording()
    {
        timelineRecorder.StartRecordingWithParameters(1f / (float)timelineRecordingRate, Time.time, dataSavingName, gameManager);
        isTimelineRecording = true;

        StopCoroutine(ShowTimelineNotification(1f));
        StartCoroutine(ShowTimelineNotification(1f));

        pythonConnector.StartInternal();
    }

    IEnumerator ShowTimelineNotification(float duration)
    {
        timelineRecordingScreen.SetActive(true);
        yield return new WaitForSeconds(duration);
        timelineRecordingScreen.SetActive(false);
    }

    public void StopTimelineRecording()
    {
        pythonConnector.StopInternal();

        StopCoroutine(ShowTimelineNotification(1f));
        StartCoroutine(ShowTimelineNotification(1f));

        isTimelineRecording = false;
        timelineRecorder.StopRecordingAndSave(gameManager);
    }

}