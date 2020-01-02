using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class Spawner : MonoBehaviour
{

    [Range(0, 100)]
    public int chanceToSpawnCheck = 100;
    public bool spawnOnGround = false;
    public bool accomodateForVertexWobble = false;
    public Vector3 randomPositionRange = Vector3.zero;
    public Vector3 randomRotationRange = Vector3.zero;

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
            Vector3 pos = transform.position + (Vector3.one * 0.5f);
            if (spawnOnGround) pos += Vector3.down * 0.5f + (Vector3.up * 0.02f);
            pos += new Vector3(Random.Range(-randomPositionRange.x / 2f, randomPositionRange.x / 2f), Random.Range(-randomPositionRange.y / 2f, randomPositionRange.y / 2f), Random.Range(-randomPositionRange.z / 2f, randomPositionRange.z / 2f));
            if (accomodateForVertexWobble) pos += Vector3.up * 0.02f;

            Quaternion rot = Quaternion.Euler(Random.Range(-randomRotationRange.x / 2f, randomRotationRange.x / 2f), Random.Range(-randomRotationRange.y / 2f, randomRotationRange.y / 2f), Random.Range(-randomRotationRange.z / 2f, randomRotationRange.z / 2f));

            spawnedObjects.Add(Instantiate(spawnPool[Random.Range(0, spawnPool.Length)], pos, rot));
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

    public bool isLight = false;


}
