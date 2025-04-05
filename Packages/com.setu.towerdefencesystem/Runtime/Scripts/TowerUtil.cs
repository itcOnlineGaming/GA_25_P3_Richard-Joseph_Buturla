using log4net.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class TowerUtils : MonoBehaviour
{

    /// <summary>
    /// Spawns a tower at the specified position
    /// </summary>
    public static Tower SpawnTower(Vector3 position, Quaternion rotation, TowerData data, GameObject buildingPrefab)
    {
        GameObject building = Instantiate(buildingPrefab, position, rotation);
        Tower tower = building.GetComponent<Tower>();
        tower.placed = true;
        tower.Initialise(data);
        TowerDefenseEvents.RaiseTowerPlaced(tower);
        return tower;
    }
    public static void UpgradeBuilding(Tower placedTower)
    {
        TowerData towerData = placedTower.GetTowerData();
        if (towerData.Level != TowerData.TowerLevel.LevelThree)
        {
            TowerData.TowerLevel newLevel = TowerData.TowerLevelUtils.IncrementTowerLevel(towerData.Level);
            TowerData upgradedTowerData = TowerFactory.GetTowerData(towerData.TowerType, newLevel);
            int healthBeforeUpgrade = placedTower.GetCurrentHealth();
            placedTower.Initialise(upgradedTowerData); // Set new tower data
            placedTower.SetCurrentHealth(healthBeforeUpgrade);
            TowerDefenseEvents.RaiseTowerUpgraded(placedTower);
        }
    }

    public static void RepairBuilding(Tower tower, int healthToRepair)
    {
        tower.RepairTower(healthToRepair);
        TowerDefenseEvents.RaiseTowerRepaired(tower);
    }
}
