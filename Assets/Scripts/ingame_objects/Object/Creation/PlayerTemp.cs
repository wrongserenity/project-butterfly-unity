using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTemp : Creation
{
    public GameManager gameManager;
    DataRecorder dataRec;

    Cooldown player_heal_cooldown;
    Cooldown player_rewind_cooldown;

    int max_energy = GlobalVariables.player_max_energy;
    float max_speed = GlobalVariables.player_max_speed;

    public GameObject hit_box;
    public Camera camera_obj;
    public GameObject hitbox_bottom; // suppose it for rewind trigger, and may be no need in it

    public GameObject sound;
    public GameObject state_machine;

    float teleport_distance = GlobalVariables.player_teleport_distance;
    int heal_cost = GlobalVariables.player_heal_cost;
    int teleport_cost = GlobalVariables.player_teleport_cost;
    int position_rewind_cost = GlobalVariables.player_rewind_cost;

    public Vector3 pos;

    List<Vector3> position_trace;
    int position_trace_max_length = GlobalVariables.player_trace_max_length;
    int position_rewind_offset = GlobalVariables.player_position_rewind_offset;
    bool is_falling = false;

    GameObject action_obj; // there should be Weapon class, probably (may be trigger object)

    public Vector3 telep = new Vector3(0f, 0f, 0f);
    Vector3 dir_m = new Vector3(0f, 0f, 0f);
    public Vector3 dir_v = new Vector3(0f, 0f, 0f);
    public Vector3 prev_dir_m = new Vector3(0f, 0f, 0f);

    public int cur_energy = 0;

    // animation variables
    // 0 - forward, 1 - backward, 2 - to the right, 3 - to the left (inversed right)
    int anim_walk_direction = 0;


    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        dataRec = gameManager.dataRecorder;
        max_hp = GlobalVariables.player_max_hp;
        cur_hp = max_hp;
        speed_vel = GlobalVariables.player_max_speed;

        player_heal_cooldown = gameManager.cooldownSystem.AddCooldown(this, GlobalVariables.player_teleport_cooldown);
        player_rewind_cooldown = gameManager.cooldownSystem.AddCooldown(this, GlobalVariables.player_position_rewind_cooldown);

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
    void Attack() {
        if (weapon != null)
        {
            weapon.PlayerAttack();
        }
    }

    // TODO:  idea: make it return boolean, with returning false if had no enoght energy
    void EnergyTransfer(int value)
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
            // state_machine.add_state('healing');
            if (player_heal_cooldown.Try())
            {
                ProcessHp(1);
                dataRec.AddTo("restored_hp", 1);
                EnergyTransfer(-heal_cost);
            }
        }
    }

    void PlayerAction()
    {
        if (action_obj)
        {
            // 'not action_obj.is_iterative' instead true
            if (true)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    // action_obj.activate()
                }
            }
            else
            {
                // action_obj.activate()
            }
        }
        else
        {
            HealSelf();
        }
    }

    void ChangeUseMode(GameObject obj)
    {
        if (action_obj != obj && !Input.GetKey("E"))
        {
            action_obj = obj;
            // print that changed
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
    void Animate(){}

    void TraceRecording()
    {
        while (position_trace.Count >= position_trace_max_length)
        {
            position_trace.RemoveAt(0);
        }
        if (!is_falling)
        {
            while (position_trace.Count >= position_rewind_offset)
            {
                position_trace.RemoveAt(0);
            }
        }
        position_trace.Add(pos);
    }

    void CleanPositionTrace()
    {
        position_trace.Clear();
    }

    void Teleport(Vector3 new_coordinates, bool clear_trace)
    {
        transform.position = new_coordinates;
        if (clear_trace)
            CleanPositionTrace();
    }

    void DoRewind()
    {
        if (player_rewind_cooldown.Try())
        {
            if (position_trace.Count > 0 && cur_energy >= position_rewind_cost)
            {
                Teleport(position_trace[-1], false);
                position_trace.RemoveAt(-1);
                EnergyTransfer(-position_rewind_cost);

                dataRec.AddTo("spent_on_rewind", position_rewind_cost);
                // state_machine.add_state('rewining')
            }
            else
            {
                // state_machine.remove_state('rewinding')
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
            if (telep.magnitude > 0)
            {
                // for line in Global.battle.lines:
                //      for enem in line:
                //          enem.disable_collision()
                controller.Move(telep - pos);
                telep *= 0;
                // for line in Global.battle.lines:
                //      for enem in line:
                //          enem.enable_collision()
                if (dir_m_.magnitude > 0)
                {
                    EnergyTransfer(-teleport_cost);
                    dataRec.AddTo("shifted", 1);
                }
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 dir_m = new Vector3(0f, 0f, 0f);
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
            dir_v = - transform.position + pointToLook;
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
            // state_machine.add_state('walking')
        }
        else
        {
            // state_machine.remove_state('walking')
        }
        // sound.check()

        // if state_machine.is_active('healing'):
		// if not $"model/2d_animation/heal_circle/animation".is_playing():
		//      $"model/2d_animation/heal_circle/animation".play("healing")
        // else:
		//      if $"model/2d_animation/heal_circle/animation".is_playing():
        //          $"model/2d_animation/heal_circle/animation".stop()
        //          $"model/2d_animation/heal_circle".opacity = 0
    }

    void Update()
    {
        //if Input.is_action_just_pressed("ui_cancel"):
        //  Global.game.add_interface_obj(["res://player_interfaces/pause_menu.tscn"])

        if (Input.GetKey(KeyCode.E))
        {
            Debug.Log("pressed");
            PlayerAction();
        }
        else
        {
            // state_machine.remove_state("healing")
            // state_machine.remove_state("rewinding")
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Debug.Log("pressed");
            Attack();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("pressed");
            ShortDistanceTeleport(pos, dir_m);
        }
    }
}
