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

    [Header("Spawn Options")]
    [Tooltip("When true, spawned key will use the Forced Scale below instead of the prefab's scale.")]
    public bool forceScale = false;
    [Tooltip("Scale to apply to the spawned key when Force Scale is enabled.")]
    public Vector3 forcedScale = Vector3.one;

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

        // Instantiate without parent to avoid inheriting any transform scale from spawn.
        currentKeyInstance = Instantiate(keyPrefab, spawn.position, spawn.rotation);

        // Apply scale: either forced (from Inspector) or preserve prefab's local scale
        if (currentKeyInstance != null)
        {
            if (forceScale)
                currentKeyInstance.transform.localScale = forcedScale;
            else if (keyPrefab != null)
                currentKeyInstance.transform.localScale = keyPrefab.transform.localScale;
        }
    }

    // Optional: call to respawn key (e.g., after some event)
    public void RespawnKey()
    {
        SpawnKey();
    }
}
