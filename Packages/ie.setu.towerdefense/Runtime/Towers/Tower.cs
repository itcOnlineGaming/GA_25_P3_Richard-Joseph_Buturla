using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base Building Class
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

    public List<Enemy> Attackers = new List<Enemy>();

    public virtual void Update() { }

    public virtual void Initialise(TowerData data) { }

    public TowerData GetTowerData() { return towerData; }
    public void TakeDamage(int _amount) {
     
        currentHealth -= _amount;
        onTowerSelected?.Invoke(this);

        if (currentHealth <= 0)
        {
            Destroy(gameObject);
            SelectionRing.Instance.DeselectTower();
        }
    }

    public virtual void RepairTower(int _amount)
    {
        currentHealth += _amount;
        if(currentHealth >= towerData.MaxHealth)
        {
            currentHealth = towerData.MaxHealth;
        }
    }

    public int GetCurrentHealth() {  return currentHealth; }

}

