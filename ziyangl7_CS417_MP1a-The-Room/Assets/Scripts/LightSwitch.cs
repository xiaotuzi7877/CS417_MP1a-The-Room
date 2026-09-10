using UnityEngine;
using UnityEngine.InputSystem;

public class LightSwitch : MonoBehaviour
{
    public InputActionReference action;

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
        Debug.Log("LightSwitch triggered!");

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
    }
}