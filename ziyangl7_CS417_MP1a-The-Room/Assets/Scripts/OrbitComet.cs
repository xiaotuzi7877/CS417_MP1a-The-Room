using UnityEngine;

public class OrbitComet : MonoBehaviour
{
    public Transform planet;

    public float gravity = 0.2f;

    private Vector3 velocity = new Vector3(0f, 0f, 0.2f);

    void Update()
    {
        Vector3 offset = transform.position - planet.position;

        float distance = offset.magnitude;

        Vector3 acceleration =
            -gravity * offset / Mathf.Pow(distance, 3);

        velocity += acceleration * Time.deltaTime;

        transform.position += velocity * Time.deltaTime;
    }
}