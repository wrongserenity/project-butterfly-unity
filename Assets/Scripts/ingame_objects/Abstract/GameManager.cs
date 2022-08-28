using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    public LineRenderer rewindLineRenderer;
    public PauseMenu pauseMenu;

    public string levelContainerTag = "LevelContainer";

    public GameObject[] doNotDestroyOnLoad = new GameObject[] { };

    AsyncOperation asyncOperation;

    List <Enemy> enemyAfterCheckPoint = new List<Enemy>() { };
    public Vector3 currentCheckpoint;
    // hp, energy, xiton
    List<int> currentCheckPointData = new List<int>() { GlobalVariables.player_max_hp, 0, 0 };

    public Image deathImage;

    bool isFading = false;

    float startFixedDeltaTime;
    public bool isTimeScaled = false;
    bool isTimeScalingRN = false;

    List<string> levelsPathList = new List<string>() {"Prefabs/Levels/EmptyLevel",  "Prefabs/Levels/Discoverer" };
    int curLevelIndex = 0;
    Level curLevel;

    private void Start()
    {
        deathImage = GameObject.Find("death").GetComponent<Image>();
        startFixedDeltaTime = Time.fixedDeltaTime;
        deathImage.gameObject.SetActive(false);

        foreach(GameObject go in doNotDestroyOnLoad)
            DontDestroyOnLoad(go);

        ReplaceCurrentLevel();
    }

    // creates async operation and scene switching waits calling NextLevel()
    // should be linked to trigger 5-10 seconds before current level's end
    public void NextLevelPreload()
    {
        asyncOperation = SceneManager.LoadSceneAsync(curLevelIndex + 1);
        asyncOperation.allowSceneActivation = false;
    }

    // linking to level's end
    public bool NextLevel()
    {
        if (asyncOperation != null)
        {
            curLevelIndex++;
            asyncOperation.allowSceneActivation = true;
            asyncOperation.completed += HandeAsyncLoadCompleted;

            if (curLevelIndex == (levelsPathList.Count))
                dataRecorder.StoreData();
            return true;
        }
        else
        {
            return false;
        }
        
    }

    public void HandeAsyncLoadCompleted(AsyncOperation operation)
    {
        operation.completed -= HandeAsyncLoadCompleted;
        ReplaceCurrentLevel();
    }

    public void ReplaceCurrentLevel()
    {
        int childCount = levelContainer.transform.childCount;
        if (childCount > 0)
            for (int i = childCount - 1; i >= 0; i--)
            {
                GameObject destroyableLevelObj = levelContainer.transform.GetChild(i).gameObject;
                destroyableLevelObj.tag = "Untagged";
                Destroy(destroyableLevelObj);

            }

        GameObject levelObj = GameObject.FindGameObjectWithTag(levelContainerTag);
        levelObj.transform.SetParent(levelContainer.transform);
    }

    public void ReloadCurrentLevel()
    {
        if (levelContainer.transform.childCount != 1)
            Debug.Log("ERROR: There should be only one level");
        else
        {
            currentCheckpoint = levelContainer.transform.GetChild(0).Find("SpawnPosition").position;
            currentCheckPointData = new List<int>() { 100, 0, 0 };
            enemyAfterCheckPoint.Clear();
            if (!isFading)
                StartCoroutine(LevelReloadFadeInOut());
        }
    }

    public void AddEnemyToReload(Enemy enemy)
    {
        if (!enemyAfterCheckPoint.Contains(enemy))
            enemyAfterCheckPoint.Add(enemy);
    }

    public void UpdateCheckPoint(Vector3 point)
    {
        currentCheckpoint = point;
        enemyAfterCheckPoint.Clear();
        currentCheckPointData[0] = player.cur_hp;
        currentCheckPointData[1] = player.cur_energy;
        currentCheckPointData[2] = player.curXitonCharge;
    }

    public void ReloadToCheckPoint()
    {
        if (levelContainer.transform.childCount != 1)
            Debug.Log("ERROR: There should be only one level");
        else
        {
            if (!isFading)
                StartCoroutine(LevelReloadToCheckPoint());
        }
    }

    public IEnumerator LevelReloadToCheckPoint()
    {
        SetTimeScale(0f, false);
        deathImage.gameObject.SetActive(true);
        isFading = true;
        player.movement_lock = true;
        player.rotation_lock = true;
        foreach (AudioSource sound in transform.Find("DeathSound").GetComponentsInChildren<AudioSource>())
        {
            if (sound.name == "DeathSound")
            {
                sound.Play();
            }
        }
        float curAlpha = 0.0f;
        deathImage.color = new Color(0f, 0f, 0f, 0.0f);
        while (curAlpha < 1.0f)
        {
            curAlpha += 0.05f;
            deathImage.color = new Color(0f, 0f, 0f, curAlpha);
            yield return new WaitForSecondsRealtime(0.05f);
        }
        bool isReturned = false;
        while (!isReturned)
            isReturned = ReturnTimeScale();

        yield return new WaitForSeconds(0.5f);
        levelContainer.transform.GetChild(0).GetComponent<Level>().LoadCheckPoint(enemyAfterCheckPoint);
        enemyAfterCheckPoint.Clear();
        battleSystem.Reload();
        player.cur_hp = currentCheckPointData[0];
        player.cur_energy = currentCheckPointData[1];
        player.curXitonCharge = currentCheckPointData[2];
        triggerSystem.Reload();
        yield return new WaitForSeconds(0.5f);

        while (curAlpha > 0.0f)
        {
            curAlpha -= 0.05f;
            deathImage.color = new Color(0f, 0f, 0f, curAlpha);
            yield return new WaitForSeconds(0.05f);
        }

        player.movement_lock = false;
        player.rotation_lock = false;
        isFading = false;
        deathImage.gameObject.SetActive(false);
    }


    public IEnumerator LevelReloadFadeInOut()
    {
        deathImage.gameObject.SetActive(true);
        isFading = true;
        player.movement_lock = true;
        player.rotation_lock = true;
        foreach (AudioSource sound in transform.Find("DeathSound").GetComponentsInChildren<AudioSource>())
        {
            if (sound.name == "DeathSound")
            {
                sound.Play();
            }
        }
        float curAlpha = 0.0f;
        deathImage.color = new Color(0f, 0f, 0f, 0.0f);
        while (curAlpha < 1.0f)
        {
            curAlpha += 0.05f;
            deathImage.color = new Color(0f, 0f, 0f, curAlpha);
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(0.5f);
        levelContainer.transform.GetChild(0).GetComponent<Level>().FastReload();
        battleSystem.Reload();
        dataRecorder.LevelLoaded();
        triggerSystem.Reload();
        yield return new WaitForSeconds(0.5f);

        while (curAlpha > 0.0f)
        {
            curAlpha -= 0.05f;
            deathImage.color = new Color(0f, 0f, 0f, curAlpha);
            yield return new WaitForSeconds(0.05f);
        }

        player.movement_lock = false;
        player.rotation_lock = false;
        isFading = false;
        deathImage.gameObject.SetActive(false);
    }

    public void SetTimeScale(float value, bool isFromPlayer)
    {
        if (value < 1f)
        {
            mainCamera.ChangeCrhomaticAberrationIntencity(1f);
        }
        Time.timeScale = value;
        Time.fixedDeltaTime = Time.timeScale * .01f;
        if (isFromPlayer)
            isTimeScaled = true;
    }
    public bool ReturnTimeScale()
    {
        if (!isTimeScalingRN)
        {
            mainCamera.ChangeCrhomaticAberrationIntencity(0f);
            Time.timeScale = 1f;
            Time.fixedDeltaTime = startFixedDeltaTime;
            isTimeScaled = false;
            return true;
        }
        return false;
        
    }
    public void SetTimeScaleFor(float value, float duration)
    {
        StartCoroutine(TimeScaleFor(value, duration));
    }

    IEnumerator TimeScaleFor(float value, float duration)
    {
        isTimeScalingRN = true;
        Time.timeScale = value;
        Time.fixedDeltaTime = Time.timeScale * .01f;
        yield return new WaitForSeconds(duration);
        isTimeScalingRN = false;
        ReturnTimeScale();
        
    }

    public void CancelTimeScaleFor()
    {
        StopCoroutine(TimeScaleFor(0f, 0f));
        isTimeScalingRN = false;
        ReturnTimeScale();
        
    }

    public void ChangeDifficultyOn(int difficulty)
    {
        if (battleSystem.game_difficulty != difficulty)
        {
            battleSystem.game_difficulty = difficulty;
            battleSystem.Reload();
        }
    }


    public string Ping()
    {
        return "GameManager is found";
    }
}
