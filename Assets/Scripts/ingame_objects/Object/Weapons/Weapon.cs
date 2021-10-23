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
    public string deprivationWeaponPath = "";

    public float pushForce;

    float lastTime = 0f;
    float curTime = 0f;

    // 0 - ready to attack
    // 1 - attack preparing
    // 2 - realtime using
    // 3 - weapon returning
    public int state = 0;
    List<Cooldown> cooldown_list = new List<Cooldown>() {};
    List<float> cooldown_list_dur = new List<float>() { };

    // for visual effects
    public GameObject attackSprite;
    public float fadeStepSec = 0.01f;


    public Creation GetOwner()
    {
        return owner_;
    }

    public void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        cooldownSystem = gameManager.cooldownSystem;
    }

    public void GiveWeaponTo(Creation creation)
    {
        owner_ = creation;
    }

    public static void LoadWeaponFrom(string pathInResources, Creation owner, bool isDeprivative)
    {
        if (!isDeprivative)
        {
            GameObject go = Resources.Load(pathInResources) as GameObject;
            owner.weapon = Instantiate(go, new Vector3(0f, 0f, 0f), owner.transform.Find("Model").transform.Find("weapon").transform.rotation).GetComponent<Weapon>();
            owner.weapon.transform.SetParent(owner.transform.Find("Model").transform.Find("weapon").transform, false);
            owner.weapon.GiveWeaponTo(owner);
        }
        else
        {
            Player player = owner.GetComponent<Player>();
            if (player.deprivatedWeapon == null)
            {
                GameObject go = Resources.Load(pathInResources) as GameObject;
                player.deprivatedWeapon = Instantiate(go, new Vector3(0f, 0f, 0f), new Quaternion(0f, 0f, 0f, 1.0f)).GetComponent<Weapon>();
                player.deprivatedWeapon.transform.SetParent(player.transform.Find("Model").transform.Find("weapon").transform, false);
                player.deprivatedWeapon.GiveWeaponTo(player);
                player.cur_deprivated_weapon_path = pathInResources;

                if (player.deprivatedWeapon.GetComponent<Firethrower>())
                    GameObject.Find("GameManager").GetComponent<GameManager>().dataRecorder.AddTo("depr_flamethrower", 1);

                if (player.deprivatedWeapon.GetComponent<GravityBomb>())
                    GameObject.Find("GameManager").GetComponent<GameManager>().dataRecorder.AddTo("depr_gravitybomb", 1);
            }
        }


    }

    public virtual void DestroyWeapon() { }

    // returns weapon because it will be hard to get link in this object after untie
    // be careful: use base.UntieWeapon() in the end of override fuction or just return this;
    public virtual Weapon UntieWeapon() { return this; }

    // calls only by Enemies
    public virtual void Attack() { }

    // right click action processing
    public virtual void AlternateAttack() { }

    // what to do in hitting moment
    public virtual void Using() { }

    //for animation, calls on first state's first frame
    public virtual void StateAnimate(int state)
    {
        if (attackSprite != null)
        {
            if (state == 1)
                StartCoroutine(FadeIn(cooldown_list_dur[1]));
            if (state == 3)
                StartCoroutine(FadeOut(cooldown_list_dur[3]));
        }
    }

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
            {
                Using();
                ;
            }
                



            
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
        StateAnimate(state);
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

    // returns 0 - not hit, 1 - parry, 2 - block, 3 - open hit
    public int DamageAllInHitbox(bool isFromPlayer, int damage_)
    {
        int isHit = 0;
        Collider[] cols = Physics.OverlapBox(hitBox.bounds.center, hitBox.bounds.extents, hitBox.transform.rotation);
        foreach (Collider col in cols)
        {
            if (isFromPlayer)
            { 
                Enemy enemy = col.GetComponent<Enemy>();
                if (enemy != null)
                {
                    isHit = 3;
                    enemy.ProcessHp(-damage_);
                    Impulse(enemy, 1f);
                }
            }
            else
            {
                int dif = gameManager.battleSystem.game_difficulty;
                if (dif == 1)
                    damage = Mathf.FloorToInt(damage * 1.5f);
                else if (dif == 2)
                    damage = Mathf.FloorToInt(damage * 1.2f);
                else if (dif == 3)
                    damage = Mathf.FloorToInt(damage * 1f);
                else if (dif == 4)
                    damage = Mathf.FloorToInt(damage * 0.7f);

                Player player = col.GetComponent<Player>();
                if (player != null)
                {
                    if (player.stateMachine.IsActive("parrying"))
                    {
                        isHit = 1;
                        player.DamageImmuneFor(GlobalVariables.player_parry_damage_immune_duration);

                        gameManager.dataRecorder.AddTo("parry_times", 1);

                    }
                    else if (player.stateMachine.IsActive("blocking"))
                    {
                        isHit = 2;
                        player.ProcessHp(-Mathf.FloorToInt(damage_/2));
                        Impulse(player, 0.5f);
                        damaged = true;
                        gameManager.dataRecorder.AddTo("damage_blocked", Mathf.FloorToInt(damage_ / 2));
                    }
                    else
                    {
                        isHit = 3;
                        player.ProcessHp(-damage_);
                        Impulse(player, 1f);
                        damaged = true;
                    }
                    return isHit;
                }
            }

        }
        return isHit;
    }

    // Fading out of transparancy of weapon - both for player and enemies
    public IEnumerator FadeOut(float duration)
    {
        Color whiteColor = Color.white;
        attackSprite.GetComponent<MeshRenderer>().material.color = new Color(whiteColor.r, whiteColor.g, whiteColor.b, 1.0f);
        float curAlpha = 1.0f;
        float step = 1.0f / (duration / fadeStepSec);
        while (curAlpha > 0.0f)
        {
            curAlpha -= step;
            attackSprite.GetComponent<MeshRenderer>().material.color = new Color(whiteColor.r, whiteColor.g, whiteColor.b, curAlpha);
            yield return new WaitForSeconds(fadeStepSec);
        }
    }

    public IEnumerator FadeIn(float duration)
    {
        Color redColor = Color.red;
        attackSprite.GetComponent<MeshRenderer>().material.color = new Color(redColor.r, redColor.g, redColor.b, 0.0f);
        float curAlpha = 0.0f;
        float step = 1.0f / (duration / fadeStepSec);
        while (curAlpha < 1.0f)
        {
            curAlpha += step;
            attackSprite.GetComponent<MeshRenderer>().material.color = new Color(redColor.r, redColor.g, redColor.b, curAlpha);
            yield return new WaitForSeconds(fadeStepSec);
        }
    }
}
