using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AeroPlaneTower : Tower
{
    public GameObject planePrefab; 
    private GameObject spawnedPlane;
    private Vector3 normal;

    public override void Initialise(TowerData data)
    {
        towerData = data;
        currentHealth = data.MaxHealth;
        normal = transform.up;
        if(placed)
        {
            SpawnPlane();
        }
        
    }

    void SpawnPlane()
    {
        if (spawnedPlane == null)
        {
            GameObject planet = GameObject.Find("Planet"); // Get the planet object
            spawnedPlane = Instantiate(planePrefab, transform.position + normal * 5f, Quaternion.identity);

            // Make the plane a child of the planet instead of the tower 
            // Because in Unity apparently being a child of a parent means you can still move it.
            spawnedPlane.transform.SetParent(planet.transform, true);
        }

        AeroPlaneUnit planeUnit = spawnedPlane.GetComponent<AeroPlaneUnit>();
        if (planeUnit != null)
        {
            planeUnit.Initialise(this, towerData, normal);
        }
    }


    public override void Update()
    {
        if (!placed) return;

        if (currentHealth <= 0)
        {
            Destroy(spawnedPlane);
            Destroy(gameObject);
        }
    }
}
