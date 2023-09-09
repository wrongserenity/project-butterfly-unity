using System;
using System.Collections.Generic;

[Serializable]
public struct PythonExchangeData 
{
    public float inputDirMoveX;
    public float inputDirMoveZ;
    public float inputDirViewX;
    public float inputDirViewz;
    public float curHP;
    public float curEnergy;
    public float curXiton;
    public float curEnemiesCountClose;
    public float curEnemiesCountDistant;
    public float curBattleMusicParameterValue;
    public float timeFromLastattack;
    public float timeFromLastdamaged;
    public float timeFromLastkill;
    public float timeFromLastdeath;
    public float timeFromLastenergySpending;
    public float timeFromLastenergyCollecting;
    public float timeFromLastxitonSpending;
    public float timeFromLastxitonCharging;
    public float timeFromLastshifted;
    public float timeFromLastrewinded;
    public float timeFromLastdepricationWeapon;

    public float angry;
    public float disgust;
    public float fear;
    public float happy;
    public float neutral;
    public float sad;
    public float surprise;
}


