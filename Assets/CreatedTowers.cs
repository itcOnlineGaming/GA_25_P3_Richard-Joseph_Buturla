using UnityEngine;

using TowerLevel = TowerData.TowerLevel;

public class CreatedTowers : MonoBehaviour
{
    private static CreatedTowers instance;
    public static CreatedTowers Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<CreatedTowers>();
                if (instance == null)
                {
                    GameObject managerObject = new GameObject("CreatedTowers");
                    instance = managerObject.AddComponent<CreatedTowers>();
                }
            }
            return instance;
        }
    }

    public void InitialiseTowers()
    {
        // Cannon Tower - All-around balanced tower
        TowerFactory.CreateTowerData("Cannon", TowerLevel.LevelOne, 800, 500, 1000, 350, 1.2f, 7f, 7f, null);
        TowerFactory.CreateTowerData("Cannon", TowerLevel.LevelTwo, 1200, 750, 1500, 525, 1.0f, 15f, 8f, null);
        TowerFactory.CreateTowerData("Cannon", TowerLevel.LevelThree, 1800, 1125, 2250, 800, 0.8f, 30f, 10f, null);

        // Mortar Tower (Slow but powerful AoE)
        TowerFactory.CreateTowerData("Mortar", TowerLevel.LevelOne, 450, 1250, 1300, 500, 3.5f, 15f, 11f, null);
        TowerFactory.CreateTowerData("Mortar", TowerLevel.LevelTwo, 675, 975, 1950, 750, 3.0f, 32f, 12f, null);
        TowerFactory.CreateTowerData("Mortar", TowerLevel.LevelThree, 1000, 1450, 2900, 1125, 2.5f, 65f, 14f, null);

        // Sniper Tower (Long range, high damage)
        TowerFactory.CreateTowerData("Sniper", TowerLevel.LevelOne, 700, 550, 1400, 525, 2.5f, 20f, 12f, null);
        TowerFactory.CreateTowerData("Sniper", TowerLevel.LevelTwo, 1050, 650, 2100, 800, 2.0f, 30f, 14f, null);
        TowerFactory.CreateTowerData("Sniper", TowerLevel.LevelThree, 1575, 1150, 3150, 1200, 1.5f, 50f, 16f, null);

        // AeroPlane Tower (Fast attacks, mobile)
        TowerFactory.CreateTowerData("AirplaneTower", TowerLevel.LevelOne, 550, 900, 1600, 600, 0.4f, 3f, 12f, null);
        TowerFactory.CreateTowerData("AirplaneTower", TowerLevel.LevelTwo, 825, 1200, 2400, 900, 0.3f, 6f, 13f, null);
        TowerFactory.CreateTowerData("AirplaneTower", TowerLevel.LevelThree, 1225, 1800, 3600, 1350, 0.2f, 8f, 14f, null);
    }

}
