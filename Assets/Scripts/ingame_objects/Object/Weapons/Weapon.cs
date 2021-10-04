using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public GameManager gameManager;
    public CooldownSystem cooldownSystem;
    public Collider hitBox;

    public int damage;
    public bool damaged = false;
    Creation owner_;

    public float pushForce;

    float lastTime = 0f;
    float curTime = 0f;

    // 0 - ready to attack
    // 1 - attack preparing
    // 2 - realtime using
    // 3 - weapon returning
    int state = 0;
    List<Cooldown> cooldown_list = new List<Cooldown>() {};
    List<float> cooldown_list_dur = new List<float>() { };


    public void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        cooldownSystem = gameManager.cooldownSystem;

    }

    public void GiveWeaponTo(Creation creation)
    {
        owner_ = creation;
    }

    // calls only by Enemies
    public virtual void EnemyAttack() { }

    // calls only by Player
    public virtual void PlayerAttack() { }

    // what to do in hitting moment
    public virtual void Using() { }

    public void Impulse(Creation body, float multiplier)
    {
        Vector3 target_pos = body.transform.position;
        target_pos.y = owner_.transform.position.y;
        body.GetImpulse((target_pos - owner_.transform.position).normalized, pushForce * multiplier);
    }

    void StateProcess()
    {
        if (state == 0)
        {
            if (owner_.rotation_lock)
                owner_.rotation_lock = false;
            if (owner_.movement_lock)
                owner_.movement_lock = false;
        }
        else
        {
            if (!owner_.rotation_lock)
                owner_.rotation_lock = true;
            if (!owner_.movement_lock)
                owner_.movement_lock = true;

            if (state == 2 && !damaged)
                Using();

            
            if (cooldown_list_dur[state] > 0.001)
            {
                if (!cooldown_list[state].in_use)
                {
                    NextState();
                }
            }
            else
            {
                NextState();
            }
        }
    }
    
    void NextState()
    {
        state = (state + 1) % 4;
        cooldown_list[state].Try();
    }

    public void AttackRequest()
    {
        if (state == 0)
        {
            NextState();
            damaged = false;
        }
    }

    public bool BuildCooldownList(List<float> cooldown_time_list)
    {
        cooldown_list_dur = cooldown_time_list;
        if (cooldown_time_list.Count != 4)
        {
            return false;
        }
        foreach (float sec in cooldown_time_list)
        {
            cooldown_list.Add(cooldownSystem.AddCooldown(this, sec));
        }
        return true;
    }

    void FixedUpdate()
    {
        StateProcess();
    }

    public bool DamageAllInHitbox(bool isFromPlayer)
    {
        bool isHit = false;
        Collider[] cols = Physics.OverlapBox(hitBox.bounds.center, hitBox.bounds.extents, hitBox.transform.rotation);
        foreach (Collider col in cols)
        {
            if (isFromPlayer)
            { 
                Enemy enemy = col.GetComponent<Enemy>();
                if (enemy != null)
                {
                    isHit = true;
                    enemy.ProcessHp(-damage);
                    Impulse(enemy, 1f);
                }
            }
            else
            {
                Player player = col.GetComponent<Player>();
                if (player != null)
                {
                    if (player.stateMachine.IsActive("parrying"))
                    {
                        print("parrying!");
                    }else if (player.stateMachine.IsActive("blocking"))
                    {
                        print("blocked!");
                        isHit = true;
                        player.ProcessHp(-Mathf.FloorToInt(damage/2));
                        Impulse(player, 1f);
                        damaged = true;
                        return true;
                    }
                    else
                    {
                        isHit = true;
                        player.ProcessHp(-damage);
                        Impulse(player, 1f);
                        damaged = true;
                        return true;
                    }
                    
                }
            }

        }
        return isHit;
    }
}
