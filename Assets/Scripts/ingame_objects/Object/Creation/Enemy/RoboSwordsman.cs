using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoboSwordsman : Enemy
{
    int tickCount = 0;
    Vector3 initialPosition = new Vector3(0f, 0f, 0f);

    List<bool> directrionAvailable = new List<bool>() { false, false, false, false };

    void Start()
    {
        base.Start();
        max_hp = 100;
        cur_hp = 100;
        type = 'm';
        power = 5;
        title = "roboswordsman";
        speed_vel = GlobalVariables.melee_max_speed;


        Weapon.LoadWeaponFrom("Prefabs/Weapons/SwordsmanWeapon", this, false);
    }

    // Update is called once per frame
    public override void EnemyLogicProcess()
    {
        if (initialPosition.magnitude == 0)
            initialPosition = transform.position;

        CheckPlayerNoticing();

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
