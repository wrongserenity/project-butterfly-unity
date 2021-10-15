using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creation : MonoBehaviour
{
    public GameManager gameManager;
    public DataRecorder dataRec;


    float Gr = GlobalVariables.GRAVITY;
    float LinearCoef = GlobalVariables.LINEAR_COEF;

    public string title = "";
    public Vector3 vel = new Vector3(0f, 0f, 0f);
    public int max_hp;
    public int cur_hp;
    public Weapon weapon; 
    float angular_acceleration = GlobalVariables.ANGULAR_ACC;
    public bool rotation_lock = false;
    public bool movement_lock = false;

    public Vector3 additional_force = new Vector3(0f, 0f, 0f);
    public float speed_vel;

    bool isImmortal = false;
    bool isDamageImmune = false;

    public CharacterController controller;

    public void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        dataRec = gameManager.dataRecorder;
    }
    public void ProcessHp(int value)
    {
        cur_hp += value;
        if (cur_hp <= 0)
        {
            if (gameObject.tag == "Player")
            {
                print("player killed");
                dataRec.PlayerKilled(false);
            }
            Kill();
        }
        else if (cur_hp > max_hp)
        {
            cur_hp = max_hp;
        }

        if (gameObject.tag == "Enemy" && cur_hp > 0)
            gameObject.GetComponent<Enemy>().CheckDeprivationStatus();

        if (gameObject.tag == "Player")
            gameObject.GetComponent<Player>().BarAnimation("health", "changed", 0f);
    }

    public void Kill()
    {
        gameManager.battleSystem.Kill(this);
    }

    public void DamageImmuneFor(float seconds)
    {
        StartCoroutine(DamageImmune(seconds));
    }

    public virtual void DamageImmuneAnimation(float duration) { }

    public IEnumerator DamageImmune(float duration)
    {
        isDamageImmune = true;
        DamageImmuneAnimation(duration);
        yield return new WaitForSeconds(duration);
        isDamageImmune = false;
    }

    public void FallingOutCheck(Vector3 position)
    {
        if (position.y < -5) {
            if (gameObject.tag == "Player")
            {
                dataRec.PlayerKilled(true);
            }
            Kill();
        }
    }

    void RotationSmooth(Vector3 direction)
    {
        if (!rotation_lock)
        {
            direction.y = 0;
            Vector3 dir_ = direction.normalized;
            if (Mathf.Abs(dir_.x) >= Time.deltaTime || Mathf.Abs(dir_.z) >= Time.deltaTime)
            {
                Quaternion rotTarget = Quaternion.LookRotation(Quaternion.AngleAxis(-90, Vector3.up) * dir_);
                transform.Find("Model").transform.rotation = Quaternion.RotateTowards(transform.Find("Model").transform.rotation, rotTarget, angular_acceleration * 20 * Time.deltaTime);
            }
        }
    }

    // but do we need to drag 'available_direction' through all the functions?
    public void MoveTo(Vector3 current_position, Vector3 goal_position, List<bool> available_directions)
    {
        Vector3 vector_to_goal = (goal_position - current_position).normalized;

        ProcessMovement(vector_to_goal, vector_to_goal, available_directions);
    }

    public void ProcessMovement(Vector3 movement_direction, Vector3 view_direction, List<bool> available_directions)
    {
        IncreaseVelocity(movement_direction, available_directions);

        if (gameObject.tag == "Player")
        {
            // self.rotation.y = atan2(-view_direction.x, -view_direction.z)
            // if self.state_machine.is_active('rewinding'):
            //      vel.y = 0
        }
        else if(gameObject.tag == "Enemy")
        {
            RotationSmooth(new Vector3(view_direction.x, transform.position.y, view_direction.z));
        }
        else
        {
            Debug.Log("not identified object movement call: add any tag");
        }
        controller.Move(vel * Time.deltaTime);
    }

    void IncreaseVelocity(Vector3 movement_direction, List<bool> available_directions)
    {
        if (movement_direction.magnitude > Time.deltaTime)
        {
            movement_direction = speed_vel * movement_direction.normalized;
        }
        vel += additional_force;
        additional_force = Mathf.Pow(LinearCoef, 0.3f) * additional_force;

        if (gameObject.tag == "Enemy")
        {
            movement_direction = ProcessDirectionalVector(movement_direction, available_directions);

        }

        vel.x += (movement_direction.x - vel.x) * LinearCoef;
        vel.y = Gr * Time.deltaTime;
        vel.z += (movement_direction.z - vel.z) * LinearCoef;
    }

    Vector3 ProcessDirectionalVector(Vector3 movement_directrion, List<bool> available_directions)
    {
        Vector3 dir_move = movement_directrion;

        if (!available_directions[0] && dir_move.z < 0)
        {
            dir_move.z = 0;
        }
        if (!available_directions[1] && dir_move.z > 0)
        {
            dir_move.z = 0;
        }
        if (!available_directions[2] && dir_move.x > 0)
        {
            dir_move.x = 0;
        }
        if (!available_directions[3] && dir_move.x < 0)
        {
            dir_move.x = 0;
        }

        if (this.GetComponent<Enemy>().currentLineNum != -1)
        {
            if ((gameManager.player.transform.position - this.transform.position).magnitude < gameManager.battleSystem.GetMaxDistance(this.GetComponent<Enemy>().currentLineNum))
            {
                dir_move = new Vector3(0f, 0f, 0f);
            }
        }

        if (movement_lock)
        {
            dir_move = new Vector3(0f, 0f, 0f);
        }

        return dir_move;
    }

    public void GetImpulse(Vector3 direction, float force)
    {
        additional_force += direction * force;

        if (gameObject.tag == "Player")
        {
            gameManager.mainCamera.Shake(force);
        }
    }
}
