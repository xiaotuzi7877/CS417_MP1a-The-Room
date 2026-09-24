using System.Linq;
using MichaelManor;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MichaelManorEditor
{
    public static class ManorWatcherChestInstaller
    {
        private const string ScenePath = "Assets/Scenes/MichaelManorHall.unity";

        [MenuItem("Tools/Michael Manor/Install Watcher Chest Reveal")]
        public static void InstallWatcherChestReveal()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject questRoot = FindSceneObject(scene, "FiveChamberQuest");
            ManorThreeStagePuzzle ritual =
                Object.FindFirstObjectByType<ManorThreeStagePuzzle>(FindObjectsInactive.Include);
            FiveChamberQuestController chamberQuest =
                Object.FindFirstObjectByType<FiveChamberQuestController>(FindObjectsInactive.Include);

            if (questRoot == null || ritual == null || chamberQuest == null)
            {
                Debug.LogError("Watcher chest install stopped: install the ritual and Section 1 foundation first.");
                return;
            }

            Transform previous = questRoot.transform.Find("WatcherChestReveal");
            if (previous != null)
            {
                Object.DestroyImmediate(previous.gameObject);
            }

            Material darkWood = LoadMaterial("DarkWood");
            Material gold = LoadMaterial("AntiqueGold");
            Material blackIron = LoadMaterial("BlackIron");
            Material parchment = LoadMaterial("AgedPlaster");
            Material purple = LoadMaterial("SpectralGlow");

            Transform chest = NewGroup("WatcherChestReveal", questRoot.transform);
            chest.position = new Vector3(-5.0f, 1.08f, -3.0f);
            chest.rotation = Quaternion.Euler(0f, -90f, 0f);

            Transform shell = NewGroup("ChestShell", chest);
            CreatePart("Bottom", PrimitiveType.Cube, shell, new Vector3(0f, 0.08f, 0f),
                new Vector3(1.55f, 0.16f, 0.95f), darkWood, true);
            CreatePart("FrontWall", PrimitiveType.Cube, shell, new Vector3(0f, 0.38f, -0.43f),
                new Vector3(1.55f, 0.60f, 0.09f), darkWood, true);
            CreatePart("BackWall", PrimitiveType.Cube, shell, new Vector3(0f, 0.38f, 0.43f),
                new Vector3(1.55f, 0.60f, 0.09f), darkWood, true);
            CreatePart("LeftWall", PrimitiveType.Cube, shell, new Vector3(-0.73f, 0.38f, 0f),
                new Vector3(0.09f, 0.60f, 0.78f), darkWood, true);
            CreatePart("RightWall", PrimitiveType.Cube, shell, new Vector3(0.73f, 0.38f, 0f),
                new Vector3(0.09f, 0.60f, 0.78f), darkWood, true);
            CreatePart("FrontGoldBand", PrimitiveType.Cube, shell, new Vector3(0f, 0.48f, -0.485f),
                new Vector3(1.36f, 0.08f, 0.035f), gold, false);
            CreatePart("LockPlate", PrimitiveType.Cube, shell, new Vector3(0f, 0.37f, -0.50f),
                new Vector3(0.24f, 0.28f, 0.04f), blackIron, false);
            CreatePart("LockRune", PrimitiveType.Sphere, shell, new Vector3(0f, 0.39f, -0.55f),
                new Vector3(0.10f, 0.10f, 0.035f), purple, false);

            Transform lidHinge = NewGroup("LidHinge_Moving", chest);
            lidHinge.localPosition = new Vector3(0f, 0.72f, 0.46f);
            Transform lid = NewGroup("Lid", lidHinge);
            CreatePart("LidPanel", PrimitiveType.Cube, lid, new Vector3(0f, 0f, -0.46f),
                new Vector3(1.62f, 0.16f, 0.98f), darkWood, true);
            CreatePart("LidGoldBand", PrimitiveType.Cube, lid, new Vector3(0f, 0.095f, -0.46f),
                new Vector3(1.38f, 0.045f, 0.82f), gold, false);
            CreatePart("LidCrest", PrimitiveType.Sphere, lid, new Vector3(0f, 0.135f, -0.46f),
                new Vector3(0.18f, 0.05f, 0.18f), blackIron, false);

            Transform contents = NewGroup("RevealedContents", chest);
            Transform codex = NewGroup("MidnightCodex", contents);
            codex.localPosition = new Vector3(0f, 0.22f, -0.08f);
            CreatePart("BookCover", PrimitiveType.Cube, codex, Vector3.zero,
                new Vector3(1.20f, 0.055f, 0.58f), blackIron, false);
            GameObject leftPage = CreatePart("LeftPage", PrimitiveType.Cube, codex,
                new Vector3(-0.30f, 0.055f, 0f), new Vector3(0.58f, 0.035f, 0.53f), parchment, false);
            leftPage.transform.localRotation = Quaternion.Euler(0f, 0f, 3f);
            GameObject rightPage = CreatePart("RightPage", PrimitiveType.Cube, codex,
                new Vector3(0.30f, 0.055f, 0f), new Vector3(0.58f, 0.035f, 0.53f), parchment, false);
            rightPage.transform.localRotation = Quaternion.Euler(0f, 0f, -3f);

            TextMeshPro codexTitle = CreateWorldText(
                "CodexTitle",
                codex,
                new Vector3(-0.30f, 0.085f, -0.01f),
                Quaternion.Euler(-90f, 0f, 180f),
                new Vector2(0.48f, 0.44f),
                0.30f,
                "MIDNIGHT\nCODEX\n\nI / V",
                new Color(0.16f, 0.025f, 0.02f, 1f));
            codexTitle.enableAutoSizing = false;

            TextMeshPro codexClue = CreateWorldText(
                "CodexClue",
                codex,
                new Vector3(0.30f, 0.085f, -0.01f),
                Quaternion.Euler(-90f, 0f, 180f),
                new Vector2(0.48f, 0.44f),
                0.20f,
                "WHICH GATE\nANSWERS\nMIDNIGHT?\n\nFOLLOW\nTHE MOON.",
                new Color(0.16f, 0.025f, 0.02f, 1f));
            codexClue.enableAutoSizing = false;

            Transform map = NewGroup("FiveEntranceMap", contents);
            map.localPosition = new Vector3(0f, 0.29f, 0.27f);
            CreatePart("MapBacking", PrimitiveType.Cube, map, Vector3.zero,
                new Vector3(1.18f, 0.035f, 0.23f), parchment, false);
            for (int i = 0; i < FiveChamberQuestController.RequiredChamberCount; i++)
            {
                float x = -0.44f + i * 0.22f;
                CreatePart($"PassageRune_{i + 1}", PrimitiveType.Cylinder, map,
                    new Vector3(x, 0.045f, 0f), new Vector3(0.065f, 0.018f, 0.065f),
                    purple, false).transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            }

            TextMeshPro mapText = CreateWorldText(
                "MapLabel",
                map,
                new Vector3(0f, 0.07f, -0.075f),
                Quaternion.Euler(-90f, 0f, 180f),
                new Vector2(1.10f, 0.12f),
                0.22f,
                "FIVE PASSAGES  /  FOUR FALSE  /  ONE MOON",
                new Color(0.18f, 0.05f, 0.16f, 1f));
            mapText.enableAutoSizing = false;

            ManorClueChestReveal reveal = chest.gameObject.AddComponent<ManorClueChestReveal>();
            reveal.Configure(
                ritual,
                chamberQuest,
                lidHinge,
                contents.gameObject,
                Vector3.zero,
                new Vector3(-108f, 0f, 0f),
                1.25f);

            chamberQuest.ResetQuest();
            EditorUtility.SetDirty(reveal);
            EditorUtility.SetDirty(chamberQuest);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Installed the Watcher chest physical Reveal, Midnight Codex, and five-passage map.");
        }

        private static Transform NewGroup(string name, Transform parent)
        {
            Transform group = new GameObject(name).transform;
            group.SetParent(parent, false);
            return group;
        }

        private static GameObject CreatePart(
            string name,
            PrimitiveType type,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Material material,
            bool keepCollider)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            Renderer renderer = part.GetComponent<Renderer>();
            if (renderer != null && material != null)
            {
                renderer.sharedMaterial = material;
            }

            if (!keepCollider)
            {
                Collider collider = part.GetComponent<Collider>();
                if (collider != null)
                {
                    Object.DestroyImmediate(collider);
                }
            }

            return part;
        }

        private static TextMeshPro CreateWorldText(
            string name,
            Transform parent,
            Vector3 localPosition,
            Quaternion localRotation,
            Vector2 size,
            float fontSize,
            string message,
            Color color)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            textObject.transform.localPosition = localPosition;
            textObject.transform.localRotation = localRotation;
            TextMeshPro text = textObject.AddComponent<TextMeshPro>();
            text.text = message;
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.enableWordWrapping = true;
            text.rectTransform.sizeDelta = size;
            text.color = color;
            text.fontStyle = FontStyles.Bold;
            return text;
        }

        private static Material LoadMaterial(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/MichaelManor/Materials/" + name + ".mat");
        }

        private static GameObject FindSceneObject(Scene scene, string name)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Transform match = root.GetComponentsInChildren<Transform>(true)
                    .FirstOrDefault(transform => transform.name == name);
                if (match != null)
                {
                    return match.gameObject;
                }
            }

            return null;
        }
    }
}
