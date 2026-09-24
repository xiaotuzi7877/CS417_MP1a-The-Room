using System.Collections;
using UnityEngine;

namespace MichaelManor
{
    /// <summary>
    /// Opens the physical clue chest after the Watcher Lock and unlocks the five
    /// Gate affordances only after the obstruction has moved aside.
    /// </summary>
    public sealed class ManorClueChestReveal : MonoBehaviour
    {
        [SerializeField] private ManorThreeStagePuzzle ritual;
        [SerializeField] private FiveChamberQuestController chamberQuest;
        [SerializeField] private Transform lidHinge;
        [SerializeField] private GameObject contentsRoot;
        [SerializeField] private Vector3 closedLocalEulerAngles;
        [SerializeField] private Vector3 openLocalEulerAngles = new Vector3(-108f, 0f, 0f);
        [SerializeField] private float openDuration = 1.25f;

        private Coroutine revealRoutine;

        public bool IsOpen { get; private set; }
        public bool IsAnimating => revealRoutine != null;
        public float OpenDuration => openDuration;
        public Transform LidHinge => lidHinge;

        public void Configure(
            ManorThreeStagePuzzle threeStageRitual,
            FiveChamberQuestController fiveChamberQuest,
            Transform movingLid,
            GameObject revealedContents,
            Vector3 closedEulerAngles,
            Vector3 openEulerAngles,
            float duration)
        {
            ritual = threeStageRitual;
            chamberQuest = fiveChamberQuest;
            lidHinge = movingLid;
            contentsRoot = revealedContents;
            closedLocalEulerAngles = closedEulerAngles;
            openLocalEulerAngles = openEulerAngles;
            openDuration = Mathf.Max(0.1f, duration);
        }

        private void Awake()
        {
            ApplyClosedState();
        }

        private void OnEnable()
        {
            if (ritual != null)
            {
                ritual.StageCompleted += HandleStageCompleted;
                ritual.PuzzleReset += HandlePuzzleReset;
            }
        }

        private void OnDisable()
        {
            if (ritual != null)
            {
                ritual.StageCompleted -= HandleStageCompleted;
                ritual.PuzzleReset -= HandlePuzzleReset;
            }
        }

        private void HandleStageCompleted(int completedStageIndex)
        {
            if (completedStageIndex == 0)
            {
                OpenReveal();
            }
        }

        private void HandlePuzzleReset()
        {
            ResetReveal();
        }

        [ContextMenu("Open Clue Chest Reveal")]
        public void OpenReveal()
        {
            if (IsOpen || revealRoutine != null)
            {
                return;
            }

            revealRoutine = StartCoroutine(OpenRoutine());
        }

        [ContextMenu("Reset Clue Chest Reveal")]
        public void ResetReveal()
        {
            if (revealRoutine != null)
            {
                StopCoroutine(revealRoutine);
                revealRoutine = null;
            }

            ApplyClosedState();
            chamberQuest?.ResetQuest();
        }

        private IEnumerator OpenRoutine()
        {
            Quaternion startRotation = lidHinge != null
                ? lidHinge.localRotation
                : Quaternion.Euler(closedLocalEulerAngles);
            Quaternion targetRotation = Quaternion.Euler(openLocalEulerAngles);
            float elapsed = 0f;

            while (lidHinge != null && elapsed < openDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / openDuration));
                lidHinge.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);
                yield return null;
            }

            if (lidHinge != null)
            {
                lidHinge.localRotation = targetRotation;
            }

            IsOpen = true;
            revealRoutine = null;
            chamberQuest?.UnlockGates();
            Debug.Log("Watcher Lock opened the physical clue chest and activated five Gate runes.");
        }

        private void ApplyClosedState()
        {
            IsOpen = false;
            if (lidHinge != null)
            {
                lidHinge.localRotation = Quaternion.Euler(closedLocalEulerAngles);
            }

            if (contentsRoot != null)
            {
                contentsRoot.SetActive(true);
            }
        }
    }
}
