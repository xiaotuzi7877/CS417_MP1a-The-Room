using UnityEngine;
using UnityEngine.Events;

namespace MichaelManor
{
    /// <summary>
    /// Reusable one-shot completion relay for content inside a gated location.
    /// Chamber-specific animations and props can subscribe to the completion event.
    /// </summary>
    public sealed class ManorChamberInteraction : MonoBehaviour
    {
        [SerializeField] private FiveChamberQuestController questController;
        [SerializeField, Range(0, FiveChamberQuestController.RequiredChamberCount - 1)]
        private int chamberIndex;
        [SerializeField] private GameObject feedbackRoot;
        [SerializeField] private UnityEvent onFirstCompleted = new UnityEvent();

        private bool completed;

        public int ChamberIndex => chamberIndex;
        public bool IsCompleted => completed;
        public FiveChamberQuestController QuestController => questController;
        public UnityEvent OnFirstCompleted => onFirstCompleted;

        public void Configure(
            FiveChamberQuestController controller,
            int index,
            GameObject completionFeedback = null)
        {
            questController = controller;
            chamberIndex = Mathf.Clamp(index, 0, FiveChamberQuestController.RequiredChamberCount - 1);
            feedbackRoot = completionFeedback;
        }

        private void Awake()
        {
            if (feedbackRoot != null)
            {
                feedbackRoot.SetActive(false);
            }
        }

        public bool TryCompleteInteraction()
        {
            if (completed || questController == null ||
                !questController.TryCompleteChamber(chamberIndex))
            {
                return false;
            }

            completed = true;
            if (feedbackRoot != null)
            {
                feedbackRoot.SetActive(true);
            }

            onFirstCompleted.Invoke();
            return true;
        }

        public void CompleteInteraction()
        {
            TryCompleteInteraction();
        }

        public void ResetInteraction()
        {
            completed = false;
            if (feedbackRoot != null)
            {
                feedbackRoot.SetActive(false);
            }
        }

        private void OnValidate()
        {
            chamberIndex = Mathf.Clamp(chamberIndex, 0, FiveChamberQuestController.RequiredChamberCount - 1);
        }
    }
}
