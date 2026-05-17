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

    // Reference to the currently spawned instance (if any)
    private GameObject currentInstance;

    // Called by StageManager when this group's phase starts
    public void SpawnRandom()
    {
        ClearSpawn();

        if (prefabToSpawn == null)
        {
            Debug.LogWarning($"PhaseSpawnGroup[{name}]: no prefab assigned to spawn.");
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

        currentInstance = Instantiate(prefabToSpawn, point.position, point.rotation);
        if (parentSpawnToGroup && currentInstance != null)
        {
            currentInstance.transform.SetParent(this.transform, true);
        }

        Debug.Log($"PhaseSpawnGroup[{name}] (phase {phaseIndex}) spawned '{prefabToSpawn.name}' at '{point.name}' (index={idx}).");
    }

    // Destroy current spawned instance (if any)
    public void ClearSpawn()
    {
        if (currentInstance != null)
        {
            Destroy(currentInstance);
            currentInstance = null;
        }
    }
}
