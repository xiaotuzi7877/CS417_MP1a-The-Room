using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MichaelManor;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MichaelManorEditor
{
    /// Play Mode acceptance test for Section 6. Prints one summary line: MOON CRYPT TEST PASS/FAIL.
    public static class ManorMoonCryptSelfTest
    {
        private static int errors;
        private static readonly List<string> failures = new List<string>();

        [MenuItem("Tools/Michael Manor/Run Moon Crypt Self Test (Play Mode)")]
        public static async void Run()
        {
            if (!EditorApplication.isPlaying) { Debug.LogWarning("Enter Play Mode first, then run the Moon Crypt self test."); return; }
            errors = 0; failures.Clear();
            Application.logMessageReceived += Count;
            try { await Execute(); }
            catch (System.Exception e) { failures.Add("exception: " + e.Message); }
            finally { Application.logMessageReceived -= Count; }
            string result = failures.Count == 0 && errors == 0 ? "PASS" : "FAIL";
            Debug.Log($"MOON CRYPT TEST {result} | console errors during test: {errors} | failures: {(failures.Count == 0 ? "none" : string.Join("; ", failures))}");
        }

        private static async Task Execute()
        {
            var ritual = Object.FindFirstObjectByType<ManorThreeStagePuzzle>(FindObjectsInactive.Include);
            var puzzle = Object.FindFirstObjectByType<ManorMoonCryptPuzzle>(FindObjectsInactive.Include);
            var artifacts = Object.FindObjectsByType<ManorKeyArtifact>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var moon = artifacts.FirstOrDefault(a => a.ArtifactId == "Moonstone");
            var blood = artifacts.FirstOrDefault(a => a.ArtifactId == "BloodSigil");
            var buttons = Object.FindObjectsByType<ManorMoonSequenceButton>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .OrderBy(b => b.SequenceIndex).ToArray();
            Transform crypt = GameObject.Find("Chamber_05_MoonCrypt")?.transform;
            Transform moonSlab = crypt?.Find("Content_Section06/MoonstoneVault/MoonstoneSlab_Moving");
            Transform bloodSlab = crypt?.Find("Content_Section06/BloodSigilVault/BloodSlab_Moving");
            var rune = crypt?.Find("TravelShell/ReturnRune")?.GetComponent<ManorReturnRune>();
            if (!Check(ritual && puzzle && moon && blood && buttons.Length == 3 && moonSlab && bloodSlab && rune, "scene objects missing (run Install Moon Crypt Puzzle?)")) return;

            Check(Mathf.Approximately(moon.GetComponent<Rigidbody>().mass, 0.10f), "Moonstone mass != 0.10");
            Check(Mathf.Approximately(blood.GetComponent<Rigidbody>().mass, 2.00f), "Blood Sigil mass != 2.00");
            Check(moon.GetComponent<XRGrabInteractable>() && blood.GetComponent<XRGrabInteractable>(), "key not grabbable");
            Check(artifacts.Count(a => a.ArtifactId == "Moonstone") == 1, "more than one object claims Moonstone id");
            Check(moon.transform.IsChildOf(crypt), "Moonstone is not inside the Moon Crypt");

            ritual.ResetPuzzle();
            await Wait(0.2f);
            Vector3 moonClosed = moonSlab.localPosition, bloodClosed = bloodSlab.localPosition;
            Check(!moon.gameObject.activeInHierarchy, "Moonstone visible before puzzle");

            ritual.SolveCurrentStageForPresentation();              // Watcher Lock
            await Until(() => ritual.CurrentStage == 1, 6f);
            await Wait(0.5f);
            Check(ritual.CurrentStage == 1, "Watcher Lock did not complete");
            Check(!moon.gameObject.activeInHierarchy, "Watcher Lock still auto-reveals Moonstone");

            buttons[1].Press(); await Wait(0.8f);                   // wrong: MOON first
            Check(puzzle.SequencePosition == 0 && !moon.gameObject.activeInHierarchy, "wrong first press did not reset");
            buttons[0].Press(); buttons[2].Press(); await Wait(0.8f); // wrong: WOLF, BLOOD
            Check(puzzle.SequencePosition == 0 && !moon.gameObject.activeInHierarchy, "wrong second press did not reset");
            Check(Vector3.Distance(moonSlab.localPosition, moonClosed) < 0.01f, "slab moved on wrong sequence");

            buttons[0].Press(); buttons[1].Press(); buttons[2].Press(); // WOLF MOON BLOOD
            await Wait(1.8f);
            Check(puzzle.IsSolved && moon.gameObject.activeInHierarchy, "correct sequence did not release Moonstone");
            Check(Vector3.Distance(moonSlab.localPosition, moonClosed) > 1f, "Moonstone slab did not move aside");
            Check(!buttons[0].Press(), "puzzle accepted input after solve (not one-shot)");
            Check(!rune.gameObject.activeSelf, "Return Rune active before Celestial Lock");

            ritual.SolveCurrentStageForPresentation();              // Celestial Lock
            await Until(() => ritual.CurrentStage == 2, 6f);
            await Wait(1.2f);
            Check(ritual.CurrentStage == 2, "Celestial Lock did not complete");
            Check(ritual.ProgressText == null || ritual.ProgressText.text.Contains("2 / 3"), "progress text not 2 / 3");
            Check(blood.gameObject.activeInHierarchy, "Blood Sigil not revealed");
            Check(Vector3.Distance(bloodSlab.localPosition, bloodClosed) > 1f, "blood slab did not move");
            Check(rune.gameObject.activeSelf, "Return Rune not active after Celestial Lock");
            Vector3 bloodHome = blood.transform.parent.position;
            await Wait(3f);
            Check(ritual.CurrentStage == 2, "Exit Lock completed without the player placing the Blood Sigil");
            Check(Vector3.Distance(blood.transform.position, bloodHome) < 1.5f, "Blood Sigil left its Moon Crypt plinth after reveal");
            Check(rune.TryReturn(), "Return Rune TryReturn failed");

            // Regression: after the Blood Sigil has sat in the Exit Lock once, a reset and replay
            // must not let that socket snap the freshly revealed Sigil from the Moon Crypt.
            ritual.SolveCurrentStageForPresentation();
            await Until(() => ritual.IsComplete, 8f);
            Check(ritual.IsComplete, "Exit Lock did not complete in the regression setup");
            ritual.ResetPuzzle();
            await Wait(2f);
            Check(ritual.CurrentStage == 0, "reset re-inserted a Key Prop into its Lock (stage advanced by itself)");
            ritual.SolveCurrentStageForPresentation();
            await Until(() => ritual.CurrentStage == 1, 6f);
            buttons[0].Press(); buttons[1].Press(); buttons[2].Press();
            await Wait(1.8f);
            ritual.SolveCurrentStageForPresentation();
            await Until(() => ritual.CurrentStage == 2, 6f);
            await Wait(3f);
            Check(ritual.CurrentStage == 2, "replay: Exit Lock snapped the Blood Sigil remotely");
            Check(Vector3.Distance(blood.transform.position, bloodHome) < 1.5f, "replay: Blood Sigil left the Moon Crypt");
            ritual.ResetPuzzle();
        }

        private static bool Check(bool ok, string message) { if (!ok) failures.Add(message); return ok; }
        private static void Count(string c, string s, LogType t) { if (t == LogType.Error || t == LogType.Exception) errors++; }
        private static Task Wait(float seconds) => Task.Delay((int)(seconds * 1000));
        private static async Task Until(System.Func<bool> condition, float timeout)
        {
            float end = Time.realtimeSinceStartup + timeout;
            while (!condition() && Time.realtimeSinceStartup < end) await Task.Delay(50);
        }
    }
}
