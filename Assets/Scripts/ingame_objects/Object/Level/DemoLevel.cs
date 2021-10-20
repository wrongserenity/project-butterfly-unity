using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoLevel : Level
{
    void Start()
    {
        base.Start();
        startTransform = transform.Find("SpawnPosition");
        gameManager.currentCheckpoint = startTransform.position;
    }
    public override void FastReload()
    {
        base.FastReload();
        foreach (Enemy enemy in transform.Find("Enemies").GetComponentsInChildren<Enemy>())
            enemy.deathRequest = true;
        gameManager.player.stateMachine.AddState("death");
    }

    public override void LoadCheckPoint(List<Enemy> enemies)
    {
        foreach (Enemy enemy in enemies)
            enemy.deathRequest = true;
        gameManager.player.stateMachine.AddState("checkPointLoad");
    }
}
