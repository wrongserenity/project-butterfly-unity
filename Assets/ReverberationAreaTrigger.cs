using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReverberationAreaTrigger : Trigger
{
    GameManager gameManager;
    Collider area;

    public float reverberationValue = StateMachine.REVERBERATION_DEFAULT_VALUE;

    List<Collider> affectedObjects = new List<Collider>();

    private void Start()
    {
        isIterative = false;

        area = GetComponent<Collider>();

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        gameManager.triggerSystem.NewTriggerToLevel(this);
    }

    public override bool CheckCondition()
    {
        Collider[] cols = Physics.OverlapBox(area.bounds.center, area.bounds.extents, area.transform.rotation);
        foreach (Collider col in cols)
        {
            if (!affectedObjects.Contains(col) && col.CompareTag("Player"))
            {
                col.GetComponent<Player>().stateMachine.SetEnvironmentConditionValue(StateMachine.REVERBERATION_PARAMETER_NAME, reverberationValue);
                affectedObjects.Add(col);
            }
        }

        foreach (Collider obj in affectedObjects)
        {
            if (!new List<Collider>(cols).Contains(obj))
                obj.GetComponent<Player>().stateMachine.SetEnvironmentConditionValue(StateMachine.REVERBERATION_PARAMETER_NAME);
        }
        return base.CheckCondition();
    }
}
