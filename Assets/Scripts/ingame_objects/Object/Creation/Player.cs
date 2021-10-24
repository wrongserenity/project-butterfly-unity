using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : Creation
{
    Cooldown heal_cooldown;
    Cooldown rewind_cooldown;
    Cooldown teleport_cooldown;
    Cooldown xitonCharge_cooldown;


    public InterfaceSystem interfaceObject;
    Text hpEnergy;

    public int max_energy = GlobalVariables.player_max_energy;

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
    List<string> weapon_path_trace = new List<string>() { };
    string last_deprivated_path = "";
    public string cur_deprivated_weapon_path = "";
    int trace_max_length = GlobalVariables.player_trace_max_length;
    int position_rewind_offset = GlobalVariables.player_position_rewind_offset;
    bool isLineCleaning = false;
    int tracing_step = GlobalVariables.player_tracing_step;
    int cur_tracing_step = 0;

    public Trigger actionObj; // there should be Weapon class, probably (may be trigger object)

    public Vector3 telep = new Vector3(0f, 0f, 0f);
    Vector3 dir_m;
    public Vector3 dir_v = new Vector3(0f, 0f, 0f);
    public Vector3 prev_dir_m = new Vector3(0f, 0f, 0f);

    public int cur_energy = 0;
    public int curXitonCharge = 0;
    public int maxXitonCharge = GlobalVariables.player_max_xiton_charge;

    // animation variables
    // 0 - forward, 1 - backward, 2 - to the right, 3 - to the left (inversed right)
    int anim_walk_direction = 0;

    public List<Vector3> teleportTriggerRequest = new List<Vector3>() { };
    public List<Vector3> teleportRequest = new List<Vector3>() { };

    DeprivationSystem deprivationSystem;
    public Weapon deprivatedWeapon;

    SphereCollider xitonChargeCollider;

    LineRenderer rewindLine;


    // Start is called before the first frame update
    void Start()
    {
        base.Start();

        max_hp = GlobalVariables.player_max_hp;
        speed_vel = GlobalVariables.player_max_speed;

        heal_cooldown = gameManager.cooldownSystem.AddCooldown(this, GlobalVariables.player_teleport_cooldown);
        rewind_cooldown = gameManager.cooldownSystem.AddCooldown(this, GlobalVariables.player_position_rewind_cooldown);
        teleport_cooldown = gameManager.cooldownSystem.AddCooldown(this, GlobalVariables.player_teleport_cooldown);
        xitonCharge_cooldown = gameManager.cooldownSystem.AddCooldown(this, GlobalVariables.player_xiton_charge_cooldown);


        Weapon.LoadWeaponFrom("Prefabs/Weapons/RheaSword", this, false);

        hpEnergy = GameObject.Find("hp_nrj").GetComponent<Text>();
        deprivationSystem = transform.Find("DeprivationSystem").GetComponent<DeprivationSystem>();
        xitonChargeCollider = transform.Find("XitonChargeSphere").GetComponent<SphereCollider>();

        interfaceObject = transform.Find("Interface").GetComponent<InterfaceSystem>();
        interfaceObject.LoadInterface();

        rewindLine = gameManager.rewindLineRenderer;

        PlayerRespawn();
    }

    //TODO: add init and ready from godot

    // for future ver.: game should save hp and energy on level's start
    public void PlayerRespawn()
    {
        cur_hp = max_hp;
        cur_energy = 0;
        deprivatedWeapon = null;
        if(gameManager.levelContainer.transform.childCount != 0)
            teleportTriggerRequest.Add(gameManager.levelContainer.transform.GetChild(0).Find("SpawnPosition").position);
        else
        {
            Debug.Log("PlayerRespawn: there is no level in levelContainer");
        }
        interfaceObject.BarAnimation("health", "changed", 0f);
    }

    public void PlayerReloadToCheckPoint()
    {
        teleportTriggerRequest.Add(gameManager.currentCheckpoint);
        interfaceObject.BarAnimation("health", "changed", 0f);
        if (deprivatedWeapon != null)
        {
            deprivatedWeapon.UntieWeapon().DestroyWeapon();
            deprivatedWeapon = null;
        }
        
    }

    // there should be weapon instead true in if costruction
    void Attack()
    {
        if (deprivatedWeapon == null)
        {
            if (weapon != null)
                weapon.Attack();
        }
        else 
            deprivatedWeapon.Attack();
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
        interfaceObject.BarAnimation("energy", "changed", 0f);
    }

    public void XitonTransfer(int value)
    {
        curXitonCharge += value;

        if (value > 0)
        {
            dataRec.AddTo("xiton_charged", value);
        }
        else
        {
            dataRec.AddTo("xiton_spent", -value);
        }

        if (curXitonCharge > maxXitonCharge)
            curXitonCharge = maxXitonCharge;

        interfaceObject.BarAnimation("xiton", "changed", 0f);
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

    bool XitonChargeAnimation()
    {
        bool hasSource = false;
        foreach (Collider col in Physics.OverlapSphere(xitonChargeCollider.bounds.center, xitonChargeCollider.radius))
        { 
            if (col.tag == "GlowingObject" && col.GetComponent<GlowingObject>().SpawnParticlesRequest())
            {
                hasSource = true;
            }
        }
        return hasSource;
    }

    void PlayerAction()
    {
        if (deprivationSystem.CheckReadyEnemy() && deprivatedWeapon == null)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                deprivationSystem.DeprivateClothestWeapon();
                soundSystem.PlayOnce("equipSound");
            }      
        }
        else
        {
            if (actionObj != null)
            {
                if (!actionObj.isIterative)
                {
                    if (Input.GetKeyDown(KeyCode.E))
                        actionObj.Activate();
                }
                else
                    actionObj.Activate();
            }
            else
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
            if (position_trace.Count == 0)
            {
                position_trace.Add(pos);
                health_trace.Add(cur_hp);
                weapon_path_trace.Add(cur_deprivated_weapon_path);
                cur_tracing_step = tracing_step;
            }
            else
            {
                if (pos != position_trace[position_trace.Count-1] || cur_hp != health_trace[health_trace.Count - 1] || cur_deprivated_weapon_path != weapon_path_trace[weapon_path_trace.Count - 1])
                {
                    position_trace.Add(pos);
                    health_trace.Add(cur_hp);
                    if (last_deprivated_path != cur_deprivated_weapon_path)
                        weapon_path_trace.Add(cur_deprivated_weapon_path);
                    else
                        weapon_path_trace.Add("");

                    if (cur_deprivated_weapon_path == "")
                        last_deprivated_path = "";
                    cur_tracing_step = tracing_step;
                }
            }
            
        }
    }

    void CleanTrace()
    {
        position_trace.Clear();
        health_trace.Clear();
        weapon_path_trace.Clear();
    }

    public void Teleport(Vector3 new_coordinates, bool clear_trace)
    {
        transform.position = new_coordinates;
        if (clear_trace)
            CleanTrace();
    }

    bool DoRewind()
    {
        if (rewind_cooldown.Try())
        {
            int count = position_trace.Count;
            if (count > 0 && cur_energy >= rewind_cost)
            {
                StartCoroutine(SmoothTeleport(position_trace[count - 1], (float)rewind_cooldown.timeout * 0.8f));
                //Teleport(position_trace[count-1], false);
                position_trace.RemoveAt(count-1);
                if (health_trace[count - 1] - cur_hp != 0)
                    ProcessHp(health_trace[count - 1] - cur_hp);
                health_trace.RemoveAt(count-1);
                if (cur_deprivated_weapon_path != weapon_path_trace[count - 1])
                {
                    if (weapon_path_trace[count - 1] == "")
                    {
                        //deprivatedWeapon.UntieWeapon().DestroyWeapon();
                    }
                    else
                    {
                        soundSystem.PlayOnce("equipSound");
                        Weapon.LoadWeaponFrom(weapon_path_trace[count - 1], this, true);
                        ClearLastDeprivatedWith(weapon_path_trace[count - 1]);
                    }
                }
                weapon_path_trace.RemoveAt(count - 1);


                EnergyTransfer(-rewind_cost);

                dataRec.AddTo("spent_on_rewind", rewind_cost);
                return true;
            }
            else
            {
                if(gameManager.isTimeScaled)
                    gameManager.ReturnTimeScale();
                stateMachine.RemoveState("rewinding");
            }
        }
        else
        {
            if (position_trace.Count > 0 && cur_energy >= rewind_cost)
            {
                return true;
            }
        }
        return false;
    }

    public void ClearLastDeprivatedWith(string path)
    {
        bool isEmptyFound = false;
        for (int i = weapon_path_trace.Count - 1; i >= 0; i--)
        {
            if (!isEmptyFound)
            {
                if (weapon_path_trace[i] == path)
                    weapon_path_trace[i] = "";
                else
                    isEmptyFound = true;
            }
        }
        last_deprivated_path = path;
    }

    public IEnumerator SmoothTeleport(Vector3 destination, float duration)
    {
        int stepNumber = 5;
        float curTime = 0f;
        Vector3 curVector = transform.position;
        Vector3 stepVector = (destination - curVector) / (stepNumber + 1);
        float stepTime = duration / stepNumber;
        
        while (curTime < duration)
        {
            curVector += stepVector;
            teleportRequest.Add(curVector);
            curTime += stepTime;
            yield return new WaitForSecondsRealtime(stepTime);
        }
    }
    
    void RewindVisualEffect()
    {
        rewindLine.positionCount = position_trace.Count;
        rewindLine.SetPositions(position_trace.ToArray());
    }

    void ShortDistanceTeleport(Vector3 pos, Vector3 dir_m_)
    {
        if (cur_energy >= teleport_cost)
        {
            StartCoroutine(AnimateShortDistanceTeleport(pos, dir_m_));

            telep = pos + dir_m_.normalized * teleport_distance;
            // disables enemy collision for passing through them
            // immediately enables after
            if (dir_m_.magnitude > 0)
            {
                foreach (Line line in gameManager.battleSystem.lines)
                {
                    foreach(Enemy enemy in line.enemies)
                    {
                        if (enemy != null)
                            enemy.DisableCollision();
                    }
                }
                controller.Move((telep - pos));
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

    public IEnumerator AnimateShortDistanceTeleport(Vector3 pos, Vector3 dir_m_)
    {
        List<GameObject> circles = new List<GameObject>() { };
        float step = teleport_distance / (GlobalVariables.short_distance_teleport_circle_count);
        GameObject go = Resources.Load("Prefabs/Player/TeleportCircle") as GameObject;
        for (int i = 0; i < GlobalVariables.short_distance_teleport_circle_count; i++)
        {
            circles.Add(Instantiate(go, new Vector3(0f, 0f, 0f), new Quaternion(0f, 0f, 0f, 1.0f)));
            circles[circles.Count - 1].transform.position = pos + (i + 1) * step * dir_m_.normalized;
            circles[circles.Count - 1].transform.rotation = Quaternion.LookRotation(dir_m_.normalized);
            yield return new WaitForSeconds(GlobalVariables.short_distance_teleport_circle_timestep);

        }
        yield return new WaitForSeconds(GlobalVariables.short_distance_teleport_circle_lifetime);
        foreach(GameObject circle in circles)
        {
            Destroy(circle);
            yield return new WaitForSeconds(GlobalVariables.short_distance_teleport_circle_timestep);
        }
        circles.Clear();

    }

    public IEnumerator CleanTraceLine(float stepTime)
    {
        isLineCleaning = true;
        for (int i = rewindLine.positionCount; i>=0; i--)
        {
            if (!stateMachine.IsActive("rewindRequest") && !stateMachine.IsActive("rewinding") && !stateMachine.IsActive("trace_pause"))
                rewindLine.positionCount = i;
            yield return new WaitForSeconds(stepTime);
        }
        isLineCleaning = false;
    }

    public override void DamageImmuneAnimation(float duration)
    {
        base.DamageImmuneAnimation(duration);
        interfaceObject.BarAnimation("health", "immune", duration);
    }

    public Vector3 GetVeiwPoint()
    {
        return dir_v;
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
        groundPlane.Translate(new Vector3(0f, -transform.position.y, 0f));
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

        if (stateMachine.IsActive("rewindRequest"))
        {
            
            if (DoRewind())
            {
                RewindVisualEffect();
                stateMachine.AddState("rewinding");
                gameManager.SetTimeScale(0.3f, true);
            }
            else
            {
                if (gameManager.isTimeScaled)
                    gameManager.ReturnTimeScale();
            }
        }

        if (!stateMachine.IsActive("rewindRequest") && !stateMachine.IsActive("rewinding") && !stateMachine.IsActive("trace_pause"))
        {
            TraceRecording();
            if (!isLineCleaning && rewindLine.positionCount > 0)
                StartCoroutine(CleanTraceLine(0.1f));
        }

        if (stateMachine.IsActive("parrying") && !weapon.GetComponent<RheasSword>().parry_window_cooldown.in_use)
        {
            stateMachine.RemoveState("parrying");
            stateMachine.AddState("blocking");
        }


        if (stateMachine.IsActive("parrySoundReq"))
        {
            gameManager.mainCamera.ChangeChromaticAberrationIntencityFor(1f, GlobalVariables.player_parry_window_duration);
            gameManager.SetTimeScaleFor(0.3f, GlobalVariables.player_parry_window_duration);
            weapon.gameObject.GetComponent<RheasSword>().parryDamageScaleCooldown.Update();
            soundSystem.PlayOnce("parrySound");
            stateMachine.RemoveState("parrySoundReq");
        }

        if (stateMachine.IsActive("blockSoundReq"))
        {
            soundSystem.PlayOnce("blockSound");
            stateMachine.RemoveState("blockSoundReq");
        }

        if (stateMachine.IsActive("chargingRequest"))
        {
            if (XitonChargeAnimation())
            {
                stateMachine.AddState("charging");
                if (xitonCharge_cooldown.Try())
                    XitonTransfer(1);
            }
            else
                stateMachine.RemoveState("charging");
        }

        if (stateMachine.IsActive("teleportRequest"))
        {
            ShortDistanceTeleport(pos, prev_dir_m);
            soundSystem.PlayOnce("teleportSound");
            stateMachine.RemoveState("teleportRequest");
        }

        if (teleportTriggerRequest.Count > 0)
        {
            Teleport(teleportTriggerRequest[teleportTriggerRequest.Count - 1], true);
            teleportTriggerRequest.Clear();
        }

        if (teleportRequest.Count > 0)
        {
            Teleport(teleportRequest[teleportRequest.Count - 1], false);
            teleportRequest.Clear();
        }

        if (stateMachine.IsActive("death"))
        {
            PlayerRespawn();
            stateMachine.RemoveState("death");
        }

        if (stateMachine.IsActive("checkPointLoad"))
        {
            PlayerReloadToCheckPoint();
            stateMachine.RemoveState("checkPointLoad");
        }

        //hpEnergy.text = "hp: "+ cur_hp + "\nenergy: " + cur_energy + "\nxiton: " + curXitonCharge;
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
            stateMachine.AddState("rewindRequest");
        }
        else
        {
            if (gameManager.isTimeScaled)
                gameManager.ReturnTimeScale();
            stateMachine.RemoveState("rewindRequest");
            if (stateMachine.IsActive("rewinding"))
            {
                stateMachine.RemoveState("rewinding");
            }
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Attack();
        }

        if (Input.GetKeyDown(KeyCode.Space) && teleport_cooldown.Try())
        {
            stateMachine.AddState("teleportRequest");
        }

        if (Input.GetKey(KeyCode.Mouse1))
        {
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                if (deprivatedWeapon == null)
                {
                    weapon.AlternateAttack();
                }
                else
                {
                    deprivatedWeapon.AlternateAttack();
                }
            }  
        }
        else
        {
            stateMachine.RemoveState("parrying");
            stateMachine.RemoveState("blocking");
        }

        if (Input.GetKey(KeyCode.LeftShift))
            stateMachine.AddState("chargingRequest");
        else
        {
            stateMachine.RemoveState("chargingRequest");
            stateMachine.RemoveState("charging");
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!gameManager.pauseMenu.isPaused)
                gameManager.pauseMenu.Pause();
            else
                gameManager.pauseMenu.Resume();
        }

        if (Input.GetKeyDown(KeyCode.R))
            gameManager.ReloadToCheckPoint();
    }

    public void Ping()
    {
        print("Player ping!");
    }
}
