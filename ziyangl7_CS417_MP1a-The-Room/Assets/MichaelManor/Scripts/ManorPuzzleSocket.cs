using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace MichaelManor
{
    /// <summary>
    /// Validates the artifact placed in an XR socket and opens the manor door.
    /// </summary>
    public sealed class ManorPuzzleSocket : MonoBehaviour
    {
        [SerializeField] private XRSocketInteractor socket;
        [SerializeField] private string requiredArtifactId = "SilverFang";
        [SerializeField] private Transform door;
        [SerializeField] private float doorOpenHeight = 5.6f;
        [SerializeField] private float doorOpenDuration = 2.25f;
        [SerializeField] private GameObject successFeedback;
        [SerializeField] private Light feedbackLight;
        [SerializeField] private UnityEvent onSolved = new UnityEvent();

        private Vector3 doorClosedLocalPosition;
        private bool solved;
        private Coroutine feedbackRoutine;

        public bool IsSolved => solved;
        public UnityEvent OnSolved => onSolved;

        public void Configure(
            XRSocketInteractor puzzleSocket,
            string artifactId,
            Transform puzzleDoor,
            GameObject solvedFeedback,
            Light statusLight)
        {
            socket = puzzleSocket;
            requiredArtifactId = artifactId;
            door = puzzleDoor;
            successFeedback = solvedFeedback;
            feedbackLight = statusLight;
        }

        private void Awake()
        {
            if (door != null)
            {
                doorClosedLocalPosition = door.localPosition;
            }

            if (successFeedback != null)
            {
                successFeedback.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (socket != null)
            {
                socket.selectEntered.AddListener(HandleSelectEntered);
            }
        }

        private void OnDisable()
        {
            if (socket != null)
            {
                socket.selectEntered.RemoveListener(HandleSelectEntered);
            }
        }

        private void HandleSelectEntered(SelectEnterEventArgs args)
        {
            if (solved)
            {
                return;
            }

            ManorKeyArtifact artifact =
                args.interactableObject.transform.GetComponentInParent<ManorKeyArtifact>();

            if (artifact != null && artifact.ArtifactId == requiredArtifactId)
            {
                SolvePuzzle();
            }
            else
            {
                if (feedbackRoutine != null)
                {
                    StopCoroutine(feedbackRoutine);
                }

                feedbackRoutine = StartCoroutine(FlashFeedback(Color.red, 0.9f));
                Debug.Log("The pedestal rejected the artifact.");
            }
        }

        [ContextMenu("Solve Puzzle")]
        public void SolvePuzzle()
        {
            if (solved)
            {
                return;
            }

            solved = true;
            if (feedbackRoutine != null)
            {
                StopCoroutine(feedbackRoutine);
            }

            feedbackRoutine = StartCoroutine(SolveRoutine());
        }

        private IEnumerator SolveRoutine()
        {
            if (successFeedback != null)
            {
                successFeedback.SetActive(true);
            }

            if (feedbackLight != null)
            {
                feedbackLight.color = new Color(0.35f, 0.75f, 1f);
                feedbackLight.intensity = 650f;
            }

            Vector3 openPosition = doorClosedLocalPosition + Vector3.up * doorOpenHeight;
            float elapsed = 0f;

            while (door != null && elapsed < doorOpenDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / doorOpenDuration);
                door.localPosition = Vector3.Lerp(doorClosedLocalPosition, openPosition, t);
                yield return null;
            }

            if (door != null)
            {
                door.localPosition = openPosition;
            }

            onSolved.Invoke();
            Debug.Log("The Silver Fang unlocked the manor exit.");
        }

        private IEnumerator FlashFeedback(Color color, float duration)
        {
            if (feedbackLight == null)
            {
                yield break;
            }

            Color originalColor = feedbackLight.color;
            float originalIntensity = feedbackLight.intensity;
            feedbackLight.color = color;
            feedbackLight.intensity = 500f;
            yield return new WaitForSeconds(duration);
            feedbackLight.color = originalColor;
            feedbackLight.intensity = originalIntensity;
        }
    }
}
