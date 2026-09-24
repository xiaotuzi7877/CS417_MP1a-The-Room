using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MichaelManor;
using UnityEditor;
using UnityEngine;

namespace MichaelManorEditor
{
    /// Play Mode acceptance test for Section 7. Prints one summary line: QUEST PROGRESS TEST PASS/FAIL.
    public static class ManorQuestProgressSelfTest
    {
        private static int errors;
        private static readonly List<string> failures = new List<string>();
        private static ManorThreeStagePuzzle ritual;
        private static FiveChamberQuestController quest;
        private static ManorQuestScoreboard board;
        private static ManorGatePortal[] gates;

        [MenuItem("Tools/Michael Manor/Run Quest Progress Self Test (Play Mode)")]
        public static async void Run()
        {
            if (!EditorApplication.isPlaying) { Debug.LogWarning("Enter Play Mode first, then run the quest progress self test."); return; }
            errors = 0; failures.Clear();
            Application.logMessageReceived += Count;
            try { await Execute(); }
            catch (System.Exception e) { failures.Add("exception: " + e.Message); }
            finally { Application.logMessageReceived -= Count; }
            string result = failures.Count == 0 && errors == 0 ? "PASS" : "FAIL";
            Debug.Log($"QUEST PROGRESS TEST {result} | console errors during test: {errors} | failures: {(failures.Count == 0 ? "none" : string.Join("; ", failures))}");
        }

        private static async Task Execute()
        {
            ritual = Object.FindFirstObjectByType<ManorThreeStagePuzzle>();
            quest = Object.FindFirstObjectByType<FiveChamberQuestController>();
            board = Object.FindFirstObjectByType<ManorQuestScoreboard>();
            var puzzle = Object.FindFirstObjectByType<ManorMoonCryptPuzzle>();
            gates = Object.FindObjectsByType<ManorGatePortal>(FindObjectsSortMode.None).OrderBy(g => g.ChamberIndex).ToArray();
            var reveals = Object.FindObjectsByType<ManorChamberReveal>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var runes = Object.FindObjectsByType<ManorReturnRune>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var buttons = Object.FindObjectsByType<ManorMoonSequenceButton>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .OrderBy(b => b.SequenceIndex).ToArray();
            if (!Check(ritual && quest && board && puzzle && gates.Length == 5 && reveals.Length == 4 && buttons.Length == 3,
                    "scene objects missing (run Install Quest Progress Board?)")) return;
            Check(gates.All(g => g.ExploredLabel != null), "a Gate has no EXPLORED label");

            ritual.ResetPuzzle();
            await Wait(1f);
            Expect(0, 0, "SILVER FANG", "after reset");
            Check(gates.All(g => !g.IsUnlocked && RuneColor(g) == Locked), "Gates not dark/locked after reset");

            ritual.SolveCurrentStageForPresentation();
            await Until(() => ritual.CurrentStage == 1, 6f);
            await Until(() => gates.All(g => g.IsUnlocked), 5f);   // unlock follows the chest lid
            await Wait(0.3f);
            Expect(1, 0, "CODEX", "after Watcher Lock");
            Check(gates.All(g => g.IsUnlocked), "Gates not unlocked after Watcher Lock");
            Check(gates.Where(g => !g.IsMoonCryptGate).All(g => RuneColor(g) == Purple), "false Gate runes not purple");
            Check(RuneColor(gates[4]) == BlueWhite, "Moon Gate rune not blue-white");

            for (int i = 0; i < 4; i++)
            {
                Check(gates[i].TryRequestActivation(), $"Gate {i + 1} travel failed");
                await Wait(0.3f);
                Check(quest.ExploredCount == i, $"arrival in chamber {i + 1} changed the explored count");
                Expect(1, i, "MOON-MARKED", $"after arriving in chamber {i + 1}");
                ManorChamberReveal reveal = reveals.First(r => RevealIndex(r) == i);
                Check(reveal.TryActivate(), $"chamber {i + 1} interaction failed");
                await Until(() => quest.ExploredCount == i + 1, 5f);
                await Wait(0.2f);
                Expect(1, i + 1, "MOON-MARKED", $"after chamber {i + 1} interaction");
                Check(gates[i].ExploredLabel.gameObject.activeInHierarchy && RuneColor(gates[i]) == Green,
                    $"Gate {i + 1} not green + EXPLORED");
                Check(runes.First(r => r.ChamberIndex == i).TryReturn(), $"Return Rune {i + 1} failed");
            }

            Check(gates[4].TryRequestActivation(), "Moon Gate travel failed");
            await Wait(0.3f);
            Expect(1, 4, "WOLF", "inside Moon Crypt");
            buttons[0].Press(); buttons[1].Press(); buttons[2].Press();
            await Until(() => quest.ExploredCount == 5, 5f);
            await Wait(0.2f);
            Expect(1, 5, "CELESTIAL", "after Moon Crypt sequence");
            Check(gates[4].ExploredLabel.gameObject.activeInHierarchy && RuneColor(gates[4]) == Green, "Moon Gate not green + EXPLORED");

            ritual.SolveCurrentStageForPresentation();
            await Until(() => ritual.CurrentStage == 2, 6f);
            await Wait(1.2f);
            Expect(2, 5, "BLOOD SIGIL", "after Celestial Lock");

            ritual.SolveCurrentStageForPresentation();
            await Until(() => ritual.IsComplete, 6f);
            await Wait(0.5f);
            Expect(3, 5, "COMPLETE", "after Exit Lock");

            ritual.ResetPuzzle();
            await Wait(1f);
            Expect(0, 0, "SILVER FANG", "after final reset");
            Check(gates.All(g => !g.ExploredLabel.gameObject.activeInHierarchy), "EXPLORED labels survived reset");

            // Presentation shortcut K path: solve all three Locks in sequence.
            ritual.SolveAllForPresentation();
            await Until(() => ritual.IsComplete, 20f);
            await Wait(0.5f);
            Check(ritual.IsComplete && board.InstructionValue.Contains("COMPLETE") && board.ProgressValue.Contains("3 / 3"),
                "presentation solve-all did not end at 3 / 3 with the completion message");
            ritual.ResetPuzzle();
        }

        private static readonly Color Locked = new Color(0.08f, 0.06f, 0.10f, 1f);
        private static readonly Color Purple = new Color(0.62f, 0.18f, 1f, 1f);
        private static readonly Color BlueWhite = new Color(0.48f, 0.86f, 1f, 1f);
        private static readonly Color Green = new Color(0.1f, 1f, 0.15f, 1f);

        private static Color RuneColor(ManorGatePortal gate)
        {
            Renderer rune = gate.GetComponentsInChildren<Renderer>(true).First(r => r.name == "GateRune");
            var block = new MaterialPropertyBlock();
            rune.GetPropertyBlock(block);
            return block.GetColor("_BaseColor");
        }

        private static int RevealIndex(ManorChamberReveal reveal)
        {
            Transform chamber = reveal.transform;
            while (chamber != null && !chamber.name.StartsWith("Chamber_")) chamber = chamber.parent;
            return chamber != null ? int.Parse(chamber.name.Substring(8, 2)) - 1 : -1;
        }

        private static void Expect(int stage, int explored, string objectiveWord, string when)
        {
            string progress = board.ProgressValue ?? "";
            string instruction = board.InstructionValue ?? "";
            int left = 3 - stage;
            Check(ritual.ProgressText.text == progress && ritual.InstructionText.text == instruction, $"{when}: board text overwritten");
            Check(progress.Contains($"RITUAL PROGRESS    {stage} / 3"), $"{when}: ritual progress wrong ({progress.Split('\n')[0]})");
            Check(progress.Contains($"KEYS REMAINING     {left}") && progress.Contains($"LOCKS REMAINING    {left}"), $"{when}: key/lock counts wrong");
            Check(progress.Contains($"CHAMBERS EXPLORED  {explored} / 5"), $"{when}: chamber count wrong");
            Check(instruction.Contains(objectiveWord), $"{when}: objective '{instruction.Replace('\n', ' ')}' lacks {objectiveWord}");
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
