using UnityEngine;
using UnityEngine.InputSystem;

public class LightSwitch : MonoBehaviour
{
    public InputActionReference action;

    public ParticleSystem lightBurstPrefab;
    public Transform burstPoint;

    private Light roomLight;
    private bool alternateColor = false;

    private void Awake()
    {
        roomLight = GetComponent<Light>();
    }

    private void OnEnable()
    {
        if (action != null)
        {
            action.action.Enable();
            action.action.performed += OnLightSwitch;
        }
    }

    private void OnDisable()
    {
        if (action != null)
        {
            action.action.performed -= OnLightSwitch;
            action.action.Disable();
        }
    }

    private void OnLightSwitch(InputAction.CallbackContext ctx)
    {
        alternateColor = !alternateColor;

        if (alternateColor)
        {
            roomLight.color = Color.red;
            roomLight.intensity = 5f;
        }
        else
        {
            roomLight.color = Color.white;
            roomLight.intensity = 2f;
        }

        if (lightBurstPrefab != null && burstPoint != null)
        {
            Instantiate(
                lightBurstPrefab,
                burstPoint.position,
                Quaternion.identity
            );
        }

        Debug.Log("LightSwitch triggered with particle feedback!");
    }
}