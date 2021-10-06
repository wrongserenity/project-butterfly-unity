using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Creation
{
    public bool is_player_noticed = false;
    public float notice_range = GlobalVariables.default_notice_range;

    /// 0- forward, 1 - backward, 2 - right, 3 - left
    public Collider[] movementHitbox;

    public char type;
    public int power;
    public int currentLineNum = -1;   // so -1 is for non-existent line number
    Vector3 spawn_position;
    Quaternion spawn_rotation;
    bool is_killed = false;

    bool moving_to_original_position = false;

    // Start is called before the first frame update
    public void Start()
    {
        base.Start();
        spawn_position = transform.position;
        spawn_rotation = transform.rotation;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!is_killed)
        {
            EnemyLogicProcess();
        }
    }

    public virtual void EnemyLogicProcess() {}

    public void Attack()
    {
        if (is_player_noticed)
        {
            if (weapon != null)
            {
                if (new List<char>() { 'm', 'e'}.Exists(x => x == type))
                {
                    if (currentLineNum <= 1)
                    {
                        weapon.EnemyAttack();
                    }
                }
                // there will be condition for 'r' - range, and 's' - support enemies
            }
        }
    }

    public void BattleMove(Vector3 player_pos, Vector3 own_pos, List<bool> available_directions)
    {
        // for melee enemies
        if (new List<char>() { 'm', 'e' }.Exists(x => x == type))
        {
            player_pos.y = own_pos.y;
            MoveTo(own_pos, player_pos, available_directions);
        }
    }

    // should be in abstract logic class
    Vector3 CreateRandomVector()
    {
        return Random.insideUnitCircle.normalized;
    }

    public void MoveAround(List<bool> available_directions)
    {
        Vector3 current_position = transform.position;
        if ((current_position - spawn_position).magnitude < 0.05f)
        {
            moving_to_original_position = false;
        }
        if ((current_position - spawn_position).magnitude > 0.5f || moving_to_original_position)
        {
            MoveTo(current_position, spawn_position, available_directions);
            if (!moving_to_original_position)
            {
                moving_to_original_position = true;
            }
        }
        else
        {
            Vector3 random_vector = CreateRandomVector();
            ProcessMovement(random_vector, random_vector, available_directions);
        }
    }

    public void EnemyTurnOff()
    {
        is_killed = true;
        foreach (Renderer ren in GetComponentsInChildren<Renderer>())
            ren.enabled = false;
        DisableCollision();
    }

    public void EnemyReload()
    {
        is_player_noticed = false;
        is_killed = false;
        foreach (Renderer ren in GetComponentsInChildren<Renderer>())
            ren.enabled = true;
        EnableCollision();
        transform.position = spawn_position;
        transform.rotation = spawn_rotation;
        cur_hp = max_hp;
    }

    public void DisableCollision()
    {
        foreach (Collider col in GetComponentsInChildren<Collider>())
            col.enabled = false;
    }

    public void EnableCollision()
    {
        foreach (Collider col in GetComponentsInChildren<Collider>())
            col.enabled = true;
    }

    public List<bool> GetAvailableDirections()
    {
        List<bool> availability = new List<bool>() { false, false, false, false };
        for (int i = 0; i < 4; i++)
        {
            Collider[] cols = Physics.OverlapBox(movementHitbox[i].bounds.center, movementHitbox[i].bounds.extents, movementHitbox[i].transform.rotation);
            foreach (Collider col in cols)
            {
                if (col.tag == "Level")
                {
                    availability[i] = true;
                }
            }
        }
        return availability;
    }
}
