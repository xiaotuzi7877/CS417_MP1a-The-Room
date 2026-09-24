using System.Linq;
using MichaelManor;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MichaelManorEditor
{
    public static class ManorRestartControlInstaller
    {
        private const string ScenePath = "Assets/Scenes/MichaelManorHall.unity";

        [MenuItem("Tools/Michael Manor/Install VR Restart Control")]
        public static void Install()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ManorThreeStagePuzzle ritual = Object.FindFirstObjectByType<ManorThreeStagePuzzle>(FindObjectsInactive.Include);
            FiveChamberQuestController quest = Object.FindFirstObjectByType<FiveChamberQuestController>(FindObjectsInactive.Include);
            ManorGateTravelSystem travel = Object.FindFirstObjectByType<ManorGateTravelSystem>(FindObjectsInactive.Include);
            ManorReturnRune returnRune = Object.FindObjectsByType<ManorReturnRune>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault();
            Transform systems = Find(scene, "Systems");
            if (ritual == null || quest == null || travel == null || returnRune == null || systems == null)
            {
                Debug.LogError("Restart control install requires ritual, quest, travel system, Return Rune, and Systems root.");
                return;
            }

            Transform old = systems.Find("VR_Restart_Control");
            if (old != null) Object.DestroyImmediate(old.gameObject);
            GameObject root = new GameObject("VR_Restart_Control");
            root.transform.SetParent(systems, false);
            root.transform.SetPositionAndRotation(new Vector3(0f, 1.25f, -14.72f), Quaternion.identity);

            GameObject plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plate.name = "Restart_Backplate";
            plate.transform.SetParent(root.transform, false);
            plate.transform.localScale = new Vector3(2.8f, 0.8f, 0.12f);
            ApplyMaterial(plate, "DarkWood");

            GameObject button = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            button.name = "Hold_To_Restart_Button";
            button.transform.SetParent(root.transform, false);
            button.transform.localPosition = new Vector3(0f, -0.04f, 0.18f);
            button.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            button.transform.localScale = new Vector3(0.24f, 0.08f, 0.24f);
            ApplyMaterial(button, "BloodRed");
            button.AddComponent<XRSimpleInteractable>();

            TextMeshPro label = CreateText(root.transform, "Restart_Label", new Vector3(0f, 0.22f, 0.09f), 0.26f,
                "HOLD 2 SEC TO RESTART", new Color(1f, 0.82f, 0.38f));
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ring.name = "Hold_Progress";
            ring.transform.SetParent(root.transform, false);
            ring.transform.localPosition = new Vector3(0f, -0.31f, 0.09f);
            ring.transform.localScale = new Vector3(2.25f, 0.07f, 0.05f);
            Object.DestroyImmediate(ring.GetComponent<Collider>());
            ApplyMaterial(ring, "Gold");

            ManorRitualRestartControl control = button.AddComponent<ManorRitualRestartControl>();
            control.Configure(ritual, quest, travel, returnRune.HallReturnAnchor, label, ring.transform, 2f);
            EditorSceneManager.MarkSceneDirty(scene);
            ManorTextOrientationFixer.Apply(); EditorSceneManager.SaveScene(scene);
            Debug.Log("Installed deliberate two-second world-space VR restart control beside the mission wall.");
        }

        [MenuItem("Tools/Michael Manor/Test VR Restart Control (Play Mode)")]
        public static void TestInPlayMode()
        {
            if (!Application.isPlaying) { Debug.LogError("Enter Play Mode before testing Restart Control."); return; }
            ManorRitualRestartControl control = Object.FindFirstObjectByType<ManorRitualRestartControl>(FindObjectsInactive.Include);
            ManorThreeStagePuzzle ritual = Object.FindFirstObjectByType<ManorThreeStagePuzzle>(FindObjectsInactive.Include);
            FiveChamberQuestController quest = Object.FindFirstObjectByType<FiveChamberQuestController>(FindObjectsInactive.Include);
            bool valid = control != null && control.enabled && control.GetComponent<XRSimpleInteractable>() != null &&
                         control.GetComponent<Collider>() != null && control.HoldDuration >= 1.5f && control.Label != null;
            if (valid)
            {
                quest.UnlockGates();
                control.RestartNowForTest();
                valid = ritual.CurrentStage == 0 && !quest.GatesUnlocked && quest.ExploredCount == 0;
            }
            if (valid) Debug.Log("VR RESTART CONTROL PASS: hold affordance is interactive and centralized reset restored ritual and chamber state.");
            else Debug.LogError("VR Restart Control test failed.");
        }

        private static TextMeshPro CreateText(Transform parent, string name, Vector3 localPosition, float size, string value, Color color)
        {
            GameObject target = new GameObject(name);
            target.transform.SetParent(parent, false);
            target.transform.localPosition = localPosition;
            TextMeshPro text = target.AddComponent<TextMeshPro>();
            text.text = value; text.fontSize = size; text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center; text.color = color;
            text.rectTransform.sizeDelta = new Vector2(2.6f, 0.5f);
            return text;
        }

        private static void ApplyMaterial(GameObject target, string contains)
        {
            string guid = AssetDatabase.FindAssets(contains + " t:Material").FirstOrDefault();
            if (!string.IsNullOrEmpty(guid)) target.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
        }

        private static Transform Find(Scene scene, string name) => scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(true)).FirstOrDefault(item => item.name == name);
    }
}
