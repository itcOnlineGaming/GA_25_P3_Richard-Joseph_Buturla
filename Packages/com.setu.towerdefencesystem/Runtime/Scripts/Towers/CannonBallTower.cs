using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBallTower : Tower
{
    private IAttackBehaviour attackStrategy;
    private ITargetable targetEntity;

    public GameObject Turret;

    public override void Initialise(TowerData data)
    {
        if (data.ID.NullID())
        {
            data.ID = TowerData.GetNewTowerID();
        }
        base.Initialise(data);
        attackStrategy = GetComponent<IAttackBehaviour>();
    }

    public override void Update()
    {
        if (!placed) { return; }

        FindTarget();
        if (targetEntity != null && attackStrategy != null)
        {
            FiringTarget firingTarget = new FiringTarget(targetEntity.Transform);
            attackStrategy.Attack(firingTarget, firePoint, towerData.FireRate, towerData.Damage);
            LookAtTarget(targetEntity.Transform);
        }

        if (currentHealth <= 0)
        {
            TowerDefenseEvents.RaiseTowerDestroyed(this);
            GameObject.Destroy(gameObject);
        }
    }

    void LookAtTarget(Transform targetPos)
    {
        Vector3 aimVector;

        aimVector = targetPos.position - Turret.transform.position; //vector to target pos
        aimVector = Vector3.ProjectOnPlane(aimVector, Turret.transform.up); //project it on an x-z plane 

        Quaternion q_penguin = Quaternion.LookRotation(aimVector, Turret.transform.up);

        Turret.transform.rotation = Quaternion.Slerp(Turret.transform.rotation, q_penguin, 10f * Time.deltaTime); //slowly transition
    }

    void FindTarget()
    {
        ITargetable nearestTarget = FindNearestTarget();

        if (targetEntity == null || !targetEntity.IsAlive)
        {
            targetEntity = nearestTarget;
        }
    }
}
