using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MichaelManor
{
    [RequireComponent(typeof(XRSimpleInteractable))]
    public sealed class ManorKeyReleaseButton : MonoBehaviour
    {
        [SerializeField] private ManorKeyReleasePuzzle puzzle;
        [SerializeField] private int buttonIndex;
        private XRSimpleInteractable interactable;
        public void Configure(ManorKeyReleasePuzzle owner, int index) { puzzle = owner; buttonIndex = index; }
        private void Awake() => interactable = GetComponent<XRSimpleInteractable>();
        private void OnEnable() { if (interactable == null) interactable = GetComponent<XRSimpleInteractable>(); interactable.selectEntered.AddListener(Selected); }
        private void OnDisable() { if (interactable != null) interactable.selectEntered.RemoveListener(Selected); }
        private void Selected(SelectEnterEventArgs args) => puzzle?.Press(buttonIndex);
        public bool PressForTest() => puzzle != null && puzzle.Press(buttonIndex);
    }
}
