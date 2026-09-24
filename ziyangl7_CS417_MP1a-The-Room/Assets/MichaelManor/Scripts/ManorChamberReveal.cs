using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MichaelManor
{
    /// <summary>
    /// Explicit one-shot chamber mechanism. It eases a physical obstruction away,
    /// then records exploration and exposes the chamber's FALSE RELIC feedback.
    /// </summary>
    [RequireComponent(typeof(XRSimpleInteractable))]
    public sealed class ManorChamberReveal : MonoBehaviour
    {
        [SerializeField] private ManorChamberInteraction chamberInteraction;
        [SerializeField] private Transform movingPart;
        [SerializeField] private Vector3 openLocalPosition;
        [SerializeField] private Vector3 openLocalEuler;
        [SerializeField] private float duration = 1.0f;
        [SerializeField] private Rigidbody revealedRelic;

        private XRSimpleInteractable interactable;
        private Vector3 closedLocalPosition;
        private Quaternion closedLocalRotation;
        private Coroutine revealRoutine;

        public bool IsCompleted { get; private set; }
        public bool IsAnimating => revealRoutine != null;
        public Rigidbody RevealedRelic => revealedRelic;

        public void Configure(
            ManorChamberInteraction interaction,
            Transform obstruction,
            Vector3 targetLocalPosition,
            Vector3 targetLocalEuler,
            float animationDuration,
            Rigidbody relic)
        {
            chamberInteraction = interaction;
            movingPart = obstruction;
            openLocalPosition = targetLocalPosition;
            openLocalEuler = targetLocalEuler;
            duration = Mathf.Max(0.1f, animationDuration);
            revealedRelic = relic;
        }

        private void Awake()
        {
            interactable = GetComponent<XRSimpleInteractable>();
            CaptureClosedState();
        }

        private void OnEnable()
        {
            if (interactable == null)
            {
                interactable = GetComponent<XRSimpleInteractable>();
            }
            interactable.selectEntered.AddListener(HandleSelected);
            if (chamberInteraction != null && chamberInteraction.QuestController != null)
            {
                chamberInteraction.QuestController.QuestReset += ResetReveal;
            }
        }

        private void OnDisable()
        {
            if (interactable != null)
            {
                interactable.selectEntered.RemoveListener(HandleSelected);
            }
            if (chamberInteraction != null && chamberInteraction.QuestController != null)
            {
                chamberInteraction.QuestController.QuestReset -= ResetReveal;
            }
        }

        public bool TryActivate()
        {
            if (IsCompleted || revealRoutine != null || movingPart == null || chamberInteraction == null)
            {
                return false;
            }

            revealRoutine = StartCoroutine(RevealRoutine());
            return true;
        }

        public void ResetReveal()
        {
            if (revealRoutine != null)
            {
                StopCoroutine(revealRoutine);
                revealRoutine = null;
            }
            IsCompleted = false;
            chamberInteraction?.ResetInteraction();
            if (movingPart != null)
            {
                movingPart.localPosition = closedLocalPosition;
                movingPart.localRotation = closedLocalRotation;
            }
            if (revealedRelic != null)
            {
                revealedRelic.linearVelocity = Vector3.zero;
                revealedRelic.angularVelocity = Vector3.zero;
                revealedRelic.Sleep();
            }
        }

        private IEnumerator RevealRoutine()
        {
            Vector3 startPosition = movingPart.localPosition;
            Quaternion startRotation = movingPart.localRotation;
            Quaternion targetRotation = Quaternion.Euler(openLocalEuler);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = t * t * (3f - 2f * t);
                movingPart.localPosition = Vector3.LerpUnclamped(startPosition, openLocalPosition, eased);
                movingPart.localRotation = Quaternion.SlerpUnclamped(startRotation, targetRotation, eased);
                yield return null;
            }

            movingPart.localPosition = openLocalPosition;
            movingPart.localRotation = targetRotation;
            IsCompleted = chamberInteraction.TryCompleteInteraction();
            revealRoutine = null;
        }

        private void CaptureClosedState()
        {
            if (movingPart == null)
            {
                return;
            }
            closedLocalPosition = movingPart.localPosition;
            closedLocalRotation = movingPart.localRotation;
        }

        private void HandleSelected(SelectEnterEventArgs args)
        {
            TryActivate();
        }
    }
}
