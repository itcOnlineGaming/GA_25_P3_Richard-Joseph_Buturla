using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanonBallProjectile : Projectile
{
    public override void Initialize(Transform target, float speed, float damage)
    {
        base.Initialize(target, speed, damage);
    }

    void Update()
    {
        if (target == null)
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

            TowerDefenseEvents.RaiseProjectileHit(HitGoldGain);
        }
    }
}