using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Tower : MonoBehaviour
{
    public float range = 5f;
    public Transform firePoint;
    protected TowerData towerData;
    public bool placed = false;
    public bool targetable = true;
    public int towerID = -1;

    protected int currentHealth;

    public static event Action<Tower> onTowerSelected;

    public List<ITargetable> Attackers = new List<ITargetable>();

    public virtual void Update() { }

    public virtual void Initialise(TowerData data)
    {
        towerData = data;
        currentHealth = data.MaxHealth;
    }

    public TowerData GetTowerData() { return towerData; }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        onTowerSelected?.Invoke(this);

        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    public virtual void RepairTower(int amount)
    {
        currentHealth += amount;
        if (currentHealth >= towerData.MaxHealth)
        {
            currentHealth = towerData.MaxHealth;
        }
    }

    public int GetCurrentHealth() { return currentHealth; }

    public void SetCurrentHealth(int newHealth) { currentHealth = newHealth; }

    /// <summary>
    /// Finds the nearest targetable entity within range
    /// </summary>
    protected ITargetable FindNearestTarget()
    {
        // Find all objects implementing ITargetable in the scene
        // This assumes you're using a service locator or similar pattern
        List<ITargetable> targets = TowerDefenseManager.Instance.GetAllTargets();

        float shortestDistance = Mathf.Infinity;
        ITargetable nearestTarget = null;

        foreach (ITargetable target in targets)
        {
            if (target == null || !target.IsAlive)
                continue;

            float distance = Vector3.Distance(transform.position, target.Position);

            if (distance < shortestDistance && distance <= range)
            {
                shortestDistance = distance;
                nearestTarget = target;
            }
        }

        return nearestTarget;
    }
}