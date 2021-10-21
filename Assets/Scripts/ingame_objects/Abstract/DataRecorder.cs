using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DataRecorder : MonoBehaviour
{
    GameManager gameManager;

    public GameObject dataRepMenu;
    Text dataRepText;

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

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        dataRepText = dataRepMenu.transform.Find("Text").GetComponent<Text>();
        dataRepMenu.SetActive(false);
    }

    void FixedUpdate()
    {
        if (isWriting)
        {
            if(iter == 0)
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
        isWriting = false;
        movementTrace.AddRange(curLifeTrace);
        dataRepMenu.SetActive(true);
        dataRepText.text = totalPassingTime + "\n" +
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
        else if (variable == "parry_times")
            totalParryTimes += value;
        else if (variable == "xiton_charged")
            totalXitonCharged += value;
        else if (variable == "xiton_spent")
            totalXitonSpent += value;
        else if (variable == "depr_flamethrower")
            totalDeprivatedWeapon[0] += value;
        else if (variable == "depr_gravitybomb")
            totalDeprivatedWeapon[1] += value;
        else if (variable == "damage_blocked")
            totalDamageBlocked += value;
        else if (variable == "parry_damage")
            totalDamageByParry += value;
        else
            Debug.Log("ERROR: data writing exeption");
    }
}
