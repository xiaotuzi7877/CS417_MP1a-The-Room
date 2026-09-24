using System.Collections;
using TMPro;
using UnityEngine;

namespace MichaelManor
{
    public sealed class ManorMoonCryptPuzzle : MonoBehaviour
    {
        private static readonly int[] RequiredOrder = { 0, 1, 2 };

        [SerializeField] private ManorChamberInteraction chamberInteraction;
        [SerializeField] private ManorThreeStagePuzzle ritual;
        [SerializeField] private Renderer[] buttonRenderers;
        [SerializeField] private Light[] buttonLights;
        [SerializeField] private Transform moonstoneSlab;
        [SerializeField] private Vector3 moonstoneSlabOpenPosition;
        [SerializeField] private Vector3 moonstoneSlabOpenEuler;
        [SerializeField] private ManorKeyArtifact moonstone;
        [SerializeField] private GameObject moonstoneReturnRune;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private float revealDuration = 1.2f;

        private Vector3 slabClosedPosition;
        private Quaternion slabClosedRotation;
        private int sequencePosition;
        private Coroutine activeRoutine;

        public int SequencePosition => sequencePosition;
        public bool IsSolved { get; private set; }
        public ManorKeyArtifact Moonstone => moonstone;

        public void Configure(
            ManorChamberInteraction interaction,
            ManorThreeStagePuzzle ritualController,
            Renderer[] renderers,
            Light[] lights,
            Transform movingSlab,
            Vector3 slabOpenPosition,
            Vector3 slabOpenEuler,
            ManorKeyArtifact revealedMoonstone,
            GameObject returnRune,
            TMP_Text status)
        {
            chamberInteraction = interaction;
            ritual = ritualController;
            buttonRenderers = renderers;
            buttonLights = lights;
            moonstoneSlab = movingSlab;
            moonstoneSlabOpenPosition = slabOpenPosition;
            moonstoneSlabOpenEuler = slabOpenEuler;
            moonstone = revealedMoonstone;
            moonstoneReturnRune = returnRune;
            statusText = status;
        }

        private void Awake()
        {
            if (moonstoneSlab != null)
            {
                slabClosedPosition = moonstoneSlab.localPosition;
                slabClosedRotation = moonstoneSlab.localRotation;
            }
            ApplyResetState();
        }

        private void OnEnable()
        {
            if (chamberInteraction != null && chamberInteraction.QuestController != null)
                chamberInteraction.QuestController.QuestReset += ResetPuzzle;
            if (ritual != null)
            {
                ritual.StageCompleted += HandleRitualStageCompleted;
                ritual.PuzzleReset += HandleRitualReset;
            }
        }

        private void OnDisable()
        {
            if (chamberInteraction != null && chamberInteraction.QuestController != null)
                chamberInteraction.QuestController.QuestReset -= ResetPuzzle;
            if (ritual != null)
            {
                ritual.StageCompleted -= HandleRitualStageCompleted;
                ritual.PuzzleReset -= HandleRitualReset;
            }
        }

        public bool PressButton(int buttonIndex)
        {
            if (IsSolved || activeRoutine != null || buttonIndex < 0 || buttonIndex > 2)
                return false;

            if (buttonIndex != RequiredOrder[sequencePosition])
            {
                activeRoutine = StartCoroutine(WrongSequenceRoutine());
                return false;
            }

            SetButtonState(buttonIndex, new Color(0.30f, 0.72f, 1f), 5.5f);
            sequencePosition++;
            if (statusText != null) statusText.text = $"SEQUENCE  {sequencePosition} / 3";
            if (sequencePosition == RequiredOrder.Length)
            {
                IsSolved = true;
                activeRoutine = StartCoroutine(RevealMoonstoneRoutine());
            }
            return true;
        }

        public void ResetPuzzle()
        {
            if (activeRoutine != null) StopCoroutine(activeRoutine);
            activeRoutine = null;
            ApplyResetState();
        }

        private IEnumerator WrongSequenceRoutine()
        {
            sequencePosition = 0;
            if (statusText != null) statusText.text = "WRONG ORDER - BEGIN WITH WOLF";
            for (int i = 0; i < 3; i++) SetButtonState(i, new Color(1f, 0.08f, 0.08f), 7f);
            yield return new WaitForSeconds(0.55f);
            for (int i = 0; i < 3; i++) SetButtonState(i, new Color(0.42f, 0.12f, 0.52f), 1.8f);
            if (statusText != null) statusText.text = "SEQUENCE  0 / 3";
            activeRoutine = null;
        }

        private IEnumerator RevealMoonstoneRoutine()
        {
            Vector3 startPosition = moonstoneSlab.localPosition;
            Quaternion startRotation = moonstoneSlab.localRotation;
            Quaternion targetRotation = Quaternion.Euler(moonstoneSlabOpenEuler);
            float elapsed = 0f;
            while (elapsed < revealDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / revealDuration));
                moonstoneSlab.localPosition = Vector3.Lerp(startPosition, moonstoneSlabOpenPosition, t);
                moonstoneSlab.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);
                yield return null;
            }
            moonstoneSlab.localPosition = moonstoneSlabOpenPosition;
            moonstoneSlab.localRotation = targetRotation;
            if (moonstone != null) moonstone.gameObject.SetActive(true);
            chamberInteraction?.TryCompleteInteraction();
            if (statusText != null) statusText.text = "MOONSTONE REVEALED - TAKE IT TO THE CELESTIAL LOCK";
            activeRoutine = null;
        }

        private void HandleRitualStageCompleted(int stageIndex)
        {
            if (stageIndex >= 1 && !IsSolved)
                ForceSolvedState();
            if (statusText != null && stageIndex >= 1)
                statusText.text = stageIndex == 1
                    ? "MOONSTONE PLACED - TAKE THE BLOOD SIGIL TO THE EXIT LOCK"
                    : "THE RITUAL IS COMPLETE";
            if (stageIndex == 1 && moonstoneReturnRune != null)
                moonstoneReturnRune.SetActive(true);
        }

        // Presentation shortcuts can satisfy the Celestial Lock directly; keep the crypt consistent.
        private void ForceSolvedState()
        {
            if (activeRoutine != null) StopCoroutine(activeRoutine);
            activeRoutine = null;
            IsSolved = true;
            sequencePosition = RequiredOrder.Length;
            if (moonstoneSlab != null)
            {
                moonstoneSlab.localPosition = moonstoneSlabOpenPosition;
                moonstoneSlab.localRotation = Quaternion.Euler(moonstoneSlabOpenEuler);
            }
            for (int i = 0; i < 3; i++) SetButtonState(i, new Color(0.30f, 0.72f, 1f), 5.5f);
            chamberInteraction?.TryCompleteInteraction();
            if (statusText != null) statusText.text = "MOONSTONE PLACED - THE BLOOD SIGIL IS FREE";
        }

        private void HandleRitualReset()
        {
            ApplyResetState();
        }

        private void ApplyResetState()
        {
            sequencePosition = 0;
            IsSolved = false;
            chamberInteraction?.ResetInteraction();
            if (moonstoneSlab != null)
            {
                moonstoneSlab.localPosition = slabClosedPosition;
                moonstoneSlab.localRotation = slabClosedRotation;
            }
            if (moonstone != null) moonstone.gameObject.SetActive(false);
            if (moonstoneReturnRune != null) moonstoneReturnRune.SetActive(false);
            for (int i = 0; i < 3; i++) SetButtonState(i, new Color(0.42f, 0.12f, 0.52f), 1.8f);
            if (statusText != null) statusText.text = "SEQUENCE  0 / 3";
        }

        private void SetButtonState(int index, Color color, float intensity)
        {
            if (buttonRenderers != null && index >= 0 && index < buttonRenderers.Length && buttonRenderers[index] != null)
            {
                MaterialPropertyBlock block = new MaterialPropertyBlock();
                buttonRenderers[index].GetPropertyBlock(block);
                block.SetColor("_BaseColor", color);
                block.SetColor("_EmissionColor", color * 2f);
                buttonRenderers[index].SetPropertyBlock(block);
            }
            if (buttonLights != null && index >= 0 && index < buttonLights.Length && buttonLights[index] != null)
            {
                buttonLights[index].color = color;
                buttonLights[index].intensity = intensity;
            }
        }
    }
}
