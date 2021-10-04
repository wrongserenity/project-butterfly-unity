using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoboSamuraiLogic : Enemy
{
    int tickCount = 0;
    Vector3 initialPosition = new Vector3(0f, 0f, 0f);
    Vector3 playerPos = new Vector3(0f, 0f, 0f);
    Vector3 ownPos = new Vector3(0f, 0f, 0f);

    List<bool> directrionAvailable = new List<bool>() { false, false, false, false };

    void Start()
    {
        base.Start();
        max_hp = 100;
        cur_hp = 100;
        type = 'm';
        power = 5;
        title = "robosamurai";
        speed_vel = GlobalVariables.melee_max_speed;

        GameObject go = Resources.Load("Prefabs/Weapons/SamuraiWeapon") as GameObject;
        weapon = Instantiate(go, new Vector3(0f, 0f, 0f), transform.rotation).GetComponent<Weapon>();
        weapon.transform.SetParent(transform.Find("Model").transform.Find("weapon").transform, false);
        weapon.GiveWeaponTo(this);
    }

    // Update is called once per frame
    public override void EnemyLogicProcess()
    {
        if (initialPosition.magnitude == 0)
            initialPosition = transform.position;

        playerPos = gameManager.player.transform.position;
        ownPos = transform.position;
        float distance = (playerPos - ownPos).magnitude;
        if (distance <= notice_range && !is_player_noticed)
        {
            gameManager.battleSystem.AddToBattle(this);
            is_player_noticed = true;
        }

        directrionAvailable = GetAvailableDirections();

        if (is_player_noticed)
        {
            BattleMove(playerPos, ownPos, directrionAvailable);
        }
        else
        {
            tickCount++;
            if (tickCount == 10)
            {
                MoveAround(directrionAvailable);
                tickCount = 0;
            }
        }
        FallingOutCheck(ownPos);
        Attack();
    }
}
