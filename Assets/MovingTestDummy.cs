using UnityEngine;

public class MovingTestDummy : MonoBehaviour
{
    private Vector3 centerPoint;
    private float radius = 5f;
    private float speed = 1f;
    private float angle = 0f;

    public void Setup(Vector3 center, float moveRadius, float moveSpeed)
    {
        centerPoint = center;
        radius = moveRadius;
        speed = moveSpeed;
    }

    private void Update()
    {
        // Circular movement pattern
        angle += speed * Time.deltaTime;

        Vector3 offset = new Vector3(
            Mathf.Cos(angle) * radius,
            0f,
            Mathf.Sin(angle) * radius
        );

        transform.position = centerPoint + offset;
    }
}
