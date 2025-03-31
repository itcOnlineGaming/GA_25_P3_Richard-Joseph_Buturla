using NUnit.Framework;
using System;
using UnityEngine;

public class DamageTestTarget : MonoBehaviour, ITargetable
{
    public static int MaxTowerLevels = 3;
    public static int TimeTestWindow = 3;
    float[] towerLevelDamages = new float[MaxTowerLevels];
    int currentTowerLevelIndex = 0;

    public Transform Transform => transform;

    public bool IsAlive => true;

    public Vector3 Position => transform.position;

    public void TakeDamage(float damage)
    {
        towerLevelDamages[currentTowerLevelIndex] += damage; 
        Debug.Log($"DamageTestTarget took {damage} damage. Total Damage: {towerLevelDamages[currentTowerLevelIndex]}");
    }

    public void SwitchToNextLevel()
    {
        currentTowerLevelIndex++;
    }

    public float GetTowerDamageFromLevelPerSecond(int levelIndex)
    {
        float damagePerSecond = towerLevelDamages[(int)levelIndex] / TimeTestWindow;
        return damagePerSecond;
    }
}
