using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Calculates the placement for towers on the planet

using TowerLevel = TowerData.TowerLevel;


public class TowerPlacer : MonoBehaviour
{


    //public PlanetGeneration planetGenerator;
    
    public GameObject buildingPrefab;

    public GameObject placeBuildingEffect;

    public Sprite CannonTowerSprite;
    public Sprite FactoryTowerSprite;
    public Sprite MortarTowerSprite;
    public Sprite SniperTowerSprite;
    public Sprite AeroPlaneSprite;

    private void Awake()
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


    private void Start()
    {
        //planetGenerator = GameObject.Find("PlanetMaker").GetComponent<PlanetGeneration>();
        placeBuildingEffect = Resources.Load<GameObject>("Prefabs/msVFX_Free Smoke Effects Pack/Prefabs/msVFX_Stylized Smoke 2");

    }
    public void PlaceBuilding(GameObject triangle, string towerType)
    {
        Debug.Log("trying to place tower");

        if (triangle == null || GameObject.Find("Planet") == null) 
        {
            Debug.LogError("triangle not initialized.");
            return;
        }
        Vector3 position = triangle.transform.Find("centroid").transform.position;

        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, position.normalized);

        SpawnTower(position, rotation, triangle.transform, TowerFactory.GetTowerData(towerType, TowerLevel.LevelOne), buildingPrefab);

        EnemySpawner enemySpawner = FindObjectOfType<EnemySpawner>();
        enemySpawner.startSpawning();

        GameManager.Instance.UpdateTowersPlaced();
        GameManager.Instance.TowerDataUpdate(towerType, TowerLevel.LevelOne);

    }

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


    private void SpawnTower(Vector3 position, Quaternion rotation, Transform parent, TowerData data, GameObject buildingPrefab)
    {
        ClearEnvironment(parent);
        GameObject building = Instantiate(buildingPrefab, position, rotation, parent);
        Quaternion smokeRotation = Quaternion.Euler(0, 0, 90); // Adjust if needed
        float addedHeight = 0.55f;
        Vector3 smokeRaisedHeight = -transform.forward * addedHeight;
        Instantiate(placeBuildingEffect, position + smokeRaisedHeight, smokeRotation, parent);
        Tower tower = building.GetComponent<Tower>();
        tower.placed = true;
        tower.Initialise(data);
    }

}