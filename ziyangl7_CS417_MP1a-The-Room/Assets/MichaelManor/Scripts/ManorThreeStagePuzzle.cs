using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace MichaelManor
{
    /// <summary>
    /// Runs the three required lock-and-key events for Michael Manor in a strict sequence.
    /// Each solved lock reveals the next physical key and advances the in-world scoreboard.
    /// </summary>
    public sealed class ManorThreeStagePuzzle : MonoBehaviour
    {
        [SerializeField] private XRSocketInteractor[] sockets;
        [SerializeField] private string[] requiredArtifactIds;
        [SerializeField] private ManorKeyArtifact[] artifacts;
        [SerializeField] private Transform[] revealBarriers;
        [SerializeField] private Vector3[] solvedBarrierLocalPositions;
        [SerializeField] private Vector3[] solvedBarrierLocalEulerAngles;
        [SerializeField] private Transform[] secondaryEffects;
        [SerializeField] private Vector3[] solvedEffectLocalEulerAngles;
        [SerializeField] private Light[] statusLights;
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private TMP_Text instructionText;
        [SerializeField] private Transform exitDoor;
        [SerializeField] private Transform exitDoorSeal;
        [SerializeField] private WinCelebrationController celebration;
        [SerializeField] private float transitionDuration = 1.15f;
        [SerializeField] private float doorOpenHeight = 5.6f;
        [SerializeField] private float doorOpenDuration = 2.25f;

        private Vector3[] barrierStartPositions;
        private Quaternion[] barrierStartRotations;
        private Quaternion[] effectStartRotations;
        private Transform[] artifactStartParents;
        private Vector3[] artifactStartLocalPositions;
        private Quaternion[] artifactStartLocalRotations;
        private Vector3[] artifactStartLocalScales;
        private bool[] artifactStartKinematic;
        private Vector3 doorClosedLocalPosition;
        private Vector3 sealStartScale;
        private Quaternion sealStartRotation;
        private int currentStage;
        private bool isAnimating;
        private bool cached;

        public int CurrentStage => currentStage;
        public int StageCount => sockets != null ? sockets.Length : 0;
        public bool IsComplete => StageCount > 0 && currentStage >= StageCount;

        public void Configure(
            XRSocketInteractor[] puzzleSockets,
            string[] artifactIds,
            ManorKeyArtifact[] keyArtifacts,
            Transform[] movingBarriers,
            Vector3[] barrierSolvedPositions,
            Vector3[] barrierSolvedEulerAngles,
            Transform[] stageEffects,
            Vector3[] effectSolvedEulerAngles,
            Light[] lockStatusLights,
            TMP_Text scoreboard,
            TMP_Text instructions,
            Transform finalDoor,
            Transform finalDoorSeal,
            WinCelebrationController winCelebration)
        {
            sockets = puzzleSockets;
            requiredArtifactIds = artifactIds;
            artifacts = keyArtifacts;
            revealBarriers = movingBarriers;
            solvedBarrierLocalPositions = barrierSolvedPositions;
            solvedBarrierLocalEulerAngles = barrierSolvedEulerAngles;
            secondaryEffects = stageEffects;
            solvedEffectLocalEulerAngles = effectSolvedEulerAngles;
            statusLights = lockStatusLights;
            progressText = scoreboard;
            instructionText = instructions;
            exitDoor = finalDoor;
            exitDoorSeal = finalDoorSeal;
            celebration = winCelebration;
        }

        private void Awake()
        {
            CacheStartState();
            ApplyResetState();
        }

        private void Start()
        {
            // Socket Interactor initializes an internal coroutine in Awake. Keeping generated
            // sockets disabled in the saved scene and enabling here also works when Fast Enter
            // Play Mode skips a domain reload.
            if (sockets == null)
            {
                return;
            }

            foreach (XRSocketInteractor socket in sockets)
            {
                if (socket != null)
                {
                    socket.enabled = true;
                }
            }
        }

        private void OnEnable()
        {
            if (sockets == null)
            {
                return;
            }

            foreach (XRSocketInteractor socket in sockets)
            {
                if (socket != null)
                {
                    socket.selectEntered.AddListener(HandleSelectEntered);
                }
            }
        }

        private void OnDisable()
        {
            if (sockets == null)
            {
                return;
            }

            foreach (XRSocketInteractor socket in sockets)
            {
                if (socket != null)
                {
                    socket.selectEntered.RemoveListener(HandleSelectEntered);
                }
            }
        }

        private void HandleSelectEntered(SelectEnterEventArgs args)
        {
            XRSocketInteractor selectedSocket = args.interactorObject as XRSocketInteractor;
            int stageIndex = FindSocketIndex(selectedSocket);
            ManorKeyArtifact artifact =
                args.interactableObject.transform.GetComponentInParent<ManorKeyArtifact>();

            if (isAnimating || stageIndex != currentStage || artifact == null ||
                !IsRequiredArtifact(stageIndex, artifact))
            {
                StartCoroutine(RejectSelection(selectedSocket, args.interactableObject));
                return;
            }

            BeginStage(stageIndex, artifact);
        }

        private bool IsRequiredArtifact(int stageIndex, ManorKeyArtifact artifact)
        {
            return requiredArtifactIds != null &&
                   stageIndex >= 0 &&
                   stageIndex < requiredArtifactIds.Length &&
                   artifact.ArtifactId == requiredArtifactIds[stageIndex];
        }

        private int FindSocketIndex(XRSocketInteractor socket)
        {
            if (socket == null || sockets == null)
            {
                return -1;
            }

            for (int i = 0; i < sockets.Length; i++)
            {
                if (sockets[i] == socket)
                {
                    return i;
                }
            }

            return -1;
        }

        private void BeginStage(int stageIndex, ManorKeyArtifact artifact)
        {
            if (stageIndex != currentStage || isAnimating)
            {
                return;
            }

            StartCoroutine(CompleteStageRoutine(stageIndex, artifact));
        }

        private IEnumerator CompleteStageRoutine(int stageIndex, ManorKeyArtifact artifact)
        {
            isAnimating = true;
            XRGrabInteractable grab = artifact.GetComponent<XRGrabInteractable>();
            Rigidbody body = artifact.GetComponent<Rigidbody>();
            if (grab != null)
            {
                grab.enabled = false;
            }

            if (body != null)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.isKinematic = true;
            }

            SetStatusLight(stageIndex, new Color(1f, 0.62f, 0.16f), 7.5f);
            yield return AnimateStageState(stageIndex);
            SetStatusLight(stageIndex, new Color(0.28f, 1f, 0.58f), 6f);

            currentStage++;
            if (currentStage < StageCount)
            {
                yield return RevealArtifact(currentStage);
                ActivateCurrentStageLight();
                UpdateGuidance();
            }
            else
            {
                yield return OpenExitAndCelebrate();
            }

            isAnimating = false;
            Debug.Log($"Michael Manor ritual lock {stageIndex + 1}/{StageCount} solved.");
        }

        private IEnumerator AnimateStageState(int stageIndex)
        {
            Transform barrier = GetAt(revealBarriers, stageIndex);
            Transform effect = GetAt(secondaryEffects, stageIndex);
            Vector3 barrierStartPosition = barrier != null
                ? barrier.localPosition
                : Vector3.zero;
            Quaternion barrierStartRotation = barrier != null
                ? barrier.localRotation
                : Quaternion.identity;
            Quaternion effectStartRotation = effect != null
                ? effect.localRotation
                : Quaternion.identity;
            Vector3 barrierTargetPosition = GetAt(solvedBarrierLocalPositions, stageIndex, barrierStartPosition);
            Quaternion barrierTargetRotation = Quaternion.Euler(
                GetAt(solvedBarrierLocalEulerAngles, stageIndex, barrierStartRotation.eulerAngles));
            Quaternion effectTargetRotation = Quaternion.Euler(
                GetAt(solvedEffectLocalEulerAngles, stageIndex, effectStartRotation.eulerAngles));

            float elapsed = 0f;
            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / transitionDuration));
                if (barrier != null)
                {
                    barrier.localPosition = Vector3.Lerp(barrierStartPosition, barrierTargetPosition, t);
                    barrier.localRotation = Quaternion.Slerp(barrierStartRotation, barrierTargetRotation, t);
                }

                if (effect != null)
                {
                    effect.localRotation = Quaternion.Slerp(effectStartRotation, effectTargetRotation, t);
                }

                yield return null;
            }

            if (barrier != null)
            {
                barrier.localPosition = barrierTargetPosition;
                barrier.localRotation = barrierTargetRotation;
            }

            if (effect != null)
            {
                effect.localRotation = effectTargetRotation;
            }
        }

        private IEnumerator RevealArtifact(int artifactIndex)
        {
            ManorKeyArtifact artifact = GetAt(artifacts, artifactIndex);
            if (artifact == null)
            {
                yield break;
            }

            artifact.gameObject.SetActive(true);
            Rigidbody body = artifact.GetComponent<Rigidbody>();
            if (body != null)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.isKinematic = true;
            }

            Vector3 targetScale = artifactStartLocalScales[artifactIndex];
            artifact.transform.localScale = targetScale * 0.08f;
            float elapsed = 0f;
            while (elapsed < 0.7f)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / 0.7f));
                artifact.transform.localScale = Vector3.Lerp(targetScale * 0.08f, targetScale, t);
                yield return null;
            }

            artifact.transform.localScale = targetScale;
            if (body != null)
            {
                body.isKinematic = artifactStartKinematic[artifactIndex];
            }
        }

        private IEnumerator OpenExitAndCelebrate()
        {
            if (instructionText != null)
            {
                instructionText.text = "THE RITUAL IS COMPLETE - THE EXIT IS OPEN";
            }

            if (progressText != null)
            {
                progressText.text = "RITUAL PROGRESS  3 / 3\nESCAPE UNLOCKED\nRITUAL CLUES FOUND  3 / 3";
            }

            float elapsed = 0f;
            while (exitDoorSeal != null && elapsed < 0.75f)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / 0.75f));
                exitDoorSeal.localScale = Vector3.Lerp(sealStartScale, sealStartScale * 0.05f, t);
                exitDoorSeal.localRotation = sealStartRotation * Quaternion.Euler(0f, 0f, 180f * t);
                yield return null;
            }

            if (exitDoorSeal != null)
            {
                exitDoorSeal.gameObject.SetActive(false);
            }

            Vector3 openPosition = doorClosedLocalPosition + Vector3.up * doorOpenHeight;
            elapsed = 0f;
            while (exitDoor != null && elapsed < doorOpenDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / doorOpenDuration));
                exitDoor.localPosition = Vector3.Lerp(doorClosedLocalPosition, openPosition, t);
                yield return null;
            }

            if (exitDoor != null)
            {
                exitDoor.localPosition = openPosition;
            }

            celebration?.TriggerWin();
        }

        private IEnumerator RejectSelection(
            XRSocketInteractor socket,
            IXRSelectInteractable interactable)
        {
            int stageIndex = FindSocketIndex(socket);
            Color previousColor = Color.red;
            float previousIntensity = 0f;
            Light light = GetAt(statusLights, stageIndex);
            if (light != null)
            {
                previousColor = light.color;
                previousIntensity = light.intensity;
                light.color = new Color(1f, 0.12f, 0.12f);
                light.intensity = 8f;
            }

            yield return null;
            if (socket != null && interactable != null && socket.interactionManager != null)
            {
                socket.interactionManager.SelectExit(
                    (IXRSelectInteractor)socket,
                    interactable);
            }

            yield return new WaitForSeconds(0.45f);
            if (light != null)
            {
                light.color = previousColor;
                light.intensity = previousIntensity;
            }
        }

        [ContextMenu("Solve All For Presentation")]
        public void SolveAllForPresentation()
        {
            StopAllCoroutines();
            CacheStartState();
            ApplyResetState();
            StartCoroutine(SolveAllPresentationRoutine());
        }

        private IEnumerator SolveAllPresentationRoutine()
        {
            yield return null;
            for (int i = 0; i < StageCount; i++)
            {
                ManorKeyArtifact artifact = GetAt(artifacts, i);
                XRSocketInteractor socket = GetAt(sockets, i);
                if (artifact == null || socket == null)
                {
                    continue;
                }

                artifact.gameObject.SetActive(true);
                Transform anchor = socket.attachTransform != null ? socket.attachTransform : socket.transform;
                artifact.transform.SetPositionAndRotation(anchor.position, anchor.rotation);
                BeginStage(i, artifact);
                yield return null;
                while (isAnimating)
                {
                    yield return null;
                }

                yield return new WaitForSeconds(0.45f);
            }
        }

        [ContextMenu("Reset Three Stage Puzzle")]
        public void ResetPuzzle()
        {
            StopAllCoroutines();
            CacheStartState();
            ApplyResetState();
            Debug.Log("Michael Manor three-stage ritual reset.");
        }

        private void CacheStartState()
        {
            if (cached)
            {
                return;
            }

            int count = StageCount;
            barrierStartPositions = new Vector3[count];
            barrierStartRotations = new Quaternion[count];
            effectStartRotations = new Quaternion[count];
            artifactStartParents = new Transform[count];
            artifactStartLocalPositions = new Vector3[count];
            artifactStartLocalRotations = new Quaternion[count];
            artifactStartLocalScales = new Vector3[count];
            artifactStartKinematic = new bool[count];

            for (int i = 0; i < count; i++)
            {
                Transform barrier = GetAt(revealBarriers, i);
                if (barrier != null)
                {
                    barrierStartPositions[i] = barrier.localPosition;
                    barrierStartRotations[i] = barrier.localRotation;
                }

                Transform effect = GetAt(secondaryEffects, i);
                if (effect != null)
                {
                    effectStartRotations[i] = effect.localRotation;
                }

                ManorKeyArtifact artifact = GetAt(artifacts, i);
                if (artifact != null)
                {
                    Transform artifactTransform = artifact.transform;
                    artifactStartParents[i] = artifactTransform.parent;
                    artifactStartLocalPositions[i] = artifactTransform.localPosition;
                    artifactStartLocalRotations[i] = artifactTransform.localRotation;
                    artifactStartLocalScales[i] = artifactTransform.localScale;
                    Rigidbody body = artifact.GetComponent<Rigidbody>();
                    artifactStartKinematic[i] = body != null && body.isKinematic;
                }
            }

            if (exitDoor != null)
            {
                doorClosedLocalPosition = exitDoor.localPosition;
            }

            if (exitDoorSeal != null)
            {
                sealStartScale = exitDoorSeal.localScale;
                sealStartRotation = exitDoorSeal.localRotation;
            }

            cached = true;
        }

        private void ApplyResetState()
        {
            currentStage = 0;
            isAnimating = false;

            for (int i = 0; i < StageCount; i++)
            {
                XRSocketInteractor socket = GetAt(sockets, i);
                ManorKeyArtifact artifact = GetAt(artifacts, i);
                XRGrabInteractable grab = artifact != null
                    ? artifact.GetComponent<XRGrabInteractable>()
                    : null;

                if (socket != null && socket.hasSelection && socket.interactionManager != null && grab != null)
                {
                    socket.interactionManager.SelectExit(
                        (IXRSelectInteractor)socket,
                        (IXRSelectInteractable)grab);
                }

                if (artifact != null)
                {
                    artifact.transform.SetParent(artifactStartParents[i], false);
                    artifact.transform.localPosition = artifactStartLocalPositions[i];
                    artifact.transform.localRotation = artifactStartLocalRotations[i];
                    artifact.transform.localScale = artifactStartLocalScales[i];
                    artifact.gameObject.SetActive(i == 0);
                    if (grab != null)
                    {
                        grab.enabled = true;
                    }

                    Rigidbody body = artifact.GetComponent<Rigidbody>();
                    if (body != null)
                    {
                        body.linearVelocity = Vector3.zero;
                        body.angularVelocity = Vector3.zero;
                        body.isKinematic = artifactStartKinematic[i];
                    }
                }

                Transform barrier = GetAt(revealBarriers, i);
                if (barrier != null)
                {
                    barrier.localPosition = barrierStartPositions[i];
                    barrier.localRotation = barrierStartRotations[i];
                }

                Transform effect = GetAt(secondaryEffects, i);
                if (effect != null)
                {
                    effect.localRotation = effectStartRotations[i];
                }

                SetStatusLight(i, new Color(0.38f, 0.16f, 0.18f), 1.4f);
            }

            if (exitDoor != null)
            {
                exitDoor.localPosition = doorClosedLocalPosition;
            }

            if (exitDoorSeal != null)
            {
                exitDoorSeal.gameObject.SetActive(true);
                exitDoorSeal.localScale = sealStartScale;
                exitDoorSeal.localRotation = sealStartRotation;
            }

            celebration?.ResetCelebration();
            ActivateCurrentStageLight();
            UpdateGuidance();
        }

        private void ActivateCurrentStageLight()
        {
            if (currentStage < StageCount)
            {
                SetStatusLight(currentStage, new Color(0.32f, 0.72f, 1f), 5.2f);
            }
        }

        private void UpdateGuidance()
        {
            if (progressText != null)
            {
                progressText.text = $"RITUAL PROGRESS  {currentStage} / {StageCount}\n" +
                                    $"LOCKS REMAINING  {Mathf.Max(0, StageCount - currentStage)}\n" +
                                    $"RITUAL CLUES FOUND  {currentStage} / {StageCount}";
            }

            if (instructionText == null)
            {
                return;
            }

            string[] clues =
            {
                "I  -  THE SILVER FANG SILENCES THE WATCHER",
                "II -  RETURN THE MOONSTONE TO THE HEAVENS",
                "III - FEED THE BLOOD SIGIL TO THE EXIT SEAL"
            };
            instructionText.text = currentStage < clues.Length
                ? clues[currentStage]
                : "THE RITUAL IS COMPLETE";
        }

        private void SetStatusLight(int index, Color color, float intensity)
        {
            Light light = GetAt(statusLights, index);
            if (light != null)
            {
                light.color = color;
                light.intensity = intensity;
            }
        }

        private static T GetAt<T>(T[] values, int index) where T : class
        {
            return values != null && index >= 0 && index < values.Length ? values[index] : null;
        }

        private static Vector3 GetAt(Vector3[] values, int index, Vector3 fallback)
        {
            return values != null && index >= 0 && index < values.Length ? values[index] : fallback;
        }
    }
}
