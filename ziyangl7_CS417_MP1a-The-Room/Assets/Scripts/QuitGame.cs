using UnityEngine;
using UnityEngine.InputSystem;

public class QuitGame : MonoBehaviour
{
    public InputActionReference action;

    private InputAction activeAction;
    private InputAction runtimeAction;

    private void OnEnable()
    {
        activeAction = action != null ? action.action : CreateRuntimeAction();
        activeAction.performed += OnQuit;
        activeAction.Enable();
    }

    private void OnDisable()
    {
        if (activeAction != null)
        {
            activeAction.performed -= OnQuit;
            activeAction.Disable();
        }

        runtimeAction?.Dispose();
        runtimeAction = null;
        activeAction = null;
    }

    private InputAction CreateRuntimeAction()
    {
        runtimeAction = new InputAction("Quit", InputActionType.Button);
        runtimeAction.AddBinding("<XRController>{RightHand}/primaryButton");
        runtimeAction.AddBinding("<Keyboard>/q");
        return runtimeAction;
    }

    private static void OnQuit(InputAction.CallbackContext context)
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
