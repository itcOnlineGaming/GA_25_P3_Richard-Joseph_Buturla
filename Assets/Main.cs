using UnityEngine;
using static TowerData;

public class Main : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TestDummy testDummy = GameObject.Find("TestDummy").GetComponent<TestDummy>();
        TowerDefenseManager.Instance.RegisterTarget(testDummy);

        // Test New Tower
        string towername = "TestTowerOne";
        TowerFactory.CreateTowerData(towername, TowerLevel.LevelOne, 1000, 900, 1800, 600, 1.0f, 14f, 8.5f, null);
        TowerPlacer towerPlacer = GameObject.Find("TowerManager").GetComponent<TowerPlacer>();
        string towerPrefabPath = "Prefabs/" + towername;
        GameObject testTower = Resources.Load<GameObject>(towerPrefabPath);
        towerPlacer.buildingPrefab = testTower;
        Vector3 pos = new Vector3(0, 0, 0);
        towerPlacer.PlaceBuilding(pos, towername, TowerData.TowerLevel.LevelOne);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
