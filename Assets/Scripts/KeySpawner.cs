using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeySpawner : MonoBehaviour
{
    [Tooltip("Key prefab to spawn")]
    public GameObject keyPrefab;

    [Tooltip("Possible spawn points (Transforms) - pick one at Start")]
    public Transform[] spawnPoints;

    [Tooltip("If true, choose a random spawn point at Start. Else first spawn point is used.")]
    public bool randomizeAtStart = true;

    private GameObject currentKeyInstance;

    void Start()
    {
        SpawnKey();
    }

    public void SpawnKey()
    {
        if (keyPrefab == null || spawnPoints == null || spawnPoints.Length == 0) return;

        int idx = 0;
        if (randomizeAtStart)
            idx = Random.Range(0, spawnPoints.Length);

        var spawn = spawnPoints[idx];
        if (spawn == null) return;

        if (currentKeyInstance != null) Destroy(currentKeyInstance);

        currentKeyInstance = Instantiate(keyPrefab, spawn.position, Quaternion.identity);
    }

    // Optional: call to respawn key (e.g., after some event)
    public void RespawnKey()
    {
        SpawnKey();
    }
}
