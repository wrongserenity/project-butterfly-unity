
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line
{
    public int max_power;
    public int current_power;
    public double radius;
    public List<char> allowed_enemies;

    public List<Enemy> enemies = new List<Enemy>() { };

    public Line(List<char> allowed_enemies_, int power_, double radius_)
    {
     allowed_enemies = allowed_enemies_;
     max_power = power_;
     radius = radius_;
     current_power = 0;
    }

    public bool CanAdd(Enemy target)
    {
        if (allowed_enemies.Contains(target.type) && current_power + target.power <= max_power && !enemies.Contains(target))
        {
            return true;
        }
        return false;
    }

    public void Add(Enemy target)
    {   
        current_power += target.power;
        enemies.Add(target);
    }

    public bool Remove(Enemy target)
    {
        current_power -= target.power;
        target.currentLineNum = -1;
        return enemies.Remove(target);
    } 
}

public class BattleSystem : MonoBehaviour
{
    GameManager gameManager;
    DataRecorder dataRec;

    public List<Line> lines;
    public List<List<string>> allowed_enemies;

    private const int lines_num = 6;
    public int game_difficulty = GlobalVariables.game_difficult;
    double default_line_radius = GlobalVariables.line_radius;
    
    public List<int> allowed_power = new List<int> {5, 15, 6, 9, 5, 100};
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        dataRec = gameManager.dataRecorder;

        lines = new List<Line>();

        RefreshVariables();
    }

    public void RefreshVariables()
    {
        lines.Clear();
        if (game_difficulty <= 2)
        {
            lines.Add(new Line(new List<char> { 'e' }, 5, default_line_radius));
            lines.Add(new Line(new List<char> { 'm', 'r' }, 10, default_line_radius * 2));
            lines.Add(new Line(new List<char> { 'm', 'r', 'e' }, 6, default_line_radius * 3));
            lines.Add(new Line(new List<char> { 'r', 'e' }, 9, default_line_radius * 4));
            lines.Add(new Line(new List<char> { 's' }, 5, default_line_radius * 5));
            lines.Add(new Line(new List<char> { 'm', 'r', 's', 'e' }, 100, default_line_radius * 6));
        }

        if (game_difficulty == 3)
        {
            lines.Add(new Line(new List<char> { 'e' }, 10, default_line_radius));
            lines.Add(new Line(new List<char> { 'm', 'r' }, 15, default_line_radius * 2));
            lines.Add(new Line(new List<char> { 'm', 'r', 'e' }, 6, default_line_radius * 3));
            lines.Add(new Line(new List<char> { 'r', 'e' }, 9, default_line_radius * 4));
            lines.Add(new Line(new List<char> { 's' }, 5, default_line_radius * 5));
            lines.Add(new Line(new List<char> { 'm', 'r', 's', 'e' }, 100, default_line_radius * 6));
        }
        
        if ( game_difficulty >= 4)
        {
            lines.Add(new Line(new List<char> { 'e' }, 15, default_line_radius));
            lines.Add(new Line(new List<char> { 'm', 'r' }, 20, default_line_radius * 2));
            lines.Add(new Line(new List<char> { 'm', 'r', 'e' }, 10, default_line_radius * 3));
            lines.Add(new Line(new List<char> { 'r', 'e' }, 12, default_line_radius * 4));
            lines.Add(new Line(new List<char> { 's' }, 15, default_line_radius * 5));
            lines.Add(new Line(new List<char> { 'm', 'r', 's', 'e' }, 100, default_line_radius * 6));
        }
    }

    public int GetAvailableLineNum(Enemy target)
    {
        for (int i = 0; i < lines_num; i++)
        {
            if (lines[i].CanAdd(target)){
                return i;
            }
        }
        return -1;
    }

    public bool AddTo(Enemy target, int line_num)
    {
        bool removal_successful = false;
        if (target.currentLineNum != -1){
            removal_successful = lines[target.currentLineNum].Remove(target);    
        } else {
            removal_successful = true;
        }
        if (removal_successful && line_num >= 0 && line_num < lines_num) {
            lines[line_num].Add(target);
            target.currentLineNum = line_num;
            return true;
        } else {
            return false;
        }
    }

    public void AddToBattle(Enemy target)
    {
        if (CalculateEnemiesCount() == 0)
            target.playerNotice.Play();

        int available_line = GetAvailableLineNum(target);
        bool result = AddTo(target, available_line);
        if (!result)
        {
            Debug.Log("Could not add object to line" + available_line + ". Target: " + target);
        }
        else
        {
            Debug.Log("successfully added to: " + available_line);
        }

    }

    public float GetMaxDistance(int lineNumber)
    {
        return (float)lines[lineNumber].radius;
    }

    /// general method for all battle members
    public void Kill(Creation object_)
    {
        if (object_.tag == "Player")
        {
            gameManager.ReloadToCheckPoint();
        }

        if (object_.tag == "Enemy")
        {
            Enemy enemy_ = (Enemy)object_;
            if (enemy_.title == "pushmachine")
                dataRec.AddTo("push_killed", 1);
            else if (enemy_.title == "robosamurai")
                dataRec.AddTo("samu_killed", 1);
            else if (enemy_.title == "roboswordsman")
                dataRec.AddTo("sword_killed", 1);
            else
                Debug.Log("ERROR: unknown enemy title: " + enemy_.title);

            RemoveEnemy(enemy_);
            enemy_.EnemyTurnOff();
            //gameManager.player.EnergyTransfer(enemy_.power * GlobalVariables.enemy_power_price);
            gameManager.AddEnemyToReload(enemy_);
        }

    }

    public void RemoveEnemy(Enemy enemy)
    {
        if (enemy.currentLineNum != -1)
        {
            lines[enemy.currentLineNum].Remove(enemy);
            enemy.currentLineNum = -1;
        }
        else
        {
            Debug.Log("ERROR: in RemoveEnemy: enemy has no line");
        }
    }

    bool FindProfitSwap(Enemy enemy, string param)
    {
        if (enemy.currentLineNum == lines_num - 1)
        {
            return false;
        }
        if (new List<char>() { 'm', 'e' }.Contains(enemy.type))
        {
            for (int j = (enemy.currentLineNum + 1); j < lines_num; j++)
            {
                foreach(Enemy enemy_target in lines[j].enemies)
                {
                    if (enemy.type == enemy_target.type)
                    {
                        if (IsProfitToSwap(enemy, enemy_target, param))
                        {
                            SwapEnemies(enemy, enemy_target);
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    bool IsProfitToSwap(Enemy first_enemy, Enemy second_enemy, string param)
    {
        if (param == "health")
        {
            if (first_enemy.cur_hp < second_enemy.cur_hp)
            {
                return true;
            }
        }else if(param == "power")
        {
            if (first_enemy.power < second_enemy.power)
            {
                return true;
            }
        }else if (param == "distance")
        {
            if (first_enemy.GetDistanceToPlayer() > second_enemy.GetDistanceToPlayer())
            {
                return true;
            }
        }
        else
        {
            Debug.Log("ERROR: unknown parameter for battle optimization");
        }
        return false;
    }

    void SwapEnemies(Enemy first, Enemy second)
    {
        List<int> swapLines = new List<int>() { first.currentLineNum, second.currentLineNum };
        RemoveEnemy(first);
        RemoveEnemy(second);
        AddTo(first, swapLines[1]);
        AddTo(second, swapLines[0]);
    }

    /// field optimizator
    void FixedUpdate()
    {
        if (game_difficulty >= 3)
        {
            for (int i = 0; i < lines_num; i++)
            {
                foreach (Enemy enemy in lines[i].enemies.ToArray())
                {
                    for (int j = 0; j < i; j++)
                    {
                        if (lines[j].CanAdd(enemy))
                        {
                            AddTo(enemy, j);
                        }
                        if (i == 0 || i == 1)
                        {
                            FindProfitSwap(enemy, "distance");
                        }
                        if (game_difficulty >= 4)
                        {
                            if (enemy.cur_hp < enemy.max_hp)
                            {
                                FindProfitSwap(enemy, "health");
                            }
                        }
                        
                    }
                }
            }
        }
    }

    public int CalculateEnemiesCount()
    {
        int sum = 0;
        foreach(Line line in lines)
        {
            sum += line.enemies.Count;
        }
        return sum;
    }

    public void Reload()
    {
        foreach (Line line in lines)
        {
            line.enemies.Clear();
            line.current_power = 0;
        }
        RefreshVariables();
    }
}