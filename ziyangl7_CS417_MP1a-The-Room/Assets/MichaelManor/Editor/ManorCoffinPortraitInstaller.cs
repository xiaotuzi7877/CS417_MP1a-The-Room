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
    public static class ManorCoffinPortraitInstaller
    {
        private const string ScenePath = "Assets/Scenes/MichaelManorHall.unity";

        [MenuItem("Tools/Michael Manor/Install Coffin And Portrait Chambers")]
        public static void Install()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Transform locations = Find(scene, "GatedLocations");
            Transform states = Find(scene, "ChamberState");
            Material stone = Mat("ManorStone");
            Material wood = Mat("DarkWood");
            Material iron = Mat("BlackIron");
            Material gold = Mat("AntiqueGold");
            Material blood = Mat("BloodVelvet");
            Material glow = Mat("SpectralGlow");

            BuildCoffin(locations.Find("Chamber_03_CoffinVault"),
                states.Find("State_03_CoffinVault").GetComponent<ManorChamberInteraction>(),
                stone, wood, iron, gold, blood);
            BuildPortrait(locations.Find("Chamber_04_Portrait"),
                states.Find("State_04_Portrait").GetComponent<ManorChamberInteraction>(),
                stone, wood, gold, glow);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Installed Coffin Vault and Portrait Chamber interactions with the third false relic and Moon Crypt clue.");
        }

        private static void BuildCoffin(Transform chamber, ManorChamberInteraction state,
            Material stone, Material wood, Material iron, Material gold, Material blood)
        {
            ReplaceGroup(chamber, "Content_Section05", out Transform content);
            PointLight("Coffin_CandleLight", content, new Vector3(0f, 3.3f, 1.2f),
                new Color(1f, 0.28f, 0.08f), 115f, 6f);

            Transform coffin = Group("Coffin", content);
            coffin.localPosition = new Vector3(0f, 0f, 1.25f);
            Part("Base", PrimitiveType.Cube, coffin, new Vector3(0f, 0.42f, 0f),
                new Vector3(1.75f, 0.70f, 3.30f), wood, true);
            Part("FootTrim", PrimitiveType.Cube, coffin, new Vector3(0f, 0.20f, -1.62f),
                new Vector3(1.95f, 0.18f, 0.18f), gold, false);
            Part("HeadTrim", PrimitiveType.Cube, coffin, new Vector3(0f, 0.20f, 1.62f),
                new Vector3(1.95f, 0.18f, 0.18f), gold, false);
            Transform lid = Part("CoffinLid_Moving", PrimitiveType.Cube, coffin,
                new Vector3(0f, 0.88f, 0f), new Vector3(1.92f, 0.18f, 3.42f), iron, true);
            Part("LidCross", PrimitiveType.Cube, lid, new Vector3(0f, 0.60f, 0f),
                new Vector3(0.22f, 2.7f, 0.25f), gold, false);
            Part("LidCrossbar", PrimitiveType.Cube, lid, new Vector3(0f, 0.60f, 0f),
                new Vector3(1.05f, 0.22f, 0.25f), gold, false);

            CreateCandle(content, new Vector3(-2.0f, 0.58f, 0.6f), gold, blood);
            CreateCandle(content, new Vector3(2.0f, 0.58f, 0.6f), gold, blood);
            Rigidbody rose = CreateBlackRose(content, new Vector3(0f, 1.02f, 1.25f), iron, blood);
            Transform feedback = Feedback(content, "CoffinFalseFeedback", new Vector3(0f, 2.8f, 4.72f),
                "FALSE RELIC\nNOTHING OF THE MOON REMAINS HERE.", new Color(1f, 0.26f, 0.22f), stone);
            state.Configure(state.QuestController, 2, feedback.gameObject);

            Transform button = Part("OpenCoffinButton", PrimitiveType.Cylinder, content,
                new Vector3(-2.25f, 1.15f, 1.20f), new Vector3(0.38f, 0.16f, 0.38f), gold, true);
            button.localRotation = Quaternion.Euler(90f, 0f, 0f);
            button.gameObject.AddComponent<XRSimpleInteractable>();
            Text("OpenLabel", content, new Vector3(-2.25f, 1.78f, 1.20f), new Vector2(1.6f, 0.42f),
                1.1f, "OPEN", new Color(1f, 0.76f, 0.28f));
            ManorChamberReveal reveal = button.gameObject.AddComponent<ManorChamberReveal>();
            reveal.Configure(state, lid, lid.localPosition + new Vector3(0f, 0.9f, 0.75f),
                new Vector3(-58f, 0f, 0f), 1.15f, rose);
            EditorUtility.SetDirty(state);
            EditorUtility.SetDirty(reveal);
        }

        private static void BuildPortrait(Transform chamber, ManorChamberInteraction state,
            Material stone, Material wood, Material gold, Material glow)
        {
            ReplaceGroup(chamber, "Content_Section05", out Transform content);
            PointLight("Portrait_MoonLight", content, new Vector3(0f, 3.45f, 1.5f),
                new Color(0.36f, 0.58f, 1f), 120f, 6f);

            for (int i = -1; i <= 1; i++)
            {
                Transform portrait = Group($"Portrait_{i + 2}", content);
                portrait.localPosition = new Vector3(i * 2.25f, 2.55f, 4.72f);
                Part("Frame", PrimitiveType.Cube, portrait, Vector3.zero,
                    new Vector3(1.55f, 2.25f, 0.14f), gold, false);
                Part("Canvas", PrimitiveType.Cube, portrait, new Vector3(0f, 0f, -0.09f),
                    new Vector3(1.28f, 1.98f, 0.08f), i == 0 ? stone : wood, false);
            }

            Transform cover = Part("MoonClueCover_Moving", PrimitiveType.Cube, content,
                new Vector3(0f, 2.55f, 4.54f), new Vector3(1.12f, 1.72f, 0.10f), wood, true);
            Transform clue = Feedback(content, "PortraitMoonClue", new Vector3(0f, 2.55f, 4.44f),
                "THE TRUE CHAMBER\nBEARS THE MARK OF THE MOON.", new Color(0.58f, 0.88f, 1f), stone);
            CreateMoonSymbol(clue, new Vector3(0f, 0.73f, -0.10f), glow, gold);
            state.Configure(state.QuestController, 3, clue.gameObject);

            Transform eye = Part("GlowingEyeButton", PrimitiveType.Sphere, content,
                new Vector3(0f, 2.62f, 4.28f), new Vector3(0.22f, 0.16f, 0.10f), glow, true);
            eye.gameObject.AddComponent<XRSimpleInteractable>();
            Text("PressEyeLabel", content, new Vector3(0f, 1.30f, 4.42f), new Vector2(2.4f, 0.45f),
                0.92f, "PRESS THE WATCHER EYE", new Color(0.58f, 0.88f, 1f));
            ManorChamberReveal reveal = eye.gameObject.AddComponent<ManorChamberReveal>();
            reveal.Configure(state, cover, cover.localPosition + new Vector3(1.55f, 0f, 0f),
                Vector3.zero, 1.0f, null);
            EditorUtility.SetDirty(state);
            EditorUtility.SetDirty(reveal);
        }

        private static Rigidbody CreateBlackRose(Transform parent, Vector3 position, Material iron, Material blood)
        {
            Transform root = Group("BlackRose_RedHerring", parent);
            root.localPosition = position;
            root.localRotation = Quaternion.Euler(0f, 0f, -18f);
            CapsuleCollider collider = root.gameObject.AddComponent<CapsuleCollider>();
            collider.height = 1.20f;
            collider.radius = 0.24f;
            Part("Stem", PrimitiveType.Cylinder, root, Vector3.zero, new Vector3(0.07f, 0.55f, 0.07f), iron, false);
            for (int i = 0; i < 7; i++)
            {
                float angle = i * Mathf.PI * 2f / 7f;
                Transform petal = Part($"Petal_{i + 1}", PrimitiveType.Sphere, root,
                    new Vector3(Mathf.Cos(angle) * 0.16f, 0.62f, Mathf.Sin(angle) * 0.16f),
                    new Vector3(0.20f, 0.10f, 0.14f), blood, false);
                petal.localRotation = Quaternion.Euler(0f, -angle * Mathf.Rad2Deg, 24f);
            }
            Part("RoseHeart", PrimitiveType.Sphere, root, new Vector3(0f, 0.63f, 0f),
                Vector3.one * 0.19f, iron, false);
            Rigidbody body = root.gameObject.AddComponent<Rigidbody>();
            body.mass = 0.55f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            root.gameObject.AddComponent<XRGrabInteractable>().useDynamicAttach = true;
            ManorKeyArtifact artifact = root.gameObject.AddComponent<ManorKeyArtifact>();
            artifact.Configure("BlackRose_RedHerring");
            return body;
        }

        private static void CreateCandle(Transform parent, Vector3 position, Material baseMat, Material flameMat)
        {
            Transform root = Group("Candle", parent);
            root.localPosition = position;
            Part("Stand", PrimitiveType.Cylinder, root, Vector3.zero, new Vector3(0.22f, 0.12f, 0.22f), baseMat, false);
            Part("Wax", PrimitiveType.Cylinder, root, new Vector3(0f, 0.42f, 0f), new Vector3(0.12f, 0.42f, 0.12f), baseMat, false);
            Part("Flame", PrimitiveType.Sphere, root, new Vector3(0f, 0.92f, 0f), new Vector3(0.10f, 0.20f, 0.10f), flameMat, false);
        }

        private static void CreateMoonSymbol(Transform parent, Vector3 position, Material moon, Material gold)
        {
            Transform root = Group("MoonSymbol", parent);
            root.localPosition = position;
            for (int i = 0; i < 14; i++)
            {
                float angle = Mathf.Lerp(-125f, 125f, i / 13f) * Mathf.Deg2Rad;
                Part($"Arc_{i + 1}", PrimitiveType.Sphere, root,
                    new Vector3(Mathf.Cos(angle) * 0.40f, Mathf.Sin(angle) * 0.40f, 0f),
                    Vector3.one * 0.10f, i % 2 == 0 ? moon : gold, false);
            }
        }

        private static Transform Feedback(Transform parent, string name, Vector3 position, string message, Color color, Material backing)
        {
            Transform root = Group(name, parent);
            root.localPosition = position;
            Part("Backing", PrimitiveType.Cube, root, Vector3.zero, new Vector3(4.9f, 1.28f, 0.10f), backing, false);
            Text("Message", root, new Vector3(0f, 0f, -0.065f), new Vector2(4.55f, 1.0f), 2.1f, message, color);
            return root;
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
            text.fontSizeMin = fontSize * 0.58f;
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

        private static void ReplaceGroup(Transform parent, string name, out Transform result)
        {
            Transform old = parent.Find(name);
            if (old != null) Object.DestroyImmediate(old.gameObject);
            result = Group(name, parent);
        }

        private static Transform Group(string name, Transform parent)
        {
            Transform result = new GameObject(name).transform;
            result.SetParent(parent, false);
            return result;
        }

        private static Material Mat(string name) => AssetDatabase.LoadAssetAtPath<Material>($"Assets/MichaelManor/Materials/{name}.mat");
        private static Transform Find(Scene scene, string name) => scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(true)).First(item => item.name == name);
    }
}
