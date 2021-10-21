using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    GameManager gameManager;
    public bool isPaused = false;

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public void Resume()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        isPaused = false;
        gameManager.ReturnTimeScale();
        gameObject.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("QuitGame");
        Application.Quit();
    }

    public void Pause()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        isPaused = true;
        gameManager.SetTimeScale(0.00f, false);
        gameObject.SetActive(true);
    }
}
