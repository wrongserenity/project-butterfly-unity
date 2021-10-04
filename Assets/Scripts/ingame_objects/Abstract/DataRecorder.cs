using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataRecorder : MonoBehaviour
{
    GameManager gameManager;

    bool isWriting = GlobalVariables.is_writing;

    // first - falling down death, second - from enemies' damage
    List<int> deathCounter = new List<int>() { 0, 0 };

    // 0 - push, 1 - samu, 2 - sword
    List<int> totalKilledEnemyCounter = new List<int>() { 0, 0, 0 };

    int totalRestoredHp = 0;
    int totalTimesShifted = 0;
    int totalSpentOnRewind = 0;
    int totalEnergySpent = 0;
    int totalEnergyCollected = 0;

    float oneLifePassingTime = 0.0f;
    float totalPassingTime = 0.0f;

    List<Vector3> movementTrace = new List<Vector3>() { };
    List<Vector3> curLifeTrace = new List<Vector3>() { };

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    void FixedUpdate()
    {
        if (isWriting)
        {
            float delta_ = Time.deltaTime;
            totalPassingTime += delta_;
            oneLifePassingTime += delta_;

            if (GameObject.FindGameObjectWithTag("Player") != null)
            {
                curLifeTrace.Add(GameObject.FindGameObjectWithTag("Player").transform.position);
            }
        }
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

    void LevelLoaded()
    {
        oneLifePassingTime = 0.0f;
        movementTrace.AddRange(curLifeTrace);
        curLifeTrace.Clear();
    }

    /// calling from last_level loading
    void StoreData()
    {
        isWriting = false;
        movementTrace.AddRange(curLifeTrace);
    }

    public void AddTo(string variable, int value)
    {
        if (variable == "restored_hp")
            totalRestoredHp += value;
        else if (variable == "shifted")
            totalTimesShifted += value;
        else if (variable == "spent_on_rewind")
            totalSpentOnRewind += value;
        else if (variable == "energy_spent")
            totalEnergySpent += value;
        else if (variable == "energy_collected")
            totalEnergyCollected += value;
        else if (variable == "push_killed")
            totalKilledEnemyCounter[0] += value;
        else if (variable == "samu_killed")
            totalKilledEnemyCounter[1] += value;
        else if (variable == "sword_killed")
            totalKilledEnemyCounter[2] += value;
        else
            Debug.Log("ERROR: data writing exeption");

    }
}
