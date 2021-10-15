using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

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

    public Image deathImage;

    bool isFading = false;

    float startFixedDeltaTime;
    bool isTimeScaled = false;
    bool isTimeScalingRN = false;
    private void Start()
    {
        deathImage = GameObject.Find("death").GetComponent<Image>();
        startFixedDeltaTime = Time.fixedDeltaTime;
    }


    public void ReloadCurrentLevel()
    {
        if (levelContainer.transform.childCount != 1)
            Debug.Log("There should be only one level");
        else
        {
            if(!isFading)
                StartCoroutine(LevelFadeInOut());
        }

    }

    public IEnumerator LevelFadeInOut()
    {
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
    }

    public void SetTimeScale(float value)
    {
        if (value < 1f)
        {
            mainCamera.ChangeCrhomaticAberrationIntencity(1f);
        }
        Time.timeScale = value;
        Time.fixedDeltaTime = Time.timeScale * .01f;
    }
    public void ReturnTimeScale()
    {
        if (!isTimeScalingRN)
        {
            mainCamera.ChangeCrhomaticAberrationIntencity(0f);
            Time.timeScale = 1f;
            Time.fixedDeltaTime = startFixedDeltaTime;
        }
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


    public string Ping()
    {
        return "GameManager is found";
    }
}
