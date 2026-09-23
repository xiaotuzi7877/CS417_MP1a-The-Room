using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
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
        [SerializeField] private Transform insertionAnchor;
        [SerializeField] private Transform doorSeal;
        [SerializeField] private Renderer[] runeRenderers;
        [SerializeField] private AudioSource unlockAudio;
        [SerializeField] private float insertionDuration = 0.8f;
        [SerializeField] private float sealReleaseDuration = 0.65f;
        [SerializeField] private UnityEvent onSolved = new UnityEvent();

        private Vector3 doorClosedLocalPosition;
        private Vector3 insertionTargetLocalPosition;
        private Vector3 doorSealStartScale;
        private Quaternion doorSealStartRotation;
        private Transform acceptedArtifact;
        private bool solved;
        private Coroutine feedbackRoutine;

        public bool IsSolved => solved;
        public UnityEvent OnSolved => onSolved;

        public void Configure(
            XRSocketInteractor puzzleSocket,
            string artifactId,
            Transform puzzleDoor,
            GameObject solvedFeedback,
            Light statusLight,
            Transform artifactInsertionAnchor = null,
            Transform exitDoorSeal = null,
            Renderer[] pedestalRunes = null,
            AudioSource audioSource = null)
        {
            socket = puzzleSocket;
            requiredArtifactId = artifactId;
            door = puzzleDoor;
            successFeedback = solvedFeedback;
            feedbackLight = statusLight;
            insertionAnchor = artifactInsertionAnchor;
            doorSeal = exitDoorSeal;
            runeRenderers = pedestalRunes;
            unlockAudio = audioSource;
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

            if (insertionAnchor != null)
            {
                insertionTargetLocalPosition = insertionAnchor.localPosition;
            }

            if (doorSeal != null)
            {
                doorSealStartScale = doorSeal.localScale;
                doorSealStartRotation = doorSeal.localRotation;
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
                acceptedArtifact = artifact.transform;
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
            if (acceptedArtifact == null)
            {
                ManorKeyArtifact[] artifacts = FindObjectsByType<ManorKeyArtifact>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None);
                foreach (ManorKeyArtifact artifact in artifacts)
                {
                    if (artifact.ArtifactId == requiredArtifactId)
                    {
                        acceptedArtifact = artifact.transform;
                        break;
                    }
                }
            }

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

            yield return AnimateArtifactInsertion();

            if (unlockAudio != null)
            {
                unlockAudio.Play();
            }

            yield return ReleaseDoorSeal();

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

        private IEnumerator AnimateArtifactInsertion()
        {
            if (insertionAnchor == null)
            {
                yield return new WaitForSeconds(0.25f);
                yield break;
            }

            insertionAnchor.localPosition = insertionTargetLocalPosition + Vector3.up * 0.34f;
            Vector3 anchorStart = insertionAnchor.localPosition;
            bool socketOwnsArtifact = socket != null && socket.hasSelection;
            Vector3 artifactStartPosition = acceptedArtifact != null ? acceptedArtifact.position : Vector3.zero;
            Quaternion artifactStartRotation = acceptedArtifact != null ? acceptedArtifact.rotation : Quaternion.identity;

            Rigidbody artifactBody = acceptedArtifact != null ? acceptedArtifact.GetComponent<Rigidbody>() : null;
            XRGrabInteractable artifactGrab = acceptedArtifact != null
                ? acceptedArtifact.GetComponent<XRGrabInteractable>()
                : null;
            if (!socketOwnsArtifact && artifactBody != null)
            {
                artifactBody.linearVelocity = Vector3.zero;
                artifactBody.angularVelocity = Vector3.zero;
                artifactBody.isKinematic = true;
            }

            float elapsed = 0f;
            while (elapsed < insertionDuration)
            {
                elapsed += Time.deltaTime;
                float normalized = Mathf.Clamp01(elapsed / insertionDuration);
                float t = normalized * normalized * (3f - 2f * normalized);
                insertionAnchor.localPosition = Vector3.Lerp(anchorStart, insertionTargetLocalPosition, t);

                if (!socketOwnsArtifact && acceptedArtifact != null)
                {
                    acceptedArtifact.position = Vector3.Lerp(artifactStartPosition, insertionAnchor.position, t);
                    acceptedArtifact.rotation = Quaternion.Slerp(artifactStartRotation, insertionAnchor.rotation, t);
                }

                PulseRunes(normalized);
                yield return null;
            }

            insertionAnchor.localPosition = insertionTargetLocalPosition;
            if (acceptedArtifact != null)
            {
                acceptedArtifact.SetParent(insertionAnchor, true);
                acceptedArtifact.localPosition = Vector3.zero;
                acceptedArtifact.localRotation = Quaternion.identity;
                if (artifactBody != null)
                {
                    artifactBody.isKinematic = true;
                }

                if (artifactGrab != null)
                {
                    artifactGrab.enabled = false;
                }
            }

            SetRuneScale(1f);
        }

        private IEnumerator ReleaseDoorSeal()
        {
            if (doorSeal == null)
            {
                yield return new WaitForSeconds(0.2f);
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < sealReleaseDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / sealReleaseDuration);
                doorSeal.localScale = Vector3.Lerp(doorSealStartScale, doorSealStartScale * 0.08f, t);
                doorSeal.localRotation = doorSealStartRotation * Quaternion.Euler(0f, 0f, 180f * t);
                yield return null;
            }

            doorSeal.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.2f);
        }

        private void PulseRunes(float normalized)
        {
            float pulse = 0.92f + Mathf.Sin(normalized * Mathf.PI * 8f) * 0.16f + normalized * 0.18f;
            SetRuneScale(pulse);
            if (feedbackLight != null)
            {
                feedbackLight.intensity = Mathf.Lerp(250f, 750f, normalized) * (0.9f + 0.1f * pulse);
            }
        }

        private void SetRuneScale(float scale)
        {
            if (runeRenderers == null)
            {
                return;
            }

            foreach (Renderer rune in runeRenderers)
            {
                if (rune != null)
                {
                    rune.transform.localScale = Vector3.one * (0.12f * scale);
                }
            }
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
