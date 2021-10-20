using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialTrigger : Trigger
{
    GameManager gameManager;
    Collider area;
    Image image;
    Color startColor;

    public bool isTriggered = false;

    // Start is called before the first frame update
    void Start()
    {
        isIterative = false;

        area = GetComponent<Collider>();
        image = transform.Find("Canvas").Find("Image").GetComponent<Image>();
        image.enabled = false;
        startColor = image.color;


        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        gameManager.triggerSystem.NewTriggerToLevel(this);
    }

    public override bool CheckCondition()
    {
        Collider[] cols = Physics.OverlapBox(area.bounds.center, area.bounds.extents, area.transform.rotation);
        foreach (Collider col in cols)
        {
            if (col.tag == "Player")
            {
                Player player = col.GetComponent<Player>();
                if (player.actionObj != this)
                {
                    if (!isTriggered)
                    {
                        Appear();
                        isTriggered = true;
                    }
                    return true;
                }
                
            }
        }
        if (isTriggered)
        {
            Disappear();
            isTriggered = false;
        }
        return base.CheckCondition();
    }

    void Appear() { StartCoroutine(FadeIn(0.4f)); }

    void Disappear() { StartCoroutine(FadeOut(0.4f)); }

    IEnumerator FadeIn(float duration)
    {
        float curAlpha = 0f;
        float step = 0.05f;
        float stepTime = duration / (1f / step);
        image.enabled = true;

        while (curAlpha < 1.0f)
        {
            curAlpha += step;
            image.color = new Color(startColor.r, startColor.g, startColor.b, curAlpha);
            yield return new WaitForSeconds(stepTime);
        }
    }

    IEnumerator FadeOut(float duration)
    {
        float curAlpha = 1f;
        float step = 0.05f;
        float stepTime = duration * step;

        while (curAlpha > 0.0f)
        {
            curAlpha -= step;
            image.color = new Color(startColor.r, startColor.g, startColor.b, curAlpha);
            yield return new WaitForSeconds(stepTime);
        }
        image.enabled = false;
    }
}
