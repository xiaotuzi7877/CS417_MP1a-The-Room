using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MichaelManor
{
    [RequireComponent(typeof(XRSimpleInteractable))]
    public sealed class ManorReturnRune : MonoBehaviour
    {
        [SerializeField] private ManorGateTravelSystem travelSystem;
        [SerializeField] private Transform hallReturnAnchor;
        [SerializeField] private int chamberIndex;
        private XRSimpleInteractable interactable;

        public Transform HallReturnAnchor => hallReturnAnchor;
        public int ChamberIndex => chamberIndex;
        public event Action<ManorReturnRune> ReturnRequested;

        public void Configure(ManorGateTravelSystem system, Transform returnAnchor, int index)
        {
            travelSystem = system;
            hallReturnAnchor = returnAnchor;
            chamberIndex = index;
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

        public bool TryReturn()
        {
            bool returned = travelSystem != null && travelSystem.TravelTo(hallReturnAnchor);
            if (returned) ReturnRequested?.Invoke(this);
            return returned;
        }

        private void HandleSelected(SelectEnterEventArgs args)
        {
            TryReturn();
        }
    }
}
