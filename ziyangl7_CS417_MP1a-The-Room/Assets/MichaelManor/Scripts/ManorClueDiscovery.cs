using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MichaelManor
{
    [RequireComponent(typeof(XRSimpleInteractable))]
    public sealed class ManorClueDiscovery : MonoBehaviour
    {
        [SerializeField] private ManorPuzzleProgressTracker tracker;
        [SerializeField] private int clueIndex;
        private XRSimpleInteractable interactable;
        public void Configure(ManorPuzzleProgressTracker owner, int index) { tracker = owner; clueIndex = index; }
        private void Awake() => interactable = GetComponent<XRSimpleInteractable>();
        private void OnEnable() { if (interactable == null) interactable = GetComponent<XRSimpleInteractable>(); interactable.selectEntered.AddListener(Selected); }
        private void OnDisable() { if (interactable != null) interactable.selectEntered.RemoveListener(Selected); }
        private void Selected(SelectEnterEventArgs args) => tracker?.DiscoverClue(clueIndex);
        public bool DiscoverForTest() => tracker != null && tracker.DiscoverClue(clueIndex);
    }
}
