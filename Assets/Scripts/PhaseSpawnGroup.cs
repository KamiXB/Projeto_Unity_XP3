using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PhaseSpawnGroup : MonoBehaviour
{
    [Tooltip("Which phase index this spawn group belongs to (1-based). When the StageManager starts the phase, this group will spawn one prefab at a random chosen point.")]
    public int phaseIndex = 1;

    [Tooltip("Prefab to spawn at one of the spawn points.")]
    public GameObject prefabToSpawn;

    [Tooltip("List of possible spawn points (Transforms). One of these will be chosen at random when the phase starts.")]
    public List<Transform> spawnPoints = new List<Transform>();

    [Tooltip("If true the spawned object will be parented under this PhaseSpawnGroup in the hierarchy.")]
    public bool parentSpawnToGroup = true;

    [Header("Light Spot")]
    [Tooltip("If assigned, this Light Spot GameObject will be moved to the chosen spawn point when a prefab is spawned.")]
    public GameObject lightSpotToMove;

    [Tooltip("If true, the light spot will be parented to the spawned instance after being moved.")]
    public bool parentLightToSpawn = false;

    // Reference to the currently spawned instance (if any)
    private GameObject currentInstance;

    // Called by StageManager when this group's phase starts
    public void SpawnRandom()
    {
        SpawnWithPrefab(prefabToSpawn);
    }

    // Spawn the provided prefab at a random point from spawnPoints
    public void SpawnWithPrefab(GameObject prefab)
    {
        ClearSpawn();

        if (prefab == null)
        {
            Debug.LogWarning($"PhaseSpawnGroup[{name}]: SpawnWithPrefab called with null prefab.");
            return;
        }

        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogWarning($"PhaseSpawnGroup[{name}]: no spawn points assigned.");
            return;
        }

        int idx = Random.Range(0, spawnPoints.Count);
        var point = spawnPoints[idx];
        if (point == null)
        {
            Debug.LogWarning($"PhaseSpawnGroup[{name}]: chosen spawn point was null (index={idx}).");
            return;
        }

        currentInstance = Instantiate(prefab, point.position, point.rotation);
        if (parentSpawnToGroup && currentInstance != null)
        {
            currentInstance.transform.SetParent(this.transform, true);
        }

        // Move the Light Spot (if assigned) to the spawn point and optionally parent it
        if (lightSpotToMove != null)
        {
            lightSpotToMove.transform.position = point.position;
            lightSpotToMove.transform.rotation = point.rotation;
            lightSpotToMove.SetActive(true);

            if (parentLightToSpawn && currentInstance != null)
            {
                // parent but keep world position
                lightSpotToMove.transform.SetParent(currentInstance.transform, true);
            }
            else
            {
                // ensure it's not parented to something that would offset it
                lightSpotToMove.transform.SetParent(null, true);
            }
        }

        Debug.Log($"PhaseSpawnGroup[{name}] (phase {phaseIndex}) spawned '{prefab.name}' at '{point.name}' (index={idx}).");
    }

    // Destroy current spawned instance (if any)
    public void ClearSpawn()
    {
        if (currentInstance != null)
        {
            // if the light was parented to the spawned instance, unparent it to avoid it being destroyed
            if (lightSpotToMove != null && parentLightToSpawn)
            {
                lightSpotToMove.transform.SetParent(this.transform, true);
            }

            Destroy(currentInstance);
            currentInstance = null;
        }
    }
}
