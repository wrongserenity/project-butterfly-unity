using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    GameManager gameManager;

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        gameManager.player.gameObject.SetActive(false);
    }

    public void PlayGame()
    {
        gameManager.NextLevel();
        StartCoroutine(HideIn(2f));
    }

    public void QuitGame()
    {
        Debug.Log("QuitGame");
        Application.Quit();
    }

    IEnumerator HideIn(float sec)
    {
        yield return new WaitForSeconds(sec);
        gameObject.SetActive(false);
    }
}
