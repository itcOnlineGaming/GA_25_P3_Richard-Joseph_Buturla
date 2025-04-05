// TowerDefenseEvents.cs
using System;

public static class TowerDefenseEvents
{
    public delegate void TowerDestroyedHandler(Tower tower);
    public static event TowerDestroyedHandler OnTowerDestroyed;

    public delegate void TowerPlacedHandler(Tower tower);
    public static event TowerPlacedHandler OnTowerPlaced;

    public delegate void TowerUpgradedHandler(Tower tower);
    public static event TowerUpgradedHandler OnTowerUpgraded;

    public delegate void TowerRepairedHandler(Tower tower);
    public static event TowerRepairedHandler OnTowerRepaired;

    public static void RaiseTowerDestroyed(Tower tower)
    {
        OnTowerDestroyed?.Invoke(tower);
    }

    public static void RaiseTowerPlaced(Tower tower)
    {
        OnTowerPlaced?.Invoke(tower);
    }

    public static void RaiseTowerUpgraded(Tower tower)
    {
        OnTowerUpgraded?.Invoke(tower);
    }

    public static void RaiseTowerRepaired(Tower tower)
    {
        OnTowerRepaired?.Invoke(tower);
    }
}
