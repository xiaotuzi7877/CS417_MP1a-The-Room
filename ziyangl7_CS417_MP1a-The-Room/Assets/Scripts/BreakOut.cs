using UnityEngine;
using UnityEngine.InputSystem;

public class BreakOut : MonoBehaviour
{
    public InputActionReference action;

    public Transform xrOrigin;
    public Transform outsidePoint;

    public ParticleSystem breakOutBurstPrefab;
    public ParticleSystem returnBurstPrefab;

    public Transform insideBurstPoint;
    public Transform outsideBurstPoint;

    private Vector3 insidePosition;
    private Quaternion insideRotation;

    private bool isOutside = false;
    private InputAction activeAction;
    private InputAction runtimeAction;

    private void Start()
    {
        insidePosition = xrOrigin.position;
        insideRotation = xrOrigin.rotation;
    }

    private void OnEnable()
    {
        activeAction = action != null ? action.action : CreateRuntimeAction();
        activeAction.performed += OnBreakOut;
        activeAction.Enable();
    }

    private void OnDisable()
    {
        if (activeAction != null)
        {
            activeAction.performed -= OnBreakOut;
            activeAction.Disable();
        }

        runtimeAction?.Dispose();
        runtimeAction = null;
        activeAction = null;
    }

    private void OnBreakOut(InputAction.CallbackContext ctx)
    {
        isOutside = !isOutside;

        if (isOutside)
        {
            xrOrigin.position = outsidePoint.position;
            xrOrigin.rotation = outsidePoint.rotation;

            if (breakOutBurstPrefab != null &&
                outsideBurstPoint != null)
            {
                Instantiate(
                    breakOutBurstPrefab,
                    outsideBurstPoint.position,
                    Quaternion.identity
                );
            }

            Debug.Log("Break Out: Outside with particle feedback");
        }
        else
        {
            xrOrigin.position = insidePosition;
            xrOrigin.rotation = insideRotation;

            if (returnBurstPrefab != null &&
                insideBurstPoint != null)
            {
                Instantiate(
                    returnBurstPrefab,
                    insideBurstPoint.position,
                    Quaternion.identity
                );
            }

            Debug.Log("Break Out: Inside with particle feedback");
        }
    }

    private InputAction CreateRuntimeAction()
    {
        runtimeAction = new InputAction("Break Out", InputActionType.Button);
        runtimeAction.AddBinding("<XRController>{RightHand}/secondaryButton");
        runtimeAction.AddBinding("<Keyboard>/b");
        return runtimeAction;
    }
}
