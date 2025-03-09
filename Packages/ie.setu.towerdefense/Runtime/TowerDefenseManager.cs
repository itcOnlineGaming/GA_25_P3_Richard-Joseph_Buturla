using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerDefenseManager : MonoBehaviour
{
    private static TowerDefenseManager instance;
    public static TowerDefenseManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<TowerDefenseManager>();
                if (instance == null)
                {
                    GameObject managerObject = new GameObject("TowerDefenseManager");
                    instance = managerObject.AddComponent<TowerDefenseManager>();
                }
            }
            return instance;
        }
    }

    // Registry of all targetable entities in the scene
    private List<ITargetable> allTargets = new List<ITargetable>();

    /// <summary>
    /// Register a targetable entity to the system
    /// </summary>
    public void RegisterTarget(ITargetable target)
    {
        if (!allTargets.Contains(target))
        {
            allTargets.Add(target);
        }
    }

    /// <summary>
    /// Unregister a targetable entity from the system
    /// </summary>
    public void UnregisterTarget(ITargetable target)
    {
        allTargets.Remove(target);
    }

    /// <summary>
    /// Get all registered targetable entities
    /// </summary>
    public List<ITargetable> GetAllTargets()
    {
        // Clean the list of null targets
        allTargets.RemoveAll(t => t == null);
        return allTargets;
    }
}