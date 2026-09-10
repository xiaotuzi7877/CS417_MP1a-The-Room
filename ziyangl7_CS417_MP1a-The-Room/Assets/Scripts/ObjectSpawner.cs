using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectSpawner : MonoBehaviour
{
    public InputActionReference action;

    public GameObject objectPrefab;
    public Transform spawnPoint;

    public ParticleSystem spawnBurstPrefab;
    public GameObject spawnSoundPrefab;

    public Transform attractor;
    public float gravity = 0.2f;

    private void OnEnable()
    {
        if (action != null)
        {
            action.action.Enable();
            action.action.performed += OnSpawn;
        }
    }

    private void OnDisable()
    {
        if (action != null)
        {
            action.action.performed -= OnSpawn;
            action.action.Disable();
        }
    }

    private void OnSpawn(InputAction.CallbackContext ctx)
    {
        if (objectPrefab == null ||
            spawnPoint == null ||
            attractor == null)
        {
            Debug.LogWarning("ObjectSpawner is missing a required reference.");
            return;
        }

        // Spawn slightly in front of the controller
        Vector3 spawnPosition =
            spawnPoint.position +
            spawnPoint.forward * 0.5f;

        GameObject spawnedObject = Instantiate(
            objectPrefab,
            spawnPosition,
            spawnPoint.rotation
        );

        // Direction from attractor to spawned object
        Vector3 radialVector =
            spawnPosition - attractor.position;

        float distance = radialVector.magnitude;

        if (distance < 0.001f)
        {
            Debug.LogWarning("Spawned object is too close to attractor.");
            Destroy(spawnedObject);
            return;
        }

        Vector3 radialDirection =
            radialVector.normalized;

        // Remove the velocity component pointing
        // toward or away from the attractor.
        // This leaves a tangential direction.
        Vector3 tangentDirection =
            Vector3.ProjectOnPlane(
                spawnPoint.forward,
                radialDirection
            );

        // Fallback if controller is pointing almost
        // directly toward/away from attractor.
        if (tangentDirection.sqrMagnitude < 0.001f)
        {
            tangentDirection =
                Vector3.Cross(
                    radialDirection,
                    Vector3.up
                );

            if (tangentDirection.sqrMagnitude < 0.001f)
            {
                tangentDirection =
                    Vector3.Cross(
                        radialDirection,
                        Vector3.right
                    );
            }
        }

        tangentDirection.Normalize();

        // Perfect circular orbit speed:
        // sqrt(gravity / distance)
        float orbitalSpeed =
            Mathf.Sqrt(gravity / distance);

        Vector3 initialVelocity =
            tangentDirection * orbitalSpeed;

        ProjectileMotion projectile =
            spawnedObject.GetComponent<ProjectileMotion>();

        if (projectile != null)
        {
            projectile.InitializeOrbit(
                attractor,
                gravity,
                initialVelocity
            );
        }
        else
        {
            Debug.LogWarning(
                "Spawned object has no ProjectileMotion component."
            );
        }

        // Particle burst at spawn location
        if (spawnBurstPrefab != null)
        {
            Instantiate(
                spawnBurstPrefab,
                spawnPosition,
                Quaternion.identity
            );
        }

        // Spatial sound at spawn location
        if (spawnSoundPrefab != null)
        {
            Instantiate(
                spawnSoundPrefab,
                spawnPosition,
                Quaternion.identity
            );
        }

        Debug.Log(
            "Perfect Orbit spawned! " +
            "Distance = " + distance +
            ", Orbital Speed = " + orbitalSpeed
        );
    }
}