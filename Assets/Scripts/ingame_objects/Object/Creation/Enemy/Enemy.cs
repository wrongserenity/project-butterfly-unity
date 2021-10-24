using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : Creation
{
    public bool is_player_noticed = false;
    [System.NonSerialized]
    public float notice_range = GlobalVariables.default_notice_range;
    public float forget_range = GlobalVariables.default_forget_range;

    /// 0- forward, 1 - backward, 2 - right, 3 - left
    public Collider[] movementHitbox;

    public char type;
    public int power;
    public int currentLineNum = -1;   // so -1 is for non-existent line number
    Vector3 spawn_position;
    Quaternion spawn_rotation;
    bool is_killed = false;

    public bool isReadyToDeprivate = false;
    List<Material> deprivateMaterials = new List<Material>() { null, null };
    List<Material> damageMaterials = new List<Material>() { null, null };
    public GameObject body;

    bool moving_to_original_position = false;

    public Vector3 playerPos = new Vector3(0f, 0f, 0f);
    public Vector3 ownPos = new Vector3(0f, 0f, 0f);

    public bool deathRequest = false;
    public bool isLogicBlocked = false;
    public bool deprivationActivateRequest = false;
    public bool deprivationActivateLife = false;
    bool isDamagedAnimating = false;
    float damagingAnimationDuration = GlobalVariables.enemies_damaged_animation_duration;


    Image healthBar;
    AudioSource steps;
    AudioSource death;
    public AudioSource playerNotice;

    // Start is called before the first frame update
    public void Start()
    {
        base.Start();
        spawn_position = transform.position;
        spawn_rotation = transform.rotation;
        body = transform.Find("Model").Find("Capsule").gameObject;
        deprivateMaterials[0] = body.GetComponent<MeshRenderer>().material;
        damageMaterials[0] = body.GetComponent<MeshRenderer>().material;

        transform.Find("Interface").GetComponent<Canvas>().worldCamera = gameManager.mainCamera.gameObject.GetComponent<Camera>();
        transform.Find("Interface").GetComponent<Canvas>().enabled = false;
        healthBar = transform.Find("Interface").Find("EnemyHealth").Find("Health").GetComponent<Image>();

        steps = transform.Find("Sounds").Find("steps").GetComponent<AudioSource>();
        death = transform.Find("Sounds").Find("death").GetComponent<AudioSource>();
        playerNotice = transform.Find("Sounds").Find("playerNotice").GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!is_killed && !isLogicBlocked)
        {
            if (steps != null && is_player_noticed)
            {
                if (vel.magnitude > 0.01f)
                {
                    if (!steps.isPlaying)
                        steps.Play();
                }
                else if (steps.isPlaying)
                    steps.Stop();
            }
                

            EnemyLogicProcess();
        }
        if (deathRequest)
        {
            EnemyReload();
            deathRequest = false;
        }
        if (deprivationActivateRequest && !isReadyToDeprivate)
        {
            isReadyToDeprivate = true;
            StartCoroutine(DeprivationReadyAnimation());
            deprivationActivateRequest = false;
        }
        if (deprivationActivateLife && !isReadyToDeprivate)
        {
            isReadyToDeprivate = true;
            StartCoroutine(DeprivationReadyAnimation());
        }
    }

    public virtual void EnemyLogicProcess() {}

    public void EnemyHealthBarAnimation(string toDo)
    {
        if (toDo == "changed")
        {
            if (!transform.Find("Interface").GetComponent<Canvas>().enabled)
                transform.Find("Interface").GetComponent<Canvas>().enabled = true;
            StartCoroutine(BarPulse());
            healthBar.fillAmount = (float)cur_hp / (float)max_hp; 
        }
    }

    public IEnumerator BarPulse()
    {
        float scaleX = 1f;
        float scaleXTarget = scaleX * 1.2f;
        while (scaleX < scaleXTarget)
        {
            scaleX += 0.02f;
            healthBar.gameObject.transform.localScale = new Vector3(scaleX, scaleX, scaleX);
            yield return new WaitForSeconds(0.01f);
        }
        healthBar.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
    }

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
                        weapon.Attack();
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
        for (int i=0; i < power; i++)
        {
            GameObject goES = Resources.Load("Prefabs/Staff/EnergySphere") as GameObject;
            GameObject energySphere = Instantiate(goES, new Vector3(0f, 0f, 0f), new Quaternion(0f, 0f, 0f, 1.0f));
            energySphere.transform.position = transform.position + new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)).normalized * 1.5f;
        }


        GameObject goKP = Resources.Load("Prefabs/Staff/EnemyKillParticles") as GameObject;
        GameObject killingParticles = Instantiate(goKP, new Vector3(0f, 0f, 0f), new Quaternion(0f, 0f, 0f, 1.0f));
        killingParticles.transform.position = transform.position;

        StartCoroutine(DeleteDeathParticles(killingParticles));
        foreach (Renderer ren in GetComponentsInChildren<Renderer>())
            ren.enabled = false;
        DisableCollision();

        transform.Find("Interface").GetComponent<Canvas>().enabled = false;
        death.Play();
        if (steps != null)
            steps.Stop();

    }

    public IEnumerator DeleteDeathParticles(GameObject part)
    {
        yield return new WaitForSeconds(part.GetComponent<ParticleSystem>().main.startLifetime.constantMax);
        Destroy(part);
    }

    public void EnemyReload()
    {
        is_player_noticed = false;

        if (steps != null)
            steps.Stop();
        foreach (Renderer ren in GetComponentsInChildren<Renderer>())
            ren.enabled = true;
        EnableCollision();
        transform.position = spawn_position;
        transform.rotation = spawn_rotation;
        cur_hp = max_hp;
        isReadyToDeprivate = false;

        EnemyHealthBarAnimation("changed");
        transform.Find("Interface").GetComponent<Canvas>().enabled = false;

        body.GetComponent<MeshRenderer>().material = deprivateMaterials[0];

        currentLineNum = -1;

        is_killed = false;
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

    public void CheckDeprivationStatus()
    {
        if (cur_hp <= max_hp * GlobalVariables.deprivateble_hp_percent)
        {
            deprivationActivateRequest = true;
            //print("ready to deprivate");
        }
    }

    public void CheckPlayerNoticing()
    {
        if (!is_player_noticed && GetDistanceToPlayer() <= notice_range)
        {
            gameManager.battleSystem.AddToBattle(this);
            is_player_noticed = true;
            gameManager.AddEnemyToReload(this);
        }
        if (is_player_noticed && GetDistanceToPlayer() >= forget_range)
        {
            gameManager.battleSystem.RemoveEnemy(this);
            is_player_noticed = false;
        }
    }

    public float GetDistanceToPlayer()
    {
        playerPos = gameManager.player.transform.position;
        ownPos = transform.position;
        return (playerPos - ownPos).magnitude;
    }

    public void GiveWeaponToPlayer()
    {
        if (weapon.deprivationWeaponPath != "")
        {
            Weapon.LoadWeaponFrom(weapon.deprivationWeaponPath, gameManager.player, true);
        }
        Kill();
    }

    public void DamagedAnimationPlay()
    {
        if (!isDamagedAnimating)
            StartCoroutine(DamageAnimation(damagingAnimationDuration));
    }

    public IEnumerator DeprivationReadyAnimation()
    {
        float tick = GlobalVariables.enemies_deprivation_ready_ripple_tick_time;
        deprivateMaterials[1] = Resources.Load("LoadMaterials/DeprivationReady", typeof(Material)) as Material;

        int iter = 0;
        while (!is_killed && isReadyToDeprivate)
        {
            body.GetComponent<MeshRenderer>().material = deprivateMaterials[iter];
            iter = (iter + 1) % 2;
            yield return new WaitForSeconds(tick);
        }
        body.GetComponent<MeshRenderer>().material = deprivateMaterials[0];
    }

    public IEnumerator DamageAnimation(float duration)
    {
        isDamagedAnimating = true;
        float tick = GlobalVariables.enemies_damaged_animation_tick_time;
        float curTime = 0f;
        damageMaterials[1] = Resources.Load("LoadMaterials/Damaged", typeof(Material)) as Material;

        int iter = 0;
        while (curTime < duration)
        {
            body.GetComponent<MeshRenderer>().material = damageMaterials[iter];
            iter = (iter + 1) % 2;
            curTime += tick;
            yield return new WaitForSeconds(tick);
        }
        body.GetComponent<MeshRenderer>().material = damageMaterials[0];
        isDamagedAnimating = false;
    }

}
