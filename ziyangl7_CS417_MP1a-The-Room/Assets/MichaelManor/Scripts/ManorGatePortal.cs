using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace MichaelManor
{
    /// <summary>
    /// Shared state and signifier behavior for a player-activated chamber Gate.
    /// Section 3 will connect successful activation requests to XR travel.
    /// </summary>
    public sealed class ManorGatePortal : MonoBehaviour
    {
        [SerializeField, Range(0, FiveChamberQuestController.RequiredChamberCount - 1)]
        private int chamberIndex;
        [SerializeField] private bool moonCryptGate;
        [SerializeField] private Transform destination;
        [SerializeField] private Transform returnAnchor;
        [SerializeField] private Renderer[] runeRenderers;
        [SerializeField] private Color lockedColor = new Color(0.08f, 0.06f, 0.10f, 1f);
        [SerializeField] private Color availableColor = new Color(0.62f, 0.18f, 1f, 1f);
        [SerializeField] private Color moonAvailableColor = new Color(0.48f, 0.86f, 1f, 1f);
        [SerializeField] private Color exploredColor = new Color(0.1f, 1f, 0.15f, 1f);
        [SerializeField] private TMP_Text exploredLabel;
        [SerializeField] private UnityEvent onActivationRequested = new UnityEvent();

        private bool unlocked;
        private bool explored;

        public event Action<ManorGatePortal> ActivationRequested;

        public int ChamberIndex => chamberIndex;
        public bool IsMoonCryptGate => moonCryptGate;
        public bool IsUnlocked => unlocked;
        public bool IsExplored => explored;
        public Transform Destination => destination;
        public Transform ReturnAnchor => returnAnchor;
        public TMP_Text ExploredLabel => exploredLabel;
        public UnityEvent OnActivationRequested => onActivationRequested;

        public void Configure(
            int index,
            bool isMoonCrypt,
            Transform travelDestination,
            Transform hallReturnAnchor,
            Renderer[] visualRenderers = null)
        {
            chamberIndex = Mathf.Clamp(index, 0, FiveChamberQuestController.RequiredChamberCount - 1);
            moonCryptGate = isMoonCrypt;
            destination = travelDestination;
            returnAnchor = hallReturnAnchor;
            runeRenderers = visualRenderers ?? Array.Empty<Renderer>();
            RefreshVisuals();
        }

        public void SetTravelAnchors(Transform travelDestination, Transform hallReturnAnchor)
        {
            destination = travelDestination;
            returnAnchor = hallReturnAnchor;
        }

        public void SetExploredLabel(TMP_Text label)
        {
            exploredLabel = label;
            RefreshVisuals();
        }

        public void SetUnlocked(bool value)
        {
            unlocked = value;
            RefreshVisuals();
        }

        public void SetExplored(bool value)
        {
            explored = value;
            RefreshVisuals();
        }

        /// <summary>
        /// Must be called by an explicit player interaction. Merely entering a trigger
        /// never invokes travel.
        /// </summary>
        public bool TryRequestActivation()
        {
            if (!unlocked)
            {
                return false;
            }

            ActivationRequested?.Invoke(this);
            onActivationRequested.Invoke();
            return true;
        }

        [ContextMenu("Request Gate Activation")]
        private void RequestActivationFromContextMenu()
        {
            TryRequestActivation();
        }

        private void OnValidate()
        {
            chamberIndex = Mathf.Clamp(chamberIndex, 0, FiveChamberQuestController.RequiredChamberCount - 1);
            RefreshVisuals();
        }

        private void RefreshVisuals()
        {
            if (exploredLabel != null)
            {
                exploredLabel.gameObject.SetActive(explored);
            }

            if (runeRenderers == null)
            {
                return;
            }

            Color stateColor = explored
                ? exploredColor
                : unlocked
                    ? moonCryptGate ? moonAvailableColor : availableColor
                    : lockedColor;

            foreach (Renderer runeRenderer in runeRenderers)
            {
                if (runeRenderer == null)
                {
                    continue;
                }

                MaterialPropertyBlock properties = new MaterialPropertyBlock();
                runeRenderer.GetPropertyBlock(properties);
                properties.SetColor("_BaseColor", stateColor);
                properties.SetColor("_EmissionColor", stateColor * (unlocked || explored ? 2f : 0.2f));
                runeRenderer.SetPropertyBlock(properties);
            }
        }
    }
}
