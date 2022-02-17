using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        gameManager.player.gameObject.SetActive(true);
        StartCoroutine(HideIn(2f));
    }

    public void QuitGame()
    {
        Debug.Log("QuitGame");
        Application.Quit();
    }

    IEnumerator HideIn(float sec)
    {
        int iterations = 25;
        float delta = sec / (float)iterations;

        // menu UI fade out
        List<Image> img = new List<Image>() { };
        for (int i = 0; i < transform.childCount; i++)
        {
            Image tempImg = transform.GetChild(i).GetComponent<Image>();
            if (tempImg != null)
                img.Add(tempImg);
        }

        for (int i = iterations - 1; i >= 0; i--)
        {
            SetColorForEachImage(img, new Color(1, 1, 1, (float)i / (float)iterations));
            yield return new WaitForSeconds(delta);
        }

        gameObject.SetActive(false);
        SetColorForEachImage(img, Color.white);
    }

    void SetColorForEachImage(List<Image> img, Color clr)
    {
       foreach(Image img_ in img)
            img_.color = clr;
    }

    
}
