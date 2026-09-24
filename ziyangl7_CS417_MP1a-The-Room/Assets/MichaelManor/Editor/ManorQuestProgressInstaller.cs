using System.Linq;
using MichaelManor;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MichaelManorEditor
{
    /// Section 7: drives the existing hall task board from quest state, adds EXPLORED
    /// signifiers to the five Gates, and turns the clue plaques to face the hall.
    /// Only text content, text formatting, and new child labels change; no panel moves.
    public static class ManorQuestProgressInstaller
    {
        private const string ScenePath = "Assets/Scenes/MichaelManorHall.unity";
        private static readonly Color ExploredGreen = new Color(0.35f, 1f, 0.55f);
        private static readonly Color ExploredRune = new Color(0.1f, 1f, 0.15f, 1f);

        [MenuItem("Tools/Michael Manor/Install Quest Progress Board")]
        public static void Install()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ManorThreeStagePuzzle ritual = Object.FindFirstObjectByType<ManorThreeStagePuzzle>(FindObjectsInactive.Include);
            FiveChamberQuestController quest = Object.FindFirstObjectByType<FiveChamberQuestController>(FindObjectsInactive.Include);
            ManorMoonCryptPuzzle moonCrypt = Object.FindFirstObjectByType<ManorMoonCryptPuzzle>(FindObjectsInactive.Include);
            ManorGatePortal[] gates = Object.FindObjectsByType<ManorGatePortal>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .OrderBy(gate => gate.ChamberIndex).ToArray();
            if (ritual == null || quest == null || moonCrypt == null || gates.Length != 5 ||
                ritual.ProgressText == null || ritual.InstructionText == null)
            {
                Debug.LogError("Quest progress install stopped: ritual, quest, Moon Crypt, five Gates, or board text is missing.");
                return;
            }

            FitToBoard(ritual.ProgressText, 1.15f);
            FitToBoard(ritual.InstructionText, 1.0f);
            ManorQuestScoreboard board = ritual.GetComponent<ManorQuestScoreboard>();
            if (board == null) board = ritual.gameObject.AddComponent<ManorQuestScoreboard>();
            board.Configure(ritual, quest, moonCrypt, gates, ritual.ProgressText, ritual.InstructionText);

            foreach (ManorGatePortal gate in gates)
            {
                // The old mint green bloomed to nearly the Moon Gate's blue-white.
                var serialized = new SerializedObject(gate);
                serialized.FindProperty("exploredColor").colorValue = ExploredRune;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                gate.SetExploredLabel(CreateExploredLabel(gate));
                EditorUtility.SetDirty(gate);
            }

            // The plaques' text faced into the wall and read mirrored from the hall.
            foreach (string plaque in new[] { "Clue_01_Watcher", "Clue_02_Heavens", "Clue_03_Exit" })
            {
                Transform text = Find(scene, plaque)?.Find("Text");
                if (text == null) continue;
                text.localRotation = Quaternion.identity;
                EditorUtility.SetDirty(text);
            }

            EditorUtility.SetDirty(board);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Installed quest progress board, Gate EXPLORED signifiers, and hall-facing clue plaques.");
        }

        private static void FitToBoard(TMP_Text text, float maxSize)
        {
            text.enableAutoSizing = true;
            text.fontSizeMax = maxSize;
            text.fontSizeMin = maxSize * 0.5f;
            text.enableWordWrapping = false;   // objectives carry explicit line breaks
            EditorUtility.SetDirty(text);
        }

        private static TMP_Text CreateExploredLabel(ManorGatePortal gate)
        {
            Transform visual = gate.transform.Find("Visual");
            Transform rune = visual != null ? visual.Find("GateRune") : null;
            if (rune == null) return null;
            Transform old = visual.Find("ExploredLabel");
            if (old != null) Object.DestroyImmediate(old.gameObject);

            GameObject target = new GameObject("ExploredLabel");
            target.transform.SetParent(visual, false);
            bool floorGate = visual.Find("LooseFloorboard") != null;
            // Wall Gates face south toward the entrance; the label sits above the rune on that face.
            target.transform.position = floorGate
                ? rune.position + new Vector3(0f, 0.55f, -0.7f)
                : rune.position + new Vector3(0f, 0.62f, -0.03f);
            target.transform.rotation = Quaternion.identity;
            TextMeshPro text = target.AddComponent<TextMeshPro>();
            text.text = "EXPLORED";
            text.fontSize = 1.5f;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.enableWordWrapping = false;
            text.color = ExploredGreen;
            text.rectTransform.sizeDelta = new Vector2(1.3f, 0.35f);
            target.SetActive(false);
            return text;
        }

        private static Transform Find(Scene scene, string name) => scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(true)).FirstOrDefault(item => item.name == name);
    }
}
