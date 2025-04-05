using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
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
    [SerializeField]
    public string towername = "Sniper";

    Tower testedTower;

    private void TestCannonParams()
    {
        TowerFactory.CreateTowerData(towername, TowerLevel.LevelOne, 1000, 900, 1800, 600, 1.0f, 14f, 15.5f, null);
        TowerFactory.CreateTowerData(towername, TowerLevel.LevelTwo, 1000, 900, 1800, 600, 1.0f, 14f, 18.5f, null);
        TowerFactory.CreateTowerData(towername, TowerLevel.LevelThree, 1200, 1200, 2500, 800, 0.8f, 22f, 24f, null);
    }

    private void TestSniperParams()
    {
        TowerFactory.CreateTowerData(towername, TowerLevel.LevelOne, 700, 550, 1400, 525, 2.5f, 20f, 12f, null);
        TowerFactory.CreateTowerData(towername, TowerLevel.LevelTwo, 1050, 650, 2100, 800, 2.0f, 30f, 14f, null);
        TowerFactory.CreateTowerData(towername, TowerLevel.LevelThree, 1575, 1150, 3150, 1200, 1.5f, 50f, 16f, null);
    }

    void Start()
    {
        DamageTestTarget damageTestTarget = GameObject.Find("DamageTestTarget").GetComponent<DamageTestTarget>();
        TowerDefenseManager.Instance.RegisterTarget(damageTestTarget);

        // Test New Tower
        TestCannonParams();
        string towerPrefabPath = "Prefabs/" + towername;
        GameObject testTower = Resources.Load<GameObject>(towerPrefabPath);

        startTime = Time.time;
        Vector3 pos = new Vector3(0, 0, 0);

        currentTowerLevel = TowerLevel.LevelOne;
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, pos.normalized);
        testedTower = TowerUtils.SpawnTower(pos, rotation, TowerFactory.GetTowerData(towername, TowerData.TowerLevel.LevelOne), testTower);

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
                    TowerUtils.UpgradeBuilding(testedTower);
                }
                
            }
        }
        
    }
}
