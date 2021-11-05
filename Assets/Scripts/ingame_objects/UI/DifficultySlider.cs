using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class DifficultySlider : MonoBehaviour
{
    Slider slider;
    GameManager gameManager;
    int maxDifficult = 4;
    int mixDifficult = 1;

    private void Start()
    {
        slider = transform.GetComponent<Slider>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        slider.value = ((float)GlobalVariables.game_difficult - 1f) / 3f;
    }
    public void CheckChangeValue()
    {
        gameManager.ChangeDifficultyOn(Mathf.RoundToInt(slider.value * 3f + 1));
    }
}
