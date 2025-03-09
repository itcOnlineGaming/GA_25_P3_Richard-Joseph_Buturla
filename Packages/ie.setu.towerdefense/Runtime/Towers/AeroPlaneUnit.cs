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
        Vector3 planetCenter = GameObject.Find("Planet").transform.position;

        Vector3 trueNormal = (parentTower.transform.position - planetCenter).normalized;

        angle += rotationSpeed * Time.deltaTime;

        Vector3 initialOffset = Vector3.Cross(trueNormal, Vector3.right).normalized * rotationRadius;

        Vector3 offset = Quaternion.AngleAxis(angle, trueNormal) * initialOffset;

        transform.position = parentTower.transform.position + offset;

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
            attackBehaviour.Attack(target.Transform, firePoint, towerData.FireRate, towerData.Damage);
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
