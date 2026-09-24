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
    public static class ManorMoonShardInstaller
    {
        private const string ScenePath = "Assets/Scenes/MichaelManorHall.unity";
        private static readonly string[] ChamberNames = { "Chamber_01_Cellar", "Chamber_02_BoneCloset", "Chamber_03_CoffinVault", "Chamber_04_Portrait", "Chamber_05_MoonCrypt" };

        [MenuItem("Tools/Michael Manor/Install Moon Shard Collectibles")]
        public static void Install()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ManorThreeStagePuzzle ritual = Object.FindFirstObjectByType<ManorThreeStagePuzzle>(FindObjectsInactive.Include);
            Transform decor = Find(scene, "Decor"); Transform systems = Find(scene, "Systems");
            if (ritual == null || decor == null || systems == null || ChamberNames.Any(n => Find(scene, n) == null))
            { Debug.LogError("Moon Shard install requires ritual, Decor, Systems, and all five chambers."); return; }

            Transform old = decor.Find("MoonShardCollectibles"); if (old != null) Object.DestroyImmediate(old.gameObject);
            Transform root = new GameObject("MoonShardCollectibles").transform; root.SetParent(decor, false);
            GameObject controllerObject = systems.Find("MoonShardCollection")?.gameObject;
            if (controllerObject == null) { controllerObject = new GameObject("MoonShardCollection"); controllerObject.transform.SetParent(systems, false); }
            ManorMoonShardCollection collection = controllerObject.GetComponent<ManorMoonShardCollection>();
            if (collection == null) collection = controllerObject.AddComponent<ManorMoonShardCollection>();

            ManorMoonShard[] shards = new ManorMoonShard[8];
            Vector3[] hallPositions = { new Vector3(-6.2f, 1.05f, -8f), new Vector3(6.2f, 1.05f, 0f), new Vector3(-5.7f, 1.05f, 7.3f) };
            for (int i = 0; i < 3; i++) shards[i] = CreateShard(root, i, hallPositions[i]);
            for (int i = 0; i < 5; i++) shards[i + 3] = CreateShard(Find(scene, ChamberNames[i]), i + 3, new Vector3(-2.4f + 0.25f * i, 1.15f, -2.0f));

            Transform boardParent = ritual.ProgressText.transform.parent;
            Transform oldText = boardParent.Find("MoonShardCounter"); if (oldText != null) Object.DestroyImmediate(oldText.gameObject);
            GameObject counterObject = new GameObject("MoonShardCounter"); counterObject.transform.SetParent(boardParent, false);
            counterObject.transform.localPosition = ritual.ProgressText.transform.localPosition + new Vector3(0f, -1.18f, -0.02f);
            counterObject.transform.localRotation = ritual.ProgressText.transform.localRotation;
            TextMeshPro counter = counterObject.AddComponent<TextMeshPro>();
            counter.text = "MOON SHARDS  0 / 8"; counter.fontSize = 0.68f; counter.fontStyle = FontStyles.Bold;
            counter.alignment = TextAlignmentOptions.Center; counter.color = new Color(0.58f, 0.88f, 1f);
            counter.rectTransform.sizeDelta = new Vector2(5.6f, 0.75f);

            collection.Configure(shards, counter, ritual);
            for (int i = 0; i < shards.Length; i++) shards[i].Configure(collection, i);
            EditorUtility.SetDirty(collection); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            Debug.Log("Installed eight optional one-shot Moon Shards across the hall and all five gated chambers.");
        }

        [MenuItem("Tools/Michael Manor/Test Moon Shards (Play Mode)")]
        public static void TestInPlayMode()
        {
            if (!Application.isPlaying) { Debug.LogError("Enter Play Mode before testing Moon Shards."); return; }
            ManorMoonShardCollection collection = Object.FindFirstObjectByType<ManorMoonShardCollection>(FindObjectsInactive.Include);
            ManorMoonShard[] shards = Object.FindObjectsByType<ManorMoonShard>(FindObjectsInactive.Include, FindObjectsSortMode.None).OrderBy(s => s.ShardIndex).ToArray();
            ManorThreeStagePuzzle ritual = Object.FindFirstObjectByType<ManorThreeStagePuzzle>(FindObjectsInactive.Include);
            int stage = ritual.CurrentStage;
            bool valid = collection != null && shards.Length == 8 && collection.TotalCount == 8 && shards.All(s => s.GetComponent<XRSimpleInteractable>() != null && s.GetComponent<Collider>() != null);
            if (valid)
            {
                foreach (ManorMoonShard shard in shards) valid &= shard.CollectForTest();
                valid &= collection.CollectedCount == 8 && ritual.CurrentStage == stage && !shards[0].CollectForTest();
                collection.ResetCollection(); valid &= collection.CollectedCount == 0 && shards.All(s => s.gameObject.activeSelf);
            }
            if (valid) Debug.Log("MOON SHARDS PASS: eight explicit one-shot collectibles, 0/8 reset, and main ritual independence verified.");
            else Debug.LogError("Moon Shard test failed.");
        }

        private static ManorMoonShard CreateShard(Transform parent, int index, Vector3 position)
        {
            GameObject shard = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shard.name = $"MoonShard_{index + 1:00}"; shard.transform.SetParent(parent, false);
            shard.transform.localPosition = position; shard.transform.localRotation = Quaternion.Euler(0f, index * 37f, 22f);
            shard.transform.localScale = new Vector3(0.22f, 0.48f, 0.16f);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/MichaelManor/Materials/CelestialMoon.mat");
            if (mat != null) shard.GetComponent<Renderer>().sharedMaterial = mat;
            shard.AddComponent<XRSimpleInteractable>(); return shard.AddComponent<ManorMoonShard>();
        }

        private static Transform Find(Scene scene, string name) => scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).FirstOrDefault(t => t.name == name);
    }
}
