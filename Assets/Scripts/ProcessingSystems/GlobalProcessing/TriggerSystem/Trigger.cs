using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger : MonoBehaviour
{
    public bool isIterative;

    public virtual bool CheckCondition() { return false; }

    public virtual void Activate() { }
}
