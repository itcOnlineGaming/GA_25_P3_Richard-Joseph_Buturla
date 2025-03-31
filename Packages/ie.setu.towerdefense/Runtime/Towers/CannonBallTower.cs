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
        base.Initialise(data);
        attackStrategy = GetComponent<IAttackBehaviour>();
    }

    public override void Update()
    {
        Debug.Log("Update tower");

        if (!placed) { return; }

        FindTarget();
        if (targetEntity != null && attackStrategy != null)
        {
            Debug.Log("Entity Not Null");
            attackStrategy.Attack(targetEntity.Transform, firePoint, towerData.FireRate, towerData.Damage);
            LookAtTarget(targetEntity.Transform);
        }

        if (currentHealth <= 0)
        {
            Destroy(gameObject);
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
