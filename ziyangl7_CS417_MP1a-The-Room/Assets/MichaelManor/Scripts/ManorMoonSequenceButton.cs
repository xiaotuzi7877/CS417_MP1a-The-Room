using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MichaelManor
{
    [RequireComponent(typeof(XRSimpleInteractable))]
    public sealed class ManorMoonSequenceButton : MonoBehaviour
    {
        [SerializeField] private ManorMoonCryptPuzzle puzzle;
        [SerializeField, Range(0, 2)] private int sequenceIndex;
        private XRSimpleInteractable interactable;

        public int SequenceIndex => sequenceIndex;

        public void Configure(ManorMoonCryptPuzzle controller, int index)
        {
            puzzle = controller;
            sequenceIndex = Mathf.Clamp(index, 0, 2);
        }

        private void Awake()
        {
            interactable = GetComponent<XRSimpleInteractable>();
        }

        private void OnEnable()
        {
            if (interactable == null) interactable = GetComponent<XRSimpleInteractable>();
            interactable.selectEntered.AddListener(HandleSelected);
        }

        private void OnDisable()
        {
            if (interactable != null) interactable.selectEntered.RemoveListener(HandleSelected);
        }

        public bool Press()
        {
            return puzzle != null && puzzle.PressButton(sequenceIndex);
        }

        private void HandleSelected(SelectEnterEventArgs args)
        {
            Press();
        }
    }
}
