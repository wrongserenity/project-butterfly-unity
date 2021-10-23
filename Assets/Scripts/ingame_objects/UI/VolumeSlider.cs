using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    Slider slider;
    GameManager gameManager;

    private void Start()
    {
        slider = transform.GetComponent<Slider>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        slider.value = AudioListener.volume;
    }
    public void CheckChangeValue()
    {
        AudioListener.volume = slider.value;
    }
}
