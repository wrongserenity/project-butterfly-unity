using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : Creation
{
    Cooldown heal_cooldown;
    Cooldown rewind_cooldown;
    Cooldown teleport_cooldown;

    Cooldown parry_window_cooldown;

    Text hpEnergy;

    int max_energy = GlobalVariables.player_max_energy;

    public GameObject hit_box;
    public Camera camera_obj;
    public GameObject hitbox_bottom; // suppose it for rewind trigger, and may be no need in it

    public SoundSystem soundSystem;
    public StateMachine stateMachine;

    float teleport_distance = GlobalVariables.player_teleport_distance;
    int heal_cost = GlobalVariables.player_heal_cost;
    int teleport_cost = GlobalVariables.player_teleport_cost;
    int rewind_cost = GlobalVariables.player_rewind_cost;

    public Vector3 pos;

    List<Vector3> position_trace = new List<Vector3>() { };
    List<int> health_trace = new List<int>() { };
    int trace_max_length = GlobalVariables.player_trace_max_length;
    int position_rewind_offset = GlobalVariables.player_position_rewind_offset;
    bool is_falling = false;
    int tracing_step = GlobalVariables.player_tracing_step;
    int cur_tracing_step = 0;

    public Trigger actionObj; // there should be Weapon class, probably (may be trigger object)

    public Vector3 telep = new Vector3(0f, 0f, 0f);
    Vector3 dir_m;
    public Vector3 dir_v = new Vector3(0f, 0f, 0f);
    public Vector3 prev_dir_m = new Vector3(0f, 0f, 0f);

    public int cur_energy = 0;

    // animation variables
    // 0 - forward, 1 - backward, 2 - to the right, 3 - to the left (inversed right)
    int anim_walk_direction = 0;

    bool telepRequest = false;
    public List<Vector3> teleportTriggerRequest = new List<Vector3>() { };


    // Start is called before the first frame update
    void Start()
    {
        base.Start();

        max_hp = GlobalVariables.player_max_hp;
        cur_hp = max_hp;
        speed_vel = GlobalVariables.player_max_speed;

        heal_cooldown = gameManager.cooldownSystem.AddCooldown(this, GlobalVariables.player_teleport_cooldown);
        rewind_cooldown = gameManager.cooldownSystem.AddCooldown(this, GlobalVariables.player_position_rewind_cooldown);
        teleport_cooldown = gameManager.cooldownSystem.AddCooldown(this, GlobalVariables.player_teleport_cooldown);
        parry_window_cooldown = gameManager.cooldownSystem.AddCooldown(this, GlobalVariables.player_parry_window_duration);

        GameObject go = Resources.Load("Prefabs/Weapons/RheaSword") as GameObject;
        weapon = Instantiate(go, new Vector3(0f, 0f, 0f), transform.rotation).GetComponent<Weapon>();
        weapon.transform.SetParent(this.transform, false);
        weapon.GiveWeaponTo(this);

        cur_energy = 1000;

        hpEnergy = GameObject.Find("hp_nrj").GetComponent<Text>();
    }

    //TODO: add init and ready from godot

    // for future ver.: game should save hp and energy on level's start
    void PlayerReload()
    {
        cur_hp = max_hp;
        cur_energy = 0;
        // Teleport(Global.level_start_point.translation(), true)
    }

    // there should be weapon instead true in if costruction
    void Attack()
    {
        if (weapon != null)
        {
            weapon.PlayerAttack();
        }
    }

    // TODO:  idea: make it return boolean, with returning false if had no enoght energy
    public void EnergyTransfer(int value)
    {
        if (value > 0)
        {
            dataRec.AddTo("energy_collected", value);
        }
        else
        {
            dataRec.AddTo("energy_spent", -value);
        }

        cur_energy += value;
        if (cur_energy > max_energy)
        {
            cur_energy = max_energy;
        }
    }

    void HealSelf()
    {
        if (cur_hp < max_hp && cur_energy >= heal_cost)
        {
            stateMachine.AddState("healing");
            if (heal_cooldown.Try())
            {
                ProcessHp(1);
                dataRec.AddTo("restored_hp", 1);
                EnergyTransfer(-heal_cost);
            }
        }
    }

    void PlayerAction()
    {
        if (actionObj != null)
        {
            if (!actionObj.isIterative)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    actionObj.Activate();
                }
            }
            else
            {
                actionObj.Activate();
            }
        }
        else
        {
            HealSelf();
        }
    }

    public void ChangeUseMode(Trigger obj)
    {
        if (actionObj != obj && !Input.GetKey(KeyCode.E))
        {
            actionObj = obj;
        }
    }

    void AnimationControl(Vector3 dir_move, Vector3 dir_view)
    {
        dir_move.y = 0;
        dir_view.y = 0;
        Vector3 a = dir_move.normalized;
        Vector3 b = dir_view.normalized;

        float abs_angle = Mathf.Abs(Mathf.Acos(a.x * b.x + a.z * b.z));
        if (abs_angle < 0.7)
        {
            // about 40 degree
            if (anim_walk_direction != 0)
            {
                anim_walk_direction = 0;
            }
        }
        else if (abs_angle < 1.9)
        {
            if (Vector3.Dot(b, new Vector3(a.z, 0, -a.x)) > 0)
            {
                if (anim_walk_direction != 2)
                {
                    anim_walk_direction = 2;
                }
            }
            else
            {
                if (anim_walk_direction != 3)
                {
                    anim_walk_direction = 3;
                }
            }
        }
        else if (anim_walk_direction != 1)
        {
            anim_walk_direction = 1;
        }
    }

    // method processing all animation
    // TODO: use anim_walk_direction there
    // seems good to interpolate speed of animation on beginning and end of it
    void Animate() { }

    void TraceRecording()
    {
        
        while (position_trace.Count >= trace_max_length)
        {
            position_trace.RemoveAt(0);
            health_trace.RemoveAt(0);
        }
        /// if (!is_falling)
        ///     while (position_trace.Count >= position_rewind_offset)
        ///         position_trace.RemoveAt(0);
        if (cur_tracing_step != 0)
            cur_tracing_step--;
        else 
        { 
            position_trace.Add(pos);
            health_trace.Add(cur_hp);
            cur_tracing_step = tracing_step;
        }
    }

    void CleanTrace()
    {
        position_trace.Clear();
        health_trace.Clear();
    }

    public void Teleport(Vector3 new_coordinates, bool clear_trace)
    {
        transform.position = new_coordinates;
        if (clear_trace)
            CleanTrace();
    }

    void DoRewind()
    {
        if (rewind_cooldown.Try())
        {
            int count = position_trace.Count;
            if (count > 0 && cur_energy >= rewind_cost)
            {
                Teleport(position_trace[count-1], false);
                position_trace.RemoveAt(count-1);
                ProcessHp(health_trace[count - 1] - cur_hp);
                health_trace.RemoveAt(count-1);
                EnergyTransfer(-rewind_cost);

                dataRec.AddTo("spent_on_rewind", rewind_cost);
                stateMachine.AddState("rewinding");
            }
            else
            {
                stateMachine.RemoveState("rewinding");
            }
        }
    }

    void ShortDistanceTeleport(Vector3 pos, Vector3 dir_m_)
    {
        if (cur_energy >= teleport_cost)
        {
            telep = pos + dir_m_.normalized * teleport_distance;
            // disables enemy collision for passing through them
            // immediately enables after
            if (dir_m_.magnitude > 0)
            {
                foreach (Line line in gameManager.battleSystem.lines)
                {
                    foreach(Enemy enemy in line.enemies)
                    {
                        enemy.DisableCollision();
                    }
                }
                controller.Move((telep - pos) * Time.deltaTime);
                telep *= 0;
                foreach (Line line in gameManager.battleSystem.lines)
                {
                    foreach (Enemy enemy in line.enemies)
                    {
                        enemy.EnableCollision();
                    }
                }
                EnergyTransfer(-teleport_cost);
                dataRec.AddTo("shifted", 1);
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        dir_m = new Vector3(0f, 0f, 0f);
        pos = transform.position;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        dir_m = new Vector3(horizontal, 0f, vertical).normalized;

        Ray cameraRay = camera_obj.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayLength;

        if (groundPlane.Raycast(cameraRay, out rayLength))
        {
            Vector3 pointToLook = cameraRay.GetPoint(rayLength);
            Debug.DrawLine(cameraRay.origin, pointToLook, Color.cyan);

            transform.LookAt(new Vector3(pointToLook.x, transform.position.y, pointToLook.z));
            dir_v = -transform.position + pointToLook;
            dir_v.y = 0;
        }
        ProcessMovement(dir_m, dir_v, new List<bool>());

        if (dir_m.magnitude > 0)
        {
            prev_dir_m = dir_m;
        }
        AnimationControl(prev_dir_m, dir_v);

        FallingOutCheck(pos);

        if (dir_m.magnitude > 0)
        {
            stateMachine.AddState("walking");
        }
        else
        {
            stateMachine.RemoveState("walking");
        }
        soundSystem.Check();

        if (stateMachine.IsActive("healing"))
        {
            // if not $"model/2d_animation/heal_circle/animation".is_playing():
            //      $"model/2d_animation/heal_circle/animation".play("healing")
            // else:
            //      if $"model/2d_animation/heal_circle/animation".is_playing():
            //          $"model/2d_animation/heal_circle/animation".stop()
            //          $"model/2d_animation/heal_circle".opacity = 0
        }

        if (stateMachine.IsActive("rewinding"))
            DoRewind();

        if (!stateMachine.IsActive("rewinding") && !stateMachine.IsActive("trace_pause"))
            TraceRecording();

        if (stateMachine.IsActive("parrying") && !parry_window_cooldown.in_use)
        {
            stateMachine.RemoveState("parrying");
            stateMachine.AddState("blocking");
        }




        if (telepRequest)
        {
            ShortDistanceTeleport(pos, dir_m);
            telepRequest = false;
        }
        if (teleportTriggerRequest.Count > 0)
        {
            Teleport(teleportTriggerRequest[teleportTriggerRequest.Count - 1], true);
            teleportTriggerRequest.Clear();
        }

        hpEnergy.text = "hp: "+ cur_hp + "   energy: " + cur_energy;
    }

    void Update()
    {
        //if Input.is_action_just_pressed("ui_cancel"):
        //  Global.game.add_interface_obj(["res://player_interfaces/pause_menu.tscn"])

        if (Input.GetKey(KeyCode.E))
            PlayerAction();
        else
            stateMachine.RemoveState("healing");

        if (Input.GetKey(KeyCode.F))
        {
            stateMachine.AddState("rewinding");
        }
        else
            stateMachine.RemoveState("rewinding");

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Attack();
        }

        if (Input.GetKeyDown(KeyCode.Space) && teleport_cooldown.Try())
        {
            telepRequest = true;
        }

        if (Input.GetKey(KeyCode.Mouse1))
        {
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                if (parry_window_cooldown.Try())
                    stateMachine.AddState("parrying");
                else
                    stateMachine.AddState("blocking");
            }  
        }
        else
        {
            stateMachine.RemoveState("parrying");
            stateMachine.RemoveState("blocking");
        }
    }

    public void Ping()
    {
        print("Player ping!");
    }
}
