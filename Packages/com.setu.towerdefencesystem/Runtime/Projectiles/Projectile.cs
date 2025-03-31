using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    protected FiringTarget target;
    protected float speed;
    protected float damage;
    protected Vector3 planetCenter = Vector3.zero; // Default to world origin
    protected float gravityEffect = 0.1f;

    public virtual void Initialize(FiringTarget target, float speed, float damage)
    {
        this.target = target;
        this.speed = speed;
        this.damage = damage;
    }

    protected virtual void UpdateProjectile()
    {
        if (target.GetTargetTransform() == null || target.GetTargetPosition() == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 targetPosition;
        if (target.GetTargetTransform() != null)
        {
            targetPosition = target.GetTargetTransform().position;
        } else
        {
            targetPosition = target.GetTargetPosition();
        }



        Vector3 direction = (targetPosition - transform.position).normalized;
        Vector3 gravity = (planetCenter - transform.position).normalized * gravityEffect;

        Vector3 moveVector = (direction + gravity).normalized * speed * Time.deltaTime;
        transform.position += moveVector;

        transform.LookAt(transform.position + moveVector);
    }

    public static GameObject InstanciateProjectileWithPossibleParent(Transform creator, GameObject projectilePrefab, Vector3 firePoint)
    {
        Transform worldArea = null;
        if (creator.parent != null)
        {
            worldArea = creator.parent.transform.parent;
        }
        GameObject projectile = null;

        if (worldArea != null)
        {
            projectile = Instantiate(projectilePrefab, firePoint, Quaternion.identity, worldArea);
        }
        else
        {
            projectile = Instantiate(projectilePrefab, firePoint, Quaternion.identity);
        }

        return projectile;
    }
    
}
