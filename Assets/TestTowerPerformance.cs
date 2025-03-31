using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static TowerData;

public class TestTowerPerformance : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    float startTime;
    float timeSinceUpgrade = 0;
    TowerData.TowerLevel currentTowerLevel;
    DamageTestTarget damageTestTarget;
    int currentUpgradeLevelTest = 1;
    float currentDuration = 0;
    bool testFinished = false;

    // Put in the name of the tower you are testing here.
    string towername = "TestTowerOne";


    void Start()
    {
        DamageTestTarget damageTestTarget = GameObject.Find("DamageTestTarget").GetComponent<DamageTestTarget>();
        TowerDefenseManager.Instance.RegisterTarget(damageTestTarget);

        // Test New Tower
        TowerFactory.CreateTowerData(towername, TowerLevel.LevelOne, 1000, 900, 1800, 600, 1.0f, 14f, 8.5f, null);
        TowerFactory.CreateTowerData(towername, TowerLevel.LevelTwo, 1000, 900, 1800, 600, 1.0f, 14f, 8.5f, null);
        TowerFactory.CreateTowerData(towername, TowerLevel.LevelThree, 1200, 1200, 2500, 800, 0.8f, 22f, 10f, null);
        TowerPlacer towerPlacer = GameObject.Find("TowerManager").GetComponent<TowerPlacer>();
        string towerPrefabPath = "Prefabs/" + towername;
        GameObject testTower = Resources.Load<GameObject>(towerPrefabPath);
        towerPlacer.buildingPrefab = testTower;
        Vector3 pos = new Vector3(0, 0, 0);
        currentTowerLevel = TowerLevel.LevelOne;
        towerPlacer.PlaceBuilding(pos, towername, currentTowerLevel);
        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if(!testFinished)
        {
            timeSinceUpgrade = Time.time;
            currentDuration = timeSinceUpgrade - startTime;
            Debug.Log("Time elapsed: " + currentDuration);
            if (currentDuration > DamageTestTarget.TimeTestWindow)
            {
                startTime = Time.time;
                currentUpgradeLevelTest++;

                if (currentUpgradeLevelTest > DamageTestTarget.MaxTowerLevels)
                {
                    testFinished = true;
                    damageTestTarget = GameObject.Find("DamageTestTarget").GetComponent<DamageTestTarget>();

                    // Output the results
                    Debug.Log("Test Finished");
                    Debug.Log("Test Results:");
                    Debug.Log("Test: Level One DPS: " + damageTestTarget.GetTowerDamageFromLevelPerSecond(0));
                    Debug.Log("Test: Level Two DPS: " + damageTestTarget.GetTowerDamageFromLevelPerSecond(1));
                    Debug.Log("Test: Level Three DPS: " + damageTestTarget.GetTowerDamageFromLevelPerSecond(2));

                    damageTestTarget.gameObject.SetActive(false);
                    EditorApplication.ExitPlaymode();
                }
                else
                {
                    damageTestTarget = GameObject.Find("DamageTestTarget").GetComponent<DamageTestTarget>();
                    damageTestTarget.SwitchToNextLevel();
                    UpgradeBuilding();
                }
                
            }
        }
        
    }

    void UpgradeBuilding()
    {
        Tower placedTower = GameObject.FindAnyObjectByType<Tower>();
        TowerData towerData = placedTower.GetTowerData();
        if (towerData.Level != TowerData.TowerLevel.LevelThree)
        {
            TowerData.TowerLevel newLevel = TowerData.TowerLevelUtils.IncrementTowerLevel(towerData.Level);
            TowerData upgradedTowerData = TowerFactory.GetTowerData(towername, newLevel);
            int healthBeforeUpgrade = placedTower.GetCurrentHealth();
            placedTower.Initialise(upgradedTowerData); // Set new tower data
            placedTower.SetCurrentHealth(healthBeforeUpgrade);
        }
    }
}
