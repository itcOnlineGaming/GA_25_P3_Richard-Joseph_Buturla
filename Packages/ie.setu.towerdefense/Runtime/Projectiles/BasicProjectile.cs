using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicProjectile : Projectile
{
    public override void Initialize(FiringTarget target, float speed, float damage)
    {
        base.Initialize(target, speed, damage);
    }

    void Update()
    {
        if (target.GetTargetTransform() == null || target.GetTargetPosition() == null)
        {
            Destroy(gameObject);
            return;
        }

        UpdateProjectile();
    }

    public void OnTriggerEnter(Collider other)
    {
        ITargetable targetable = other.GetComponent<ITargetable>();
        if (targetable != null)
        {
            targetable.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}