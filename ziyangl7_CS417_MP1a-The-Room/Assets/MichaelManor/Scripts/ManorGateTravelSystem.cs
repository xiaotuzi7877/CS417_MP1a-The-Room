using System;
using UnityEngine;

namespace MichaelManor
{
    /// <summary>
    /// Moves the complete XR rig only after an explicit Gate or Return Rune request.
    /// The tracked head is placed at the destination and aligned to its forward axis,
    /// which preserves room-scale head offset and keeps held objects parented to hands.
    /// </summary>
    public sealed class ManorGateTravelSystem : MonoBehaviour
    {
        [SerializeField] private Transform xrRig;
        [SerializeField] private Transform trackedHead;
        [SerializeField] private ManorGatePortal[] gates = Array.Empty<ManorGatePortal>();

        public Transform XrRig => xrRig;
        public Transform TrackedHead => trackedHead;

        public void Configure(Transform rig, Transform head, ManorGatePortal[] configuredGates)
        {
            Unsubscribe();
            xrRig = rig;
            trackedHead = head;
            gates = configuredGates ?? Array.Empty<ManorGatePortal>();
            if (isActiveAndEnabled)
            {
                Subscribe();
            }
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (gates == null)
            {
                return;
            }

            foreach (ManorGatePortal gate in gates)
            {
                if (gate == null)
                {
                    continue;
                }

                gate.ActivationRequested -= HandleGateActivation;
                gate.ActivationRequested += HandleGateActivation;
            }
        }

        private void Unsubscribe()
        {
            if (gates == null)
            {
                return;
            }

            foreach (ManorGatePortal gate in gates)
            {
                if (gate != null)
                {
                    gate.ActivationRequested -= HandleGateActivation;
                }
            }
        }

        private void HandleGateActivation(ManorGatePortal gate)
        {
            if (gate != null && gate.Destination != null)
            {
                TravelTo(gate.Destination);
            }
        }

        public bool TravelTo(Transform destination)
        {
            if (destination == null || xrRig == null || trackedHead == null)
            {
                return false;
            }

            float yawDelta = Mathf.DeltaAngle(trackedHead.eulerAngles.y, destination.eulerAngles.y);
            xrRig.RotateAround(trackedHead.position, Vector3.up, yawDelta);
            xrRig.position += destination.position - trackedHead.position;
            return true;
        }
    }
}
