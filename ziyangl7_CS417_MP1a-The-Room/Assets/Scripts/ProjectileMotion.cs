using UnityEngine;

public class ProjectileMotion : MonoBehaviour
{
    private Vector3 velocity;

    public void SetVelocity(Vector3 newVelocity)
    {
        velocity = newVelocity;
    }

    void Update()
    {
        transform.position += velocity * Time.deltaTime;
    }
}
