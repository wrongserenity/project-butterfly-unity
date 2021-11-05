using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerSystem : MonoBehaviour
{
    List<Trigger> triggersList = new List<Trigger>() { };
    bool triggered = false;
    public GameManager gameManager;

    public void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public void Delete(Trigger trigger)
    {
        triggersList.Remove(trigger);
    }

    public void Clean()
    {
        triggersList.Clear();
        triggered = false;
    }

    public void Reload()
    {
        triggered = false;
        foreach(Trigger trigger in triggersList)
                trigger.ReloadTrigged();
    }

    public void NewTriggerToLevel(Trigger trigger)
    {
        triggersList.Add(trigger);
    }

    public void Check()
    {
        List<Trigger> needToDelete = new List<Trigger>() { };
        triggered = false;
        foreach(Trigger trigger in triggersList)
        {
            if (trigger != null)
            {
                if (trigger.CheckCondition())
                    triggered = true;
            }
            else
            {
                if (triggersList.Contains(trigger))
                    needToDelete.Add(trigger);
            }
        }

        foreach (Trigger trigger in needToDelete)
            triggersList.Remove(trigger);
        if (!triggered)
        {
            if (gameManager.player.actionObj != null)
                gameManager.player.ChangeUseMode(null);
        }
    }

    private void FixedUpdate()
    {
        Check();
    }
}
