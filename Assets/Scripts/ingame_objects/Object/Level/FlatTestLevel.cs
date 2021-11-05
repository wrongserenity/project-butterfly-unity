using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlatTestLevel : Level
{
    void Start()
    {
        base.Start();
        startTransform = transform.Find("SpawnPosition");
    }
    public override void FastReload()
    {
        base.FastReload();
        foreach (Enemy enemy in transform.Find("Enemies").GetComponentsInChildren<Enemy>())
            enemy.deathRequest = true;
        gameManager.player.stateMachine.AddState("death");
    }
}
