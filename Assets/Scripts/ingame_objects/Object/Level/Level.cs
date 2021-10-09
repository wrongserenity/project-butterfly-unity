using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public GameManager gameManager;
    public Transform startTransform;
    // Start is called before the first frame update
    public void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame

    public virtual void FastReload() { }
}
