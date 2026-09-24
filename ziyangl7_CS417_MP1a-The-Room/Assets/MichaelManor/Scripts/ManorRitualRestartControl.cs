using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MichaelManor
{
    [RequireComponent(typeof(XRSimpleInteractable))]
    public sealed class ManorRitualRestartControl : MonoBehaviour
    {
        [SerializeField] private ManorThreeStagePuzzle ritual;
        [SerializeField] private FiveChamberQuestController quest;
        [SerializeField] private ManorGateTravelSystem travel;
        [SerializeField] private Transform hallAnchor;
        [SerializeField] private TMP_Text label;
        [SerializeField] private Transform progressRing;
        [SerializeField] private float holdDuration = 2f;

        private XRSimpleInteractable interactable;
        private Coroutine holdRoutine;
        private Vector3 ringFullScale;

        public float HoldDuration => holdDuration;
        public TMP_Text Label => label;
        public bool IsHolding => holdRoutine != null;

        public void Configure(ManorThreeStagePuzzle ritualController, FiveChamberQuestController questController,
            ManorGateTravelSystem travelSystem, Transform returnAnchor, TMP_Text display, Transform ring, float duration)
        {
            ritual = ritualController;
            quest = questController;
            travel = travelSystem;
            hallAnchor = returnAnchor;
            label = display;
            progressRing = ring;
            holdDuration = Mathf.Max(1.5f, duration);
            ringFullScale = ring != null ? ring.localScale : Vector3.one;
            SetIdleVisual();
        }

        private void Awake()
        {
            interactable = GetComponent<XRSimpleInteractable>();
            ringFullScale = progressRing != null ? progressRing.localScale : Vector3.one;
            SetIdleVisual();
        }

        private void OnEnable()
        {
            if (interactable == null) interactable = GetComponent<XRSimpleInteractable>();
            interactable.selectEntered.AddListener(BeginHold);
            interactable.selectExited.AddListener(CancelHold);
        }

        private void OnDisable()
        {
            if (interactable != null)
            {
                interactable.selectEntered.RemoveListener(BeginHold);
                interactable.selectExited.RemoveListener(CancelHold);
            }
            if (holdRoutine != null) StopCoroutine(holdRoutine);
            holdRoutine = null;
        }

        private void BeginHold(SelectEnterEventArgs args)
        {
            if (holdRoutine == null) holdRoutine = StartCoroutine(HoldRoutine());
        }

        private void CancelHold(SelectExitEventArgs args)
        {
            if (holdRoutine == null) return;
            StopCoroutine(holdRoutine);
            holdRoutine = null;
            SetIdleVisual();
        }

        private IEnumerator HoldRoutine()
        {
            float elapsed = 0f;
            while (elapsed < holdDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / holdDuration);
                if (label != null) label.text = $"KEEP HOLDING  {Mathf.CeilToInt(holdDuration - elapsed)}";
                if (progressRing != null)
                    progressRing.localScale = new Vector3(ringFullScale.x * progress, ringFullScale.y, ringFullScale.z);
                yield return null;
            }
            holdRoutine = null;
            RestartNow();
        }

        [ContextMenu("Restart Complete Manor Quest")]
        public void RestartNow()
        {
            ritual?.ResetPuzzle();
            quest?.ResetQuest();
            foreach (ManorMoonCryptPuzzle crypt in FindObjectsByType<ManorMoonCryptPuzzle>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                crypt.ResetPuzzle();
            if (travel != null && hallAnchor != null) travel.TravelTo(hallAnchor);
            if (label != null) label.text = "RITUAL RESTARTED";
            if (progressRing != null) progressRing.localScale = ringFullScale;
            StartCoroutine(ReturnToIdle());
        }

        public void RestartNowForTest() => RestartNow();

        private IEnumerator ReturnToIdle()
        {
            yield return new WaitForSecondsRealtime(1f);
            SetIdleVisual();
        }

        private void SetIdleVisual()
        {
            if (label != null) label.text = "HOLD 2 SEC TO RESTART";
            if (progressRing != null)
                progressRing.localScale = new Vector3(0.01f, ringFullScale.y, ringFullScale.z);
        }
    }
}
