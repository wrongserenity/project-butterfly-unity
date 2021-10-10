using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlowParticles : MonoBehaviour
{
    int iterator = 1;
    Vector3 needPos;
    public Vector3 NextDestinationPosition(Vector3 playerPos)
    {
        Vector3 rotated = Quaternion.AngleAxis(-10, Vector3.up) * (transform.position - playerPos);
        needPos = playerPos + rotated * 0.95f;
        return needPos;
    }


    public Vector3 GetDirection(Vector3 playerPos)
    {
        if (iterator == 0)
        {
            NextDestinationPosition(playerPos);
            iterator = 1;
        }
        iterator--;
        return (needPos - transform.position).normalized;
    }

}
