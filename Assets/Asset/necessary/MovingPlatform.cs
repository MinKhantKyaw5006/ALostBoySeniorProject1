using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform platform;
    public Transform startPoint;
    public Transform endPoint;

    [SerializeField] private float speed = 1.5f;
    private int direction = 1;
    private void Update()
    {
        Vector2 target = currentMovementTarget();

        platform.position = Vector2.Lerp(platform.position, target, speed * Time.deltaTime);

        float distance = (target - (Vector2)platform.position).magnitude;
        if (distance <= 0.1f)
        {
            direction *= -1;
        }

    }

    private Vector2 currentMovementTarget()
    {
        if (direction == 1)
        {
            return startPoint.position;
        }

        else
        {
            return endPoint.position;
        }
    }
    private void OnDrawGizmos()
    {
        if (platform == null && startPoint != null && endPoint != null)
        {
            Gizmos.DrawLine(platform.transform.position, startPoint.position);
            Gizmos.DrawLine(platform.transform.position, endPoint.position);
        }
    }
}
