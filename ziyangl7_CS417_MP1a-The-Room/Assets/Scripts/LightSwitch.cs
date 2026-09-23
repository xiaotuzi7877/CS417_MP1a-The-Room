using UnityEngine;
using UnityEngine.InputSystem;
using MichaelManor;

public class LightSwitch : MonoBehaviour
{
    public InputActionReference action;

    public ParticleSystem lightBurstPrefab;
    public Transform burstPoint;
    public ManorFeedbackNetwork feedbackNetwork;

    private Light roomLight;
    private bool alternateColor = false;
    private Color originalColor;
    private float originalIntensity;
    private InputAction activeAction;
    private InputAction runtimeAction;

    private void Awake()
    {
        roomLight = GetComponent<Light>();
        originalColor = roomLight.color;
        originalIntensity = roomLight.intensity;
    }

    private void OnEnable()
    {
        activeAction = action != null ? action.action : CreateRuntimeAction();
        activeAction.performed += OnLightSwitch;
        activeAction.Enable();
    }

    private void OnDisable()
    {
        if (activeAction != null)
        {
            activeAction.performed -= OnLightSwitch;
            activeAction.Disable();
        }

        runtimeAction?.Dispose();
        runtimeAction = null;
        activeAction = null;
    }

    private void OnLightSwitch(InputAction.CallbackContext ctx)
    {
        alternateColor = !alternateColor;

        if (alternateColor)
        {
            roomLight.color = new Color(0.30f, 0.62f, 1f);
            roomLight.intensity = originalIntensity * 1.35f;
        }
        else
        {
            roomLight.color = originalColor;
            roomLight.intensity = originalIntensity;
        }

        if (lightBurstPrefab != null && burstPoint != null)
        {
            Instantiate(
                lightBurstPrefab,
                burstPoint.position,
                Quaternion.identity
            );
        }

        feedbackNetwork?.PlayLightFeedback();

        Debug.Log("LightSwitch triggered with particle feedback!");
    }

    private InputAction CreateRuntimeAction()
    {
        runtimeAction = new InputAction("Change Hall Light", InputActionType.Button);
        runtimeAction.AddBinding("<XRController>{LeftHand}/primaryButton");
        runtimeAction.AddBinding("<Keyboard>/l");
        return runtimeAction;
    }
}
