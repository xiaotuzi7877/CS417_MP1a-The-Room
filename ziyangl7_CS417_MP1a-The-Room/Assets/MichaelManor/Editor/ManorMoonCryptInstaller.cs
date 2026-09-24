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
    public static class ManorMoonCryptInstaller
    {
        private const string ScenePath = "Assets/Scenes/MichaelManorHall.unity";

        [MenuItem("Tools/Michael Manor/Install Moon Crypt Puzzle")]
        public static void Install()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Transform chamber = Find(scene, "Chamber_05_MoonCrypt");
            ManorThreeStagePuzzle ritual = Object.FindFirstObjectByType<ManorThreeStagePuzzle>(FindObjectsInactive.Include);
            ManorChamberInteraction state = Find(scene, "State_05_MoonCrypt")?.GetComponent<ManorChamberInteraction>();
            ManorKeyArtifact moonstone = Artifact("Moonstone");
            ManorKeyArtifact bloodSigil = Artifact("BloodSigil");
            Transform celestialLock = Find(scene, "Lock_02_CelestialConsole");
            Transform returnRune = chamber != null ? chamber.Find("TravelShell/ReturnRune") : null;
            if (chamber == null || ritual == null || state == null || moonstone == null || bloodSigil == null ||
                celestialLock == null || returnRune == null)
            {
                Debug.LogError("Moon Crypt install stopped: ritual artifacts, lock, chamber state, or Return Rune is missing.");
                return;
            }

            // Re-running must never destroy the key artifacts or the lock that a previous run moved inside.
            Transform old = chamber.Find("Content_Section06");
            if (old != null)
            {
                moonstone.transform.SetParent(chamber, true);
                bloodSigil.transform.SetParent(chamber, true);
                celestialLock.SetParent(chamber, true);
                Object.DestroyImmediate(old.gameObject);
            }
            Transform oldLabel = celestialLock.Find("CelestialLockLabel");
            if (oldLabel != null) Object.DestroyImmediate(oldLabel.gameObject);
            Transform content = Group("Content_Section06", chamber);
            returnRune.localPosition = new Vector3(2.3f, 0.08f, -2.6f);

            Material stone = Mat("ManorStone");
            Material wood = Mat("DarkWood");
            Material iron = Mat("BlackIron");
            Material gold = Mat("AntiqueGold");
            Material moon = Mat("CelestialMoon");
            Material glow = Mat("SpectralGlow");
            Material blood = Mat("BloodVelvet");
            if (gold != null) returnRune.GetComponent<Renderer>().sharedMaterial = gold;

            PointLight("MoonCrypt_Light", content, new Vector3(0f, 3.7f, 0.8f),
                new Color(0.34f, 0.54f, 1f), 135f, 7f);
            Part("Dais", PrimitiveType.Cylinder, content, new Vector3(0f, 0.20f, 1.5f),
                new Vector3(2.0f, 0.20f, 2.0f), stone, true);
            Part("Pedestal", PrimitiveType.Cylinder, content, new Vector3(0f, 0.72f, 1.5f),
                new Vector3(0.62f, 0.55f, 0.62f), iron, true);
            BuildMoonArch(content, gold, stone, moon);

            Text("SequenceInstruction", content, new Vector3(0f, 3.45f, 4.73f), new Vector2(5.8f, 1.25f),
                1.75f, "PRESS IN ORDER:\n1. WOLF     2. MOON     3. BLOOD", new Color(0.66f, 0.88f, 1f));
            TextMeshPro status = Text("SequenceStatus", content, new Vector3(0f, 2.85f, 4.71f),
                new Vector2(4.6f, 0.60f), 1.20f, "SEQUENCE  0 / 3", new Color(1f, 0.72f, 0.25f));

            Transform vault = Group("MoonstoneVault", content);
            vault.localPosition = new Vector3(0f, 0f, 3.6f);
            Part("MoonstonePlinth", PrimitiveType.Cylinder, vault, new Vector3(0f, 0.49f, -0.12f),
                new Vector3(0.45f, 0.49f, 0.45f), iron, true);
            Part("VaultBack", PrimitiveType.Cube, vault, new Vector3(0f, 1.18f, 0.58f),
                new Vector3(2.3f, 2.35f, 0.18f), stone, true);
            Part("VaultLeft", PrimitiveType.Cube, vault, new Vector3(-1.05f, 1.18f, 0f),
                new Vector3(0.18f, 2.35f, 1.25f), stone, true);
            Part("VaultRight", PrimitiveType.Cube, vault, new Vector3(1.05f, 1.18f, 0f),
                new Vector3(0.18f, 2.35f, 1.25f), stone, true);
            Transform moonSlab = Part("MoonstoneSlab_Moving", PrimitiveType.Cube, vault,
                new Vector3(0f, 1.18f, -0.75f), new Vector3(2.0f, 2.25f, 0.20f), stone, true);
            CreateMoonMark(moonSlab, moon, gold);

            moonstone.transform.SetParent(vault, false);
            moonstone.transform.localPosition = new Vector3(0f, 1.14f, -0.12f);
            moonstone.transform.localRotation = Quaternion.identity;
            moonstone.gameObject.SetActive(false);

            celestialLock.SetParent(content, false);
            celestialLock.localPosition = new Vector3(-2.25f, 0f, -0.25f);
            celestialLock.localRotation = Quaternion.identity;
            Text("CelestialLockLabel", celestialLock, new Vector3(0f, 2.15f, 0f), new Vector2(2.3f, 0.75f),
                1.25f, "CELESTIAL LOCK\nMOONSTONE", new Color(0.55f, 0.86f, 1f));

            Transform bloodVault = Group("BloodSigilVault", content);
            bloodVault.localPosition = new Vector3(2.35f, 0f, 1.0f);
            Part("BloodVaultBack", PrimitiveType.Cube, bloodVault, new Vector3(0f, 1.15f, 0.60f),
                new Vector3(1.55f, 2.30f, 0.16f), stone, true);
            Transform bloodSlab = Part("BloodSlab_Moving", PrimitiveType.Cube, bloodVault,
                new Vector3(0f, 1.15f, -0.48f), new Vector3(1.42f, 2.20f, 0.18f), iron, true);
            Part("BloodSeal", PrimitiveType.Sphere, bloodSlab, new Vector3(0f, 0f, -0.58f),
                new Vector3(0.28f, 0.28f, 0.08f), blood, false);
            Part("BloodSigilPlinth", PrimitiveType.Cylinder, bloodVault, new Vector3(0f, 0.49f, 0f),
                new Vector3(0.45f, 0.49f, 0.45f), iron, true);
            bloodSigil.transform.SetParent(bloodVault, false);
            bloodSigil.transform.localPosition = new Vector3(0f, 1.12f, 0f);
            bloodSigil.transform.localRotation = Quaternion.identity;
            bloodSigil.gameObject.SetActive(false);

            ManorMoonCryptPuzzle puzzle = content.gameObject.AddComponent<ManorMoonCryptPuzzle>();
            Renderer[] renderers = new Renderer[3];
            Light[] lights = new Light[3];
            string[] labels = { "WOLF", "MOON", "BLOOD" };
            Part("ButtonConsole", PrimitiveType.Cube, content, new Vector3(0f, 0.50f, 0.30f),
                new Vector3(4.4f, 1.00f, 0.12f), wood, true);
            for (int i = 0; i < 3; i++)
            {
                float x = -1.55f + i * 1.55f;
                Transform button = Part($"Button_{i + 1}_{labels[i]}", PrimitiveType.Cylinder, content,
                    new Vector3(x, 0.72f, 0.15f), new Vector3(0.46f, 0.18f, 0.46f), glow, true);
                button.localRotation = Quaternion.Euler(90f, 0f, 0f);
                button.gameObject.AddComponent<XRSimpleInteractable>();
                ManorMoonSequenceButton sequenceButton = button.gameObject.AddComponent<ManorMoonSequenceButton>();
                sequenceButton.Configure(puzzle, i);
                renderers[i] = button.GetComponent<Renderer>();
                GameObject lightObject = new GameObject("ButtonLight");
                lightObject.transform.SetParent(button, false);
                lights[i] = lightObject.AddComponent<Light>();
                lights[i].type = LightType.Point;
                lights[i].range = 1.8f;
                Text($"Label_{labels[i]}", content, new Vector3(x, 1.24f, 0.15f), new Vector2(1.45f, 0.40f),
                    1.05f, labels[i], i == 2 ? new Color(1f, 0.28f, 0.22f) : new Color(0.64f, 0.86f, 1f));
            }

            state.Configure(state.QuestController, 4);
            ritual.SetArtifactAutoReveal(1, false);
            ritual.ConfigureStageReveal(0, null, Vector3.zero, Vector3.zero);
            TMP_Text heavensPlaque = Find(scene, "Clue_02_Heavens")?.GetComponentInChildren<TMP_Text>(true);
            if (heavensPlaque != null)
            {
                heavensPlaque.text = "II  MOON -> MOON CRYPT";
                EditorUtility.SetDirty(heavensPlaque);
            }
            ritual.ConfigureStageReveal(1, bloodSlab, bloodSlab.localPosition + new Vector3(0f, 2.3f, 0f), Vector3.zero);
            puzzle.Configure(state, ritual, renderers, lights, moonSlab,
                moonSlab.localPosition + new Vector3(-2.25f, 0f, 0f), Vector3.zero,
                moonstone, returnRune.gameObject, status);

            EditorUtility.SetDirty(state);
            EditorUtility.SetDirty(ritual);
            EditorUtility.SetDirty(puzzle);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Installed Moon Crypt Wolf-Moon-Blood puzzle, Moonstone reveal, Celestial Lock, and Blood Sigil reveal.");
        }

        private static void BuildMoonArch(Transform parent, Material gold, Material stone, Material moon)
        {
            Part("MoonPillarLeft", PrimitiveType.Cylinder, parent, new Vector3(-2.8f, 1.8f, 2.5f),
                new Vector3(0.28f, 1.8f, 0.28f), stone, true);
            Part("MoonPillarRight", PrimitiveType.Cylinder, parent, new Vector3(2.8f, 1.8f, 2.5f),
                new Vector3(0.28f, 1.8f, 0.28f), stone, true);
            for (int i = 0; i < 13; i++)
            {
                float angle = Mathf.Lerp(15f, 165f, i / 12f) * Mathf.Deg2Rad;
                Part($"MoonArch_{i + 1}", PrimitiveType.Cube, parent,
                    new Vector3(Mathf.Cos(angle) * 2.8f, 1.75f + Mathf.Sin(angle) * 2.4f, 2.5f),
                    new Vector3(0.42f, 0.28f, 0.35f), i % 2 == 0 ? gold : moon, false)
                    .localRotation = Quaternion.Euler(0f, 0f, -angle * Mathf.Rad2Deg + 90f);
            }
        }

        private static void CreateMoonMark(Transform parent, Material moon, Material gold)
        {
            Transform mark = Group("MoonMark", parent);
            mark.localPosition = new Vector3(0f, 0f, -0.58f);
            for (int i = 0; i < 12; i++)
            {
                float angle = Mathf.Lerp(-130f, 130f, i / 11f) * Mathf.Deg2Rad;
                Part($"MoonArc_{i + 1}", PrimitiveType.Sphere, mark,
                    new Vector3(Mathf.Cos(angle) * 0.42f, Mathf.Sin(angle) * 0.42f, 0f),
                    Vector3.one * 0.10f, i % 2 == 0 ? moon : gold, false);
            }
        }

        private static TextMeshPro Text(string name, Transform parent, Vector3 position, Vector2 size, float fontSize, string value, Color color)
        {
            GameObject target = new GameObject(name);
            target.transform.SetParent(parent, false);
            target.transform.localPosition = position;
            TextMeshPro text = target.AddComponent<TextMeshPro>();
            text.text = value;
            text.fontSize = fontSize;
            text.enableAutoSizing = true;
            text.fontSizeMin = fontSize * 0.55f;
            text.fontSizeMax = fontSize;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = color;
            text.rectTransform.sizeDelta = size;
            return text;
        }

        private static Transform Part(string name, PrimitiveType type, Transform parent, Vector3 position, Vector3 scale, Material material, bool collider)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            if (material != null) part.GetComponent<Renderer>().sharedMaterial = material;
            if (!collider) Object.DestroyImmediate(part.GetComponent<Collider>());
            return part.transform;
        }

        private static void PointLight(string name, Transform parent, Vector3 position, Color color, float intensity, float range)
        {
            GameObject target = new GameObject(name);
            target.transform.SetParent(parent, false);
            target.transform.localPosition = position;
            Light light = target.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.Soft;
        }

        private static Transform Group(string name, Transform parent)
        {
            Transform result = new GameObject(name).transform;
            result.SetParent(parent, false);
            return result;
        }

        private static Material Mat(string name) => AssetDatabase.LoadAssetAtPath<Material>($"Assets/MichaelManor/Materials/{name}.mat");
        private static Transform Find(Scene scene, string name) => scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(true)).FirstOrDefault(item => item.name == name);
        private static ManorKeyArtifact Artifact(string id) => Object
            .FindObjectsByType<ManorKeyArtifact>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .FirstOrDefault(item => item.ArtifactId == id);
    }
}
