using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InterfaceSystem : MonoBehaviour
{
    Image healthBar;
    Image energyBar;
    Image xitonBar;
    Image additionalBar;

    bool isAdditionalActivated = false;
    Player player;

    public void LoadInterface()
    {
        healthBar = transform.Find("HealthGray").Find("Health").GetComponent<Image>();
        energyBar = transform.Find("EnergyGray").Find("Energy").GetComponent<Image>();
        xitonBar = transform.Find("XitonGray").Find("Xiton").GetComponent<Image>();
        additionalBar = transform.Find("AdditionalGray").Find("Additional").GetComponent<Image>();
        HideAdditional();
        player = GetComponentInParent<Player>();
    }

    public void BarAnimation(string type, string toDo, float duration)
    {
        RefreshBasicData();
        if (type == "health")
        {
            if (toDo == "changed")
            {
                StartCoroutine(BarPulse(healthBar));
            }
        }
        else if (type == "energy")
        {
            if (toDo == "changed")
            {
                StartCoroutine(BarPulse(energyBar));
            }
        }
        else if (type == "xiton")
        {
            if (toDo == "changed")
            {
                StartCoroutine(BarPulse(xitonBar));
            }
        }else if (type == "additional")
        {
            if (toDo == "changed")
            {
                StartCoroutine(BarPulse(additionalBar));
            }
        }
    }

    public IEnumerator BarPulse(Image barObject)
    {
        float scaleX = 1f;
        float scaleXTarget = scaleX * 1.2f;
        while (scaleX < scaleXTarget)
        {
            scaleX += 0.02f;
            barObject.gameObject.transform.localScale = new Vector3(scaleX, scaleX, scaleX);
            yield return new WaitForSeconds(0.01f);
        }
        barObject.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
    }

    public void ShowAdditional()
    {
        additionalBar.enabled = true;
        transform.Find("AdditionalGray").GetComponent<Image>().enabled = true;
        isAdditionalActivated = true;
    }

    public void HideAdditional()
    {
        additionalBar.enabled = false;
        transform.Find("AdditionalGray").GetComponent<Image>().enabled = false;
        isAdditionalActivated = false;
    }

    void RefreshBasicData()
    {
        healthBar.fillAmount = ((float)player.cur_hp / (float)player.max_hp) / 3f; ;
        energyBar.fillAmount = ((float)player.cur_energy / (float)player.max_energy) / 3f;
        xitonBar.fillAmount = ((float)player.curXitonCharge / (float)player.maxXitonCharge) / 3f;
    }

    public void RefreshAdditionalData(float additional)
    {
        if (isAdditionalActivated)
            additionalBar.fillAmount = additional;
    }
}
