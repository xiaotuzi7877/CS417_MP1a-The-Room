using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectSpawner : MonoBehaviour
{
    public InputActionReference action;
    public GameObject objectPrefab;
    public Transform spawnPoint;

    public float launchSpeed = 3f;

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
        if (objectPrefab == null || spawnPoint == null)
            return;

        Vector3 spawnPosition =
            spawnPoint.position + spawnPoint.forward * 0.5f;

        GameObject spawnedObject = Instantiate(
            objectPrefab,
            spawnPosition,
            spawnPoint.rotation
        );

        ProjectileMotion projectile =
            spawnedObject.GetComponent<ProjectileMotion>();

        if (projectile != null)
        {
            projectile.SetVelocity(
                spawnPoint.forward * launchSpeed
            );
        }

        Debug.Log("Object shot!");
    }
}