using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MortarTower : Tower
{
    private IAttackBehaviour attackStrategy;
    private ITargetable targetEntity;

    public GameObject Turret;

    [Header("Mortar Specific")]
    public float projectileArcHeight = 5f; // Arc height for the mortar shot

    public override void Initialise(TowerData data)
    {
        base.Initialise(data);
        attackStrategy = GetComponent<IAttackBehaviour>(); // Assigns the attack strategy
    }

    public override void Update()
    {
        if (!placed) return; // Only attack when placed

        FindTarget();

        if (targetEntity != null && attackStrategy != null)
        {
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
        if (targetPos == null) return;

        Vector3 aimVector = targetPos.position - Turret.transform.position;
        aimVector = Vector3.ProjectOnPlane(aimVector, Turret.transform.up); // Keeps aim level on planet surface

        Quaternion targetRotation = Quaternion.LookRotation(aimVector, Turret.transform.up);
        Turret.transform.rotation = Quaternion.Slerp(Turret.transform.rotation, targetRotation, 5f * Time.deltaTime);
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
