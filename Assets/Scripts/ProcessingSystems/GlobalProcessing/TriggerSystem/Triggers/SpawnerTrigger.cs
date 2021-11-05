using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SpawnerTrigger : Trigger
{
    GameManager gameManager;
    Collider area;
    Collider spawnArea;

    GameObject enemyContainer;
    GameObject door;
    public float openDelay = 0f;
    public int enemyCount = 1;
    List<string> enemyPathList = new List<string>() { "Prefabs/Enemies/PushMachine", "Prefabs/Enemies/RoboSamurai", "Prefabs/Enemies/RoboSwordsman" };

    Image activatedButton;

    public bool isOpen = false;

    void Start()
    {
        isIterative = false;

        area = GetComponent<Collider>();
        spawnArea = transform.Find("SpawnArea").GetComponent<Collider>();
        door = transform.Find("Door").gameObject;
        activatedButton = transform.Find("Canvas").Find("Activated").GetComponent<Image>();
        enemyContainer = transform.Find("EnemyContainer").gameObject;

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        gameManager.triggerSystem.NewTriggerToLevel(this);
        transform.Find("Canvas").GetComponent<Canvas>().worldCamera = gameManager.mainCamera.gameObject.GetComponent<Camera>();
    }

    public override bool CheckCondition()
    {
        Collider[] cols = Physics.OverlapBox(area.bounds.center, area.bounds.extents, area.transform.rotation);
        foreach (Collider col in cols)
        {
            if (col.tag == "Player")
            {
                Player player = col.GetComponent<Player>();
                if (player.actionObj != this)
                {
                    if (!isOpen)
                    {
                        StartCoroutine(Open());
                        isOpen = true;
                    }
                    return true;
                }

            }
        }
        return base.CheckCondition();
    }

    IEnumerator Open()
    {
        activatedButton.enabled = true;
        activatedButton.fillAmount = 0f;
        float step = 0.01f;
        float curFill = 0f;
        float timeStep = openDelay / (1f / step);

        int totalSpawned = 0;

        Color sCol = door.GetComponent<MeshRenderer>().material.color;

        while (curFill < 1f)
        {
            if (curFill >= (float)totalSpawned / (float)enemyCount)
            {
                totalSpawned += 1;
                SpawnRandomEnemy();
            }
            curFill += step;
            activatedButton.fillAmount = curFill;
            door.GetComponent<MeshRenderer>().material.color = new Color(sCol.r, sCol.g, sCol.b, (1 - curFill * 0.5f));
            yield return new WaitForSeconds(timeStep);
        }
        door.SetActive(false);
        door.GetComponent<MeshRenderer>().material.color = sCol;
    }

    void SpawnRandomEnemy()
    {
        GameObject go = Resources.Load(enemyPathList[Random.Range(0, enemyPathList.Count - 1)]) as GameObject;
        Enemy enemy = Instantiate(go, GetRandomPointInside(), new Quaternion(0f, 0f, 0f, 1.0f)).GetComponent<Enemy>();
        enemy.transform.SetParent(enemyContainer.transform, true);
    }

    Vector3 GetRandomPointInside()
    {
        Vector3 center = spawnArea.bounds.center;
        Vector3 extents = spawnArea.bounds.extents;
        return new Vector3(Random.Range(center.x - extents.x, center.x + extents.x), Random.Range(center.y - extents.y, center.y + extents.y), Random.Range(center.z - extents.z, center.z + extents.z));
    }

    public override void ReloadTrigged()
    {
        door.SetActive(true);
        activatedButton.enabled = false;
        activatedButton.fillAmount = 0.0f;
        for (int i = enemyContainer.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(enemyContainer.transform.GetChild(i));
        }
        enemyContainer.transform.DetachChildren();
        gameManager.battleSystem.Reload();
        isOpen = false;
    }
}
