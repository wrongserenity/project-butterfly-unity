using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestTrigger : Trigger
{
    GameManager gameManager;

    Collider area;

    List<string> randomObjects = new List<string>() { "5", "flamethrower", "gravitybomb" };


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
        if (tag == "5")
        {
            for (int i = 0; i < 5; i++)
            {
                GameObject goES = Resources.Load("Prefabs/Staff/EnergySphere") as GameObject;
                GameObject energySphere = Instantiate(goES, new Vector3(0f, 0f, 0f), new Quaternion(0f, 0f, 0f, 1.0f));
                energySphere.transform.position = transform.position + new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)).normalized;
            }
        }
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
