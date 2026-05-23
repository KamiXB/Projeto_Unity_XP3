using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class StageManager : MonoBehaviour
{
    [Tooltip("Total number of phases/stages in the run. Phases are 1-based indices.")]
    [SerializeField] private int totalPhases = 3;

    [Tooltip("If true, all PhaseSpawnGroup children will be automatically discovered on Awake. Otherwise you can register them manually.")]
    [SerializeField] private bool autoDiscoverGroups = true;

    [Header("Auto Start (debug)")]
    [Tooltip("If true the StageManager will automatically start a phase on Awake (useful for testing).")]
    [SerializeField] private bool autoStartPhaseOnAwake = false;
    [Tooltip("Phase index to auto-start when autoStartPhaseOnAwake is enabled.")]
    [SerializeField] private int autoStartPhaseIndex = 1;

    [System.Serializable]
    public class PhasePool
    {
        [Tooltip("Which phase this pool applies to (1-based)")]
        public int phaseIndex = 1;

        [Tooltip("List of prefabs that can be chosen for this phase. Each chosen prefab will be unique (no repeats) while pool has enough items.")]
        public List<GameObject> prefabs = new List<GameObject>();
    }

    [Tooltip("Optional per-phase pools of prefabs to be distributed uniquely among groups for that phase.")]
    [SerializeField] private List<PhasePool> phasePools = new List<PhasePool>();
    [Header("Single pickup mode")]
    [Tooltip("If true, exactly one prefab from the globalPowerupPool will be spawned somewhere among groups for the started phase.")]
    [SerializeField] private bool singlePickupPerPhase = false;

    [Tooltip("When singlePickupPerPhase is true this list defines the candidate prefabs (powerups) that may be chosen — only one will actually spawn.")]
    [SerializeField] private List<GameObject> globalPowerupPool = new List<GameObject>();

    private List<PhaseSpawnGroup> groups = new List<PhaseSpawnGroup>();

    private int currentPhase = 0;

    void Awake()
    {
        if (autoDiscoverGroups)
        {
            var found = GetComponentsInChildren<PhaseSpawnGroup>(true);
            groups.AddRange(found);
        }

        // Debug: report discovered groups
        Debug.Log($"StageManager Awake: discovered {groups.Count} PhaseSpawnGroup(s).");
        for (int i = 0; i < groups.Count; i++)
        {
            var g = groups[i];
            if (g == null) continue;
            Debug.Log($"  Group[{i}]: name='{g.name}', phaseIndex={g.phaseIndex}, spawnPoints={g.spawnPoints?.Count}");
        }

        if (autoStartPhaseOnAwake)
        {
            Debug.Log($"StageManager: auto-starting phase {autoStartPhaseIndex} on Awake.");
            StartPhase(autoStartPhaseIndex);
        }
    }

    // Start a specific phase (1-based). Will trigger all groups matching that phaseIndex to spawn one random instance.
    public void StartPhase(int phaseIndex)
    {
        if (phaseIndex < 1 || phaseIndex > totalPhases)
        {
            Debug.LogWarning($"StageManager: invalid phaseIndex {phaseIndex} (totalPhases={totalPhases}).");
            return;
        }

        currentPhase = phaseIndex;
        Debug.Log($"StageManager: starting phase {phaseIndex}.");

        // Collect groups that belong to this phase
        var groupsForPhase = new List<PhaseSpawnGroup>();
        foreach (var g in groups)
        {
            if (g == null) continue;
            if (g.phaseIndex == phaseIndex) groupsForPhase.Add(g);
            else g.ClearSpawn();
        }

        // Find a pool for this phase (if any)
        PhasePool pool = phasePools.Find(p => p.phaseIndex == phaseIndex);

        // If singlePickupPerPhase is enabled, choose exactly one prefab from the globalPowerupPool
        if (singlePickupPerPhase)
        {
            if (globalPowerupPool != null && globalPowerupPool.Count > 0)
            {
                if (groupsForPhase.Count == 0)
                {
                    Debug.LogWarning($"StageManager: singlePickupPerPhase enabled but no PhaseSpawnGroup found for phase {phaseIndex}.");
                    return;
                }

                // choose a random prefab from the global pool
                int prefabIdx = Random.Range(0, globalPowerupPool.Count);
                var chosenPrefab = globalPowerupPool[prefabIdx];
                if (chosenPrefab == null)
                {
                    Debug.LogWarning("StageManager: chosen prefab from globalPowerupPool is null.");
                    return;
                }

                // choose a random group among the available groups for this phase
                int groupIdx = Random.Range(0, groupsForPhase.Count);
                var chosenGroup = groupsForPhase[groupIdx];

                // If the chosen entry is actually a PhaseSpawnGroup scene object (you may have dragged the group by mistake),
                // use its configured prefabToSpawn instead.
                var prefabToInstantiate = chosenPrefab;
                var pg = chosenPrefab != null ? chosenPrefab.GetComponent<PhaseSpawnGroup>() : null;
                if (pg != null)
                {
                    if (pg.prefabToSpawn == null)
                    {
                        Debug.LogWarning($"StageManager: chosen PhaseSpawnGroup '{pg.name}' has no prefabToSpawn configured. Aborting spawn.");
                        return;
                    }
                    prefabToInstantiate = pg.prefabToSpawn;
                    Debug.LogWarning($"StageManager: globalPowerupPool entry '{chosenPrefab.name}' is a PhaseSpawnGroup scene object — using its prefabToSpawn '{prefabToInstantiate.name}' instead. For clarity, assign prefab assets to the global pool.");
                }

                if (prefabToInstantiate == null)
                {
                    Debug.LogWarning($"StageManager: chosen prefab is null, cannot spawn.");
                    return;
                }

                // spawn chosen prefab in the selected group
                chosenGroup.SpawnWithPrefab(prefabToInstantiate);
                Debug.Log($"StageManager: single pickup mode - spawned '{prefabToInstantiate.name}' at group '{chosenGroup.name}' for phase {phaseIndex}.");

                // clear spawns for all other groups for this phase
                for (int i = 0; i < groupsForPhase.Count; i++)
                {
                    if (i == groupIdx) continue;
                    groupsForPhase[i].ClearSpawn();
                }

                return;
            }
            else
            {
                Debug.LogWarning("StageManager: singlePickupPerPhase is enabled but globalPowerupPool is empty. Falling back to normal pool logic.");
            }
        }

        if (pool == null || pool.prefabs == null || pool.prefabs.Count == 0)
        {
            // No pool: let each group spawn its own configured prefab
            foreach (var g in groupsForPhase)
            {
                g.SpawnRandom();
            }
            return;
        }

        // We have a pool: assign unique prefabs to groups randomly without repetition
        var available = new List<GameObject>(pool.prefabs);

        // Shuffle available list
        for (int i = 0; i < available.Count; i++)
        {
            int j = Random.Range(i, available.Count);
            var tmp = available[i];
            available[i] = available[j];
            available[j] = tmp;
        }

        int assignCount = Mathf.Min(groupsForPhase.Count, available.Count);

        // Assign one unique prefab per group up to assignCount
        for (int i = 0; i < assignCount; i++)
        {
            var g = groupsForPhase[i];
            var prefab = available[i];
            g.SpawnWithPrefab(prefab);
        }

        // Clear any remaining groups if there are fewer prefabs than groups
        for (int i = assignCount; i < groupsForPhase.Count; i++)
        {
            groupsForPhase[i].ClearSpawn();
        }
    }

    // Advance to next phase (if any)
    public void NextPhase()
    {
        int next = Mathf.Min(totalPhases, currentPhase + 1);
        StartPhase(next);
    }

    // Reset all groups
    public void ResetAll()
    {
        foreach (var g in groups)
        {
            g.ClearSpawn();
        }
        currentPhase = 0;
    }
}
