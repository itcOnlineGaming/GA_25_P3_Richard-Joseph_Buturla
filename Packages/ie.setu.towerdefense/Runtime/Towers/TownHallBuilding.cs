using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TownHallTower : Tower
{
    public override void Initialise(TowerData data)
    {
        towerData = data;
        currentHealth = data.MaxHealth;
    }

    public override void Update()
    {
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
            GameManager.Instance.GameOver();
            GamePlayUIController.Instance.OpenGameLose();
        }
    }
}
