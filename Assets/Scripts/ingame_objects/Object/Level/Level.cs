using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public GameManager gameManager;
    public Transform startTransform;

    public MusicSettings musicSettings;
    // Start is called before the first frame update
    public void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        gameManager.musicSystem.UpdateMusicSettings(musicSettings);
        gameManager.musicSystem.StartAllMusic();
    }

    public virtual void FastReload() { }

    public virtual void LoadCheckPoint(List<Enemy> enemies) { }
}
