using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AeroPlaneUnit : MonoBehaviour
{
    private AeroPlaneTower parentTower;
    private float rotationRadius = 10f;
    private float rotationSpeed = 25.0f;
    private float angle;
    private Vector3 normal;
    private Vector3 initialOffset;
    private float heightOffset = 10f; 


    public Transform firePoint;

    private IAttackBehaviour attackBehaviour;
    private TowerData towerData;

    public void Initialise(AeroPlaneTower tower, TowerData towerData, Vector3 normal)
    {
        this.parentTower = tower;
        this.normal = normal.normalized;
        this.towerData = towerData;
        attackBehaviour = GetComponent<IAttackBehaviour>();
        initialOffset = parentTower.transform.right * rotationRadius;
    }


    void UpdateMovement()
    {
        GameObject planet = GameObject.Find("Planet");
        Vector3 trueNormal = new Vector3();

        if (planet != null)
        {
            Vector3 planetCenter = planet.transform.position;
            trueNormal = (parentTower.transform.position - planetCenter).normalized;
        }
        else
        {
            trueNormal = Vector3.up;
        }

        angle += rotationSpeed * Time.deltaTime;

        Vector3 initialOffset = Vector3.Cross(trueNormal, Vector3.right).normalized * rotationRadius;
        Vector3 offset = Quaternion.AngleAxis(angle, trueNormal) * initialOffset;

        Vector3 heightAdjustment = trueNormal * heightOffset;

        transform.position = parentTower.transform.position + offset + heightAdjustment;

        Vector3 tangent = Vector3.Cross(trueNormal, offset).normalized;
        transform.rotation = Quaternion.LookRotation(tangent, trueNormal);
    }


    void Update()
    {
        if (parentTower == null) return;

        UpdateMovement();

        ITargetable target = FindClosestTarget();
        if (target != null && attackBehaviour != null)
        {
            FiringTarget firingTarget = new FiringTarget(target.Transform);
            attackBehaviour.Attack(firingTarget, firePoint, towerData.FireRate, towerData.Damage);
        }
    }

    ITargetable FindClosestTarget()
    {
        List<ITargetable> targets = TowerDefenseManager.Instance.GetAllTargets();
        ITargetable closest = null;
        float minDistance = Mathf.Infinity;

        foreach (ITargetable target in targets)
        {
            if (target == null || !target.IsAlive) continue;

            float distance = Vector3.Distance(transform.position, target.Position);
            if (distance < minDistance && distance <= towerData.Range)
            {
                minDistance = distance;
                closest = target;
            }
        }

        return closest;
    }
}
