// TowerDefenseEvents.cs
using System;

public static class TowerDefenseEvents
{
    // Event for when a projectile is fired
    public delegate void ProjectileFiredHandler();
    public static event ProjectileFiredHandler OnProjectileFired;

    // Event for when a projectile hits a target
    public delegate void ProjectileHitHandler(int goldGain);
    public static event ProjectileHitHandler OnProjectileHit;

    public delegate void TowerDestroyedHandler(TowerData towerData);
    public static event TowerDestroyedHandler OnTowerDestroyed;

    // Trigger for OnProjectileFired event
    public static void RaiseProjectileFired()
    {
        OnProjectileFired?.Invoke();
    }

    // Trigger for OnProjectileHit event
    public static void RaiseProjectileHit(int goldGain)
    {
        OnProjectileHit?.Invoke(goldGain);
    }

    // Trigger for a 
    public static void RaiseTowerDestroyed(TowerData towerData)
    {
        OnTowerDestroyed?.Invoke(towerData);
    }
}
