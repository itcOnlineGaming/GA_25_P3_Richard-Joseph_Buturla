using UnityEngine;

public class FiringTarget
{
    private Transform targetTransform;
    private Vector3 targetPosition;

    public FiringTarget(Transform targetTransform)
    {
        this.targetTransform = targetTransform;
        this.targetPosition = targetTransform.position;
    }

    public FiringTarget(Vector3 targetPosition)
    {
        this.targetPosition = targetPosition;
    }

    public Transform GetTargetTransform() => targetTransform;
    public Vector3 GetTargetPosition() => targetPosition;

}


// Interface for attack behaviour for different buildings
public interface IAttackBehaviour
{
    void Attack(FiringTarget target, Transform firePoint, float fireRate, float damage);
}
