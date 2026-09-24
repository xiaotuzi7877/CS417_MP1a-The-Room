using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MichaelManor
{
    [RequireComponent(typeof(XRSimpleInteractable))]
    public sealed class ManorGateInteractor : MonoBehaviour
    {
        [SerializeField] private ManorGatePortal gate;
        private XRSimpleInteractable interactable;

        public void Configure(ManorGatePortal configuredGate)
        {
            gate = configuredGate;
        }

        private void Awake()
        {
            interactable = GetComponent<XRSimpleInteractable>();
        }

        private void OnEnable()
        {
            if (interactable == null)
            {
                interactable = GetComponent<XRSimpleInteractable>();
            }
            interactable.selectEntered.AddListener(HandleSelected);
        }

        private void OnDisable()
        {
            if (interactable != null)
            {
                interactable.selectEntered.RemoveListener(HandleSelected);
            }
        }

        public bool TryActivate()
        {
            return gate != null && gate.TryRequestActivation();
        }

        private void HandleSelected(SelectEnterEventArgs args)
        {
            TryActivate();
        }
    }
}
