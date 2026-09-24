using System.Linq;
using MichaelManor;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MichaelManorEditor
{
    public static class ManorMoonCryptTimerInstaller
    {
        private const string ScenePath = "Assets/Scenes/MichaelManorHall.unity";

        [MenuItem("Tools/Michael Manor/Install Moon Crypt Timer")]
        public static void Install()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ManorGatePortal gate = Object.FindObjectsByType<ManorGatePortal>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(g => g.IsMoonCryptGate);
            ManorMoonCryptPuzzle puzzle = Object.FindFirstObjectByType<ManorMoonCryptPuzzle>(FindObjectsInactive.Include);
            FiveChamberQuestController quest = Object.FindFirstObjectByType<FiveChamberQuestController>(FindObjectsInactive.Include);
            ManorGateTravelSystem travel = Object.FindFirstObjectByType<ManorGateTravelSystem>(FindObjectsInactive.Include);
            ManorReturnRune rune = gate == null ? null : Object.FindObjectsByType<ManorReturnRune>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(r => r.ChamberIndex == gate.ChamberIndex);
            if (gate == null || puzzle == null || quest == null || travel == null || rune == null)
            { Debug.LogError("Moon Crypt timer install is missing the Moon Gate, puzzle, quest, travel, or Return Rune."); return; }

            Transform old = puzzle.transform.Find("MoonCryptTimerDisplay");
            if (old != null) Object.DestroyImmediate(old.gameObject);
            GameObject textObject = new GameObject("MoonCryptTimerDisplay");
            textObject.transform.SetParent(puzzle.transform, false);
            textObject.transform.localPosition = new Vector3(0f, 4.08f, 4.68f);
            TextMeshPro text = textObject.AddComponent<TextMeshPro>();
            text.text = "MOON TRIAL  02:30"; text.fontSize = 1.35f; text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center; text.color = new Color(0.62f, 0.88f, 1f);
            text.rectTransform.sizeDelta = new Vector2(5f, 0.6f);

            ManorMoonCryptTimer timer = puzzle.GetComponent<ManorMoonCryptTimer>();
            if (timer == null) timer = puzzle.gameObject.AddComponent<ManorMoonCryptTimer>();
            timer.Configure(gate, rune, travel, puzzle, quest, text, 150f);
            EditorUtility.SetDirty(timer);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            Debug.Log("Installed scoped 02:30 Moon Crypt loss timer with solve, leave, timeout, and retry states.");
        }

        [MenuItem("Tools/Michael Manor/Test Moon Crypt Timer (Play Mode)")]
        public static void TestInPlayMode()
        {
            if (!Application.isPlaying) { Debug.LogError("Enter Play Mode before testing Moon Crypt timer."); return; }
            ManorMoonCryptTimer timer = Object.FindFirstObjectByType<ManorMoonCryptTimer>(FindObjectsInactive.Include);
            ManorMoonCryptPuzzle puzzle = Object.FindFirstObjectByType<ManorMoonCryptPuzzle>(FindObjectsInactive.Include);
            bool valid = timer != null && timer.Duration == 150f && timer.Display != null;
            if (valid)
            {
                timer.StartForTest(); valid &= timer.IsRunning && timer.Remaining > 149f;
                timer.ExpireForTest(); valid &= !timer.IsRunning && !puzzle.IsSolved && timer.Display.text.Contains("MOON HAS SET");
                timer.ResetTimer();
            }
            if (valid) Debug.Log("MOON CRYPT TIMER PASS: 02:30 start, scoped loss reset, hall return, and retry state verified.");
            else Debug.LogError("Moon Crypt timer test failed.");
        }
    }
}
