using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class Spawner : MonoBehaviour
{

    [Range(0, 100)]
    public int chanceToSpawnCheck = 100;

    public GameObject[] spawnPool;

    [HideInInspector] public Vector3 position;

    private List<GameObject> spawnedObjects = new List<GameObject>();


    private void Start()
    {
        position = transform.position;
    }


    public void DoSpawn()
    {
        DoDespawn();

        if (spawnPool.Length <= 0) { return; }

        int diceRoll = Random.Range(1, 101);
        if (diceRoll <= chanceToSpawnCheck)
        {
            spawnedObjects.Add(Instantiate(spawnPool[Random.Range(0, spawnPool.Length)], transform.position + (Vector3.one * 0.5f), Quaternion.identity));
        }

    }

    public void DoDespawn()
    {
        
        foreach (GameObject o in spawnedObjects)
        {
            Destroy(o.gameObject);
        }
        spawnedObjects.Clear();
    }

    private void OnDestroy()
    {
        DoDespawn();
    }

}


[System.Serializable]
public class EntitySpawnerType
{
    [PreviewField(Alignment = ObjectFieldAlignment.Center)]
    public Sprite editorIcon;

    [Range(0, 100)]
    public int chanceToSpawnCheck = 100;

    public GameObject prefabSpawner;


}
