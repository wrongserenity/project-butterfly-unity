using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicSystem : MonoBehaviour
{
    GameManager gameManager;
    BattleSystem battleSystem;

    int playing = 0;
    int fadeCalls = 0;
    public List<AudioSource> music;

    float fadeStepSec = 0.05f;
    float fadeDelaySec = 0f;
    float fadeDurationSec = 0.2f;

    float beatTime = 1.333f;

    List<int> enoughtEnemyAmount = new List<int>() { };
    int mostEpicEnemyAmount = GlobalVariables.music_max_enemy_amount;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        battleSystem = gameManager.battleSystem;

        CalculateEnoughtEnemyAmount();
        music[playing].Play();
    }

    int EstimateBattleState()
    {
        int curAmount = battleSystem.CalculateEnemiesCount();
        int requiredStage = 0;
        for (int i = 0; i < (enoughtEnemyAmount.Count); i++)
        {
            if (curAmount >= enoughtEnemyAmount[i])
                requiredStage = i;
        }
        return requiredStage;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        int temp_playing = EstimateBattleState();
        if (temp_playing != playing && fadeCalls == 0)
        {
            ChangeMusic(temp_playing);
        }
    }

    void ChangeMusic(int what_to_play)
    {
        if (playing != what_to_play)
        {
            music[what_to_play].time = music[playing].time;
            StartCoroutine(FadeOut(playing, fadeDurationSec));
            StartCoroutine(FadeIn(what_to_play, fadeDurationSec));
            playing = what_to_play;
        }
    }

    IEnumerator FadeOut(int track_number, float duration)
    {
        fadeCalls++;
        float tempStartVolume = music[track_number].volume;
        float step = tempStartVolume / (duration / fadeStepSec);
        yield return new WaitForSeconds(fadeDelaySec);

        // for rythmic
        yield return new WaitForSeconds(DistanceToNextBar(music[track_number].time));

        while (music[track_number].volume > step)
        {
            //Debug.Log("Out " + track_number + ": " + music[track_number].volume);
            music[track_number].volume -= step;
            yield return new WaitForSeconds(fadeStepSec);
        }
        music[track_number].Stop();
        music[track_number].volume = tempStartVolume;
        fadeCalls--;
    }

    IEnumerator FadeIn(int track_number, float duration)
    {
        fadeCalls++;
        float tempEndVolume = music[track_number].volume;
        music[track_number].volume = 0f;
        music[track_number].Play();
        float step = tempEndVolume / (duration / fadeStepSec);
        yield return new WaitForSeconds(fadeDelaySec);

        // for rythmic
        yield return new WaitForSeconds(DistanceToNextBar(music[track_number].time));

        while (music[track_number].volume < tempEndVolume - step)
        {
            //Debug.Log("Out " + track_number + ": " + music[track_number].volume);
            music[track_number].volume += step;
            yield return new WaitForSeconds(fadeStepSec);
        }
        fadeCalls--;
    }

    float DistanceToNextBar(float cur_time)
    {
        while(cur_time > beatTime)
        {
            if (cur_time < beatTime * 1.5f)
            {
                return cur_time;
            }
            cur_time -= beatTime;
        }
        return cur_time - fadeDurationSec;
    }

    void CalculateEnoughtEnemyAmount()
    {
        enoughtEnemyAmount.Clear();
        enoughtEnemyAmount.Add(0);

        int lenList = music.Count;
        if (lenList < 2)
            Debug.Log("ERROR: not enought music samples");
        else if (lenList == 2)
            enoughtEnemyAmount.Add(1);
        else if (lenList == 3)
        {
            enoughtEnemyAmount.Add(1);
            enoughtEnemyAmount.Add(mostEpicEnemyAmount);
        }
        else
        {
            int step = (int)Mathf.Floor(mostEpicEnemyAmount / (lenList - 1));
            for (int i = 0; i < lenList - 1; i++)
            {
                int tempNumber = step * (i+1);
                if (tempNumber != 0)
                    enoughtEnemyAmount.Add(tempNumber);
                else
                    enoughtEnemyAmount.Add(1);
                
            }
            enoughtEnemyAmount[1] = 1;
            enoughtEnemyAmount[lenList - 1] = mostEpicEnemyAmount;

        }

    }
}
