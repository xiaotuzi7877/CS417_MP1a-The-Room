using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MichaelManor
{
    /// <summary>
    /// Desktop presentation controls for demonstrating the final MP1b interaction
    /// without needing to perform the entire VR grab sequence on stage.
    /// </summary>
    public sealed class ManorPresentationShortcuts : MonoBehaviour
    {
        [SerializeField] private ManorPuzzleSocket puzzle;
        [SerializeField] private WinCelebrationController celebration;

        public void Configure(ManorPuzzleSocket puzzleSocket, WinCelebrationController winCelebration)
        {
            puzzle = puzzleSocket;
            celebration = winCelebration;
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.kKey.wasPressedThisFrame)
            {
                StartCoroutine(PlayFullUnlockSequence());
            }

            if (keyboard.wKey.wasPressedThisFrame)
            {
                ReplayWinCelebration();
            }

            if (keyboard.rKey.wasPressedThisFrame)
            {
                ResetPresentation();
            }
        }

        private IEnumerator PlayFullUnlockSequence()
        {
            ResetPresentation();
            yield return null;
            if (puzzle != null)
            {
                puzzle.SolvePuzzle();
                Debug.Log("Presentation shortcut K: playing Silver Fang insertion and door unlock.");
            }
        }

        private void ReplayWinCelebration()
        {
            if (celebration == null)
            {
                return;
            }

            celebration.ResetCelebration();
            celebration.TriggerWin();
            Debug.Log("Presentation shortcut W: replaying Win Celebration.");
        }

        private void ResetPresentation()
        {
            if (puzzle != null)
            {
                puzzle.ResetPuzzle();
            }

            if (celebration != null)
            {
                celebration.ResetCelebration();
            }

            Debug.Log("Presentation shortcut R: reset puzzle and win effects.");
        }
    }
}
