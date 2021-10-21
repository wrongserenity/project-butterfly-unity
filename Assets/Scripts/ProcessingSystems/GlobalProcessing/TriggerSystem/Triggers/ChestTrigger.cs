using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestTrigger : Trigger
{
    GameManager gameManager;

    Collider area;

    List<string> randomObjects = new List<string>() { "50", "flamethrower", "gravitybomb" };


    void Start()
    {
        isIterative = false;

        area = transform.GetComponent<Collider>();

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        gameManager.triggerSystem.NewTriggerToLevel(this);
    }

    public override bool CheckCondition()
    {
        for (int i = 0; i < 2; i++)
        {
            Collider[] cols = Physics.OverlapBox(area.bounds.center, area.bounds.extents, area.transform.rotation);
            foreach (Collider col in cols)
            {
                if (col.tag == "Player")
                {
                    Player player = col.GetComponent<Player>();
                    if (player.actionObj != this)
                        player.ChangeUseMode(this);
                    return true;
                }
            }
        }
        return base.CheckCondition();
    }

    public override void Activate()
    {
        ProcessByTag(randomObjects[Random.Range(0, randomObjects.Count - 1)]);
        gameObject.SetActive(false);
    }

    void ProcessByTag(string tag)
    {
        if (tag == "50")
            gameManager.player.EnergyTransfer(50);
        else if (tag == "flamethrower")
            Weapon.LoadWeaponFrom("Prefabs/Weapons/Flamethrower/Flamethrower", gameManager.player, true);
        else if (tag == "gravitybomb")
            Weapon.LoadWeaponFrom("Prefabs/Weapons/GravityBomb/GravityBomb", gameManager.player, true);


    }

    public override void ReloadTrigged()
    {
        gameObject.SetActive(true);
    }
}
