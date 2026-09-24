using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MichaelManor
{
    /// Holds a displayed prop still (kinematic) until the player first lets go of it, after which it
    /// uses normal physics. XRGrabInteractable would otherwise restore the kinematic flag on release
    /// and leave the prop floating where it was dropped.
    [RequireComponent(typeof(XRGrabInteractable), typeof(Rigidbody))]
    public sealed class ManorRestUntilGrabbed : MonoBehaviour
    {
        private XRGrabInteractable grab;
        private Rigidbody body;

        private void Awake()
        {
            grab = GetComponent<XRGrabInteractable>();
            body = GetComponent<Rigidbody>();
        }

        private void OnEnable() => grab.selectExited.AddListener(Released);
        private void OnDisable() => grab.selectExited.RemoveListener(Released);

        private void Released(SelectExitEventArgs args)
        {
            if (args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor) return;
            body.isKinematic = false;
        }
    }
}
