using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TowerLevel = TowerData.TowerLevel;

public class TowerPlacer : MonoBehaviour
{
    public GameObject buildingPrefab;
    public GameObject placeBuildingEffect;

    // Tower sprites
    public Sprite CannonTowerSprite;
    public Sprite FactoryTowerSprite;
    public Sprite MortarTowerSprite;
    public Sprite SniperTowerSprite;
    public Sprite AeroPlaneSprite;

    private void Awake()
    {
        // Register tower data
        RegisterTowerData();
    }

    private void Start()
    {
        // Load place building effect if not already set
        if (placeBuildingEffect == null)
        {
            placeBuildingEffect = Resources.Load<GameObject>("Prefabs/msVFX_Free Smoke Effects Pack/Prefabs/msVFX_Stylized Smoke 2");
        }
        Vector3 pos = new Vector3(0, 0, 0);
        PlaceBuilding(pos, "Cannon");
        Debug.Log("Hello");
        TestDummy testDummy = GameObject.Find("TestDummy").GetComponent<TestDummy>();
        TowerDefenseManager.Instance.RegisterTarget(testDummy);
    }

    /// <summary>
    /// Register all tower data with the TowerFactory
    /// </summary>
    private void RegisterTowerData()
    {
        // Cannon Tower
        TowerFactory.CreateTowerData("Cannon", TowerLevel.LevelOne, 800, 600, 1200, 400, 1.2f, 7f, 7.5f, CannonTowerSprite);
        TowerFactory.CreateTowerData("Cannon", TowerLevel.LevelTwo, 1000, 900, 1800, 600, 1.0f, 14f, 8.5f, CannonTowerSprite);
        TowerFactory.CreateTowerData("Cannon", TowerLevel.LevelThree, 1200, 1200, 2500, 800, 0.8f, 22f, 10f, CannonTowerSprite);

        // Town Hall (Does not attack)
        TowerFactory.CreateTowerData("TownHall", TowerLevel.LevelOne, 600, 1000, 2000, 1000, 0f, 0f, 0f, CannonTowerSprite);
        TowerFactory.CreateTowerData("TownHall", TowerLevel.LevelTwo, 800, 1500, 3000, 1500, 0f, 0f, 0f, CannonTowerSprite);
        TowerFactory.CreateTowerData("TownHall", TowerLevel.LevelThree, 1000, 2000, 4000, 2000, 0f, 0f, 0f, CannonTowerSprite);

        // Mine (Resource Generation)
        TowerFactory.CreateTowerData("Drill", TowerLevel.LevelOne, 600, 1000, 2000, 1200, 0f, 0f, 0f, FactoryTowerSprite);
        TowerFactory.CreateTowerData("Drill", TowerLevel.LevelTwo, 750, 1500, 2500, 1500, 0f, 0f, 0f, FactoryTowerSprite);
        TowerFactory.CreateTowerData("Drill", TowerLevel.LevelThree, 900, 2000, 3000, 1800, 0f, 0f, 0f, FactoryTowerSprite);

        // Mortar Tower (Slow but powerful AoE)
        TowerFactory.CreateTowerData("Mortar", TowerLevel.LevelOne, 450, 1200, 2400, 1000, 3.5f, 15f, 11f, MortarTowerSprite);
        TowerFactory.CreateTowerData("Mortar", TowerLevel.LevelTwo, 550, 1800, 3000, 1200, 2.8f, 25f, 12f, MortarTowerSprite);
        TowerFactory.CreateTowerData("Mortar", TowerLevel.LevelThree, 650, 2400, 3600, 1400, 2.2f, 45f, 14f, MortarTowerSprite);

        // Sniper Tower (Long range, high damage)
        TowerFactory.CreateTowerData("Sniper", TowerLevel.LevelOne, 700, 1000, 2000, 700, 2.5f, 10f, 16f, SniperTowerSprite);
        TowerFactory.CreateTowerData("Sniper", TowerLevel.LevelTwo, 800, 1500, 2500, 900, 2.0f, 30f, 17f, SniperTowerSprite);
        TowerFactory.CreateTowerData("Sniper", TowerLevel.LevelThree, 900, 2000, 3000, 1100, 1.5f, 50f, 18f, SniperTowerSprite);

        // AeroPlane Tower (Fast attacks, mobile)
        TowerFactory.CreateTowerData("Plane", TowerLevel.LevelOne, 550, 1500, 3000, 1000, 0.4f, 0.3f, 5f, AeroPlaneSprite);
        TowerFactory.CreateTowerData("Plane", TowerLevel.LevelTwo, 700, 2250, 4500, 1500, 0.2f, 0.5f, 6f, AeroPlaneSprite);
        TowerFactory.CreateTowerData("Plane", TowerLevel.LevelThree, 900, 3000, 6000, 2000, 0.1f, 0.7f, 7f, AeroPlaneSprite);
    }

    /// <summary>
    /// Places a building at the specified triangle location
    /// </summary>
    public void PlaceBuilding(Vector3 position, string towerType)
    {
        Debug.Log("Trying to place tower");

        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, position.normalized);

        SpawnTower(position, rotation, TowerFactory.GetTowerData(towerType, TowerData.TowerLevel.LevelOne), buildingPrefab);

        //  Event notification for game systems
        //  OnTowerPlaced(towerType, TowerData.TowerLevel.LevelOne);
    }

    /// <summary>
    /// Event notification when a tower is placed
    /// </summary>
    private void OnTowerPlaced(string towerType, TowerData.TowerLevel level)
    {
        // This would be handled by the game manager directly
        // Instead, we now fire an event that the game manager can subscribe to
        if (TowerPlaced != null)
        {
            TowerPlaced.Invoke(towerType, level);
        }
    }

    // Event for when a tower is placed
    public delegate void TowerPlacedHandler(string towerType, TowerData.TowerLevel level);
    public event TowerPlacedHandler TowerPlaced;

    /// <summary>
    /// Clears environment objects on the parent transform
    /// </summary>
    private void ClearEnvironment(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag("Environment"))
            {
                Destroy(child.gameObject);
            }
        }
    }

    /// <summary>
    /// Spawns a tower at the specified position
    /// </summary>
    private void SpawnTower(Vector3 position, Quaternion rotation, TowerData data, GameObject buildingPrefab)
    {
        bool prefabNull = buildingPrefab == null;
        Debug.Log("Prefab null" + prefabNull);
        GameObject building = Instantiate(buildingPrefab, position, rotation);
        Quaternion smokeRotation = Quaternion.Euler(0, 0, 90); // Adjust if needed
        float addedHeight = 0.55f;
        Vector3 smokeRaisedHeight = -transform.forward * addedHeight;
        //Instantiate(placeBuildingEffect, position + smokeRaisedHeight, smokeRotation);
        Tower tower = building.GetComponent<Tower>();
        tower.placed = true;
        tower.Initialise(data);
    }
}