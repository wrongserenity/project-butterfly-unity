using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DoorTrigger : Trigger
{
    GameManager gameManager;
    Collider area;
    GameObject door;
    public float openDelay = 0f;

    Image activatedButton;
    Image notActivatedButton;

    public bool isOpen = false;

    void Start()
    {
        isIterative = false;

        area = GetComponent<Collider>();
        door = transform.Find("Door").gameObject;
        activatedButton = transform.Find("Canvas").Find("Activated").GetComponent<Image>();
        notActivatedButton = transform.Find("Canvas").Find("NotActivated").GetComponent<Image>();

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        gameManager.triggerSystem.NewTriggerToLevel(this);
        transform.Find("Canvas").GetComponent<Canvas>().worldCamera = gameManager.mainCamera.gameObject.GetComponent<Camera>();
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
                    if (!isOpen)
                    {
                        StartCoroutine(Open());
                        isOpen = true;
                    }
                    return true;
                }

            }
        }
        return base.CheckCondition();
    }

    IEnumerator Open()
    {
        activatedButton.enabled = true;
        activatedButton.fillAmount = 0f;
        float step = 0.01f;
        float curFill = 0f;
        float timeStep = openDelay / (1f / step);

        Color sCol = door.GetComponent<MeshRenderer>().material.color;

        while (curFill < 1f)
        {
            curFill += step;
            activatedButton.fillAmount = curFill;
            door.GetComponent<MeshRenderer>().material.color =  new Color(sCol.r, sCol.g, sCol.b, (1 - curFill * 0.5f));
            yield return new WaitForSeconds(timeStep);
        }
        door.SetActive(false);
        door.GetComponent<MeshRenderer>().material.color = sCol;
    }


    public override void ReloadTrigged()
    {
        door.SetActive(true);
        activatedButton.enabled = false;
        isOpen = false;
    }
}
