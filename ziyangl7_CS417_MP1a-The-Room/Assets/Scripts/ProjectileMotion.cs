using UnityEngine;

public class ProjectileMotion : MonoBehaviour
{
    private Vector3 velocity;

    private Transform attractor;
    private float gravity;

    private bool useOrbitGravity = false;

    public void InitializeOrbit(
        Transform newAttractor,
        float newGravity,
        Vector3 initialVelocity)
    {
        attractor = newAttractor;
        gravity = newGravity;
        velocity = initialVelocity;

        useOrbitGravity = true;
    }

    private void Update()
    {
        if (useOrbitGravity && attractor != null)
        {
            Vector3 offset =
                transform.position - attractor.position;

            float distance = offset.magnitude;

            if (distance > 0.001f)
            {
                Vector3 acceleration =
                    -gravity *
                    offset /
                    Mathf.Pow(distance, 3);

                // First integration:
                // acceleration -> velocity
                velocity +=
                    acceleration * Time.deltaTime;
            }
        }

        // Second integration:
        // velocity -> position
        transform.position +=
            velocity * Time.deltaTime;
    }
}