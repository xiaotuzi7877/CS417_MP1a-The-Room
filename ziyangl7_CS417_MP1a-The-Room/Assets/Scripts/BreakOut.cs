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

    private void Start()
    {
        insidePosition = xrOrigin.position;
        insideRotation = xrOrigin.rotation;
    }

    private void OnEnable()
    {
        if (action != null)
        {
            action.action.Enable();
            action.action.performed += OnBreakOut;
        }
    }

    private void OnDisable()
    {
        if (action != null)
        {
            action.action.performed -= OnBreakOut;
            action.action.Disable();
        }
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
}