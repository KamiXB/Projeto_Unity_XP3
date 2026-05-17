using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class StageManager : MonoBehaviour
{
    [Tooltip("Total number of phases/stages in the run. Phases are 1-based indices.")]
    [SerializeField] private int totalPhases = 3;

    [Tooltip("If true, all PhaseSpawnGroup children will be automatically discovered on Awake. Otherwise you can register them manually.")]
    [SerializeField] private bool autoDiscoverGroups = true;

    private List<PhaseSpawnGroup> groups = new List<PhaseSpawnGroup>();

    private int currentPhase = 0;

    void Awake()
    {
        if (autoDiscoverGroups)
        {
            var found = GetComponentsInChildren<PhaseSpawnGroup>(true);
            groups.AddRange(found);
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

        foreach (var g in groups)
        {
            if (g == null) continue;
            if (g.phaseIndex == phaseIndex)
            {
                g.SpawnRandom();
            }
            else
            {
                // ensure groups for other phases are cleared
                g.ClearSpawn();
            }
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
