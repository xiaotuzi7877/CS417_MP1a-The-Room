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
    public static class ManorFalseChambersInstaller
    {
        private const string ScenePath = "Assets/Scenes/MichaelManorHall.unity";

        [MenuItem("Tools/Michael Manor/Install Cellar And Bone Closet")]
        public static void Install()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Transform locations = FindSceneObject(scene, "GatedLocations").transform;
            Transform stateRoot = FindSceneObject(scene, "ChamberState").transform;
            Material stone = LoadMaterial("ManorStone");
            Material wood = LoadMaterial("DarkWood");
            Material iron = LoadMaterial("BlackIron");
            Material gold = LoadMaterial("AntiqueGold");
            Material glow = LoadMaterial("SpectralGlow");

            BuildCellar(locations.Find("Chamber_01_Cellar"), stateRoot.Find("State_01_Cellar")
                .GetComponent<ManorChamberInteraction>(), stone, wood, iron, gold, glow);
            BuildBoneCloset(locations.Find("Chamber_02_BoneCloset"), stateRoot.Find("State_02_BoneCloset")
                .GetComponent<ManorChamberInteraction>(), stone, wood, iron, gold, glow);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Installed Cellar Cache and Bone Closet interactions with two physical FALSE RELIC props.");
        }

        private static void BuildCellar(Transform chamber, ManorChamberInteraction state, Material stone,
            Material wood, Material iron, Material gold, Material glow)
        {
            DestroyChildIfPresent(chamber, "Content_Section04");
            Transform content = NewGroup("Content_Section04", chamber);
            CreatePointLight("Cellar_GreenLight", content, new Vector3(0f, 3.5f, 0.5f),
                new Color(0.20f, 0.95f, 0.35f), 120f, 6f);

            CreatePart("Barrel_Left", PrimitiveType.Cylinder, content, new Vector3(-2.7f, 0.65f, 2.7f),
                new Vector3(0.65f, 0.65f, 0.65f), wood, true);
            CreatePart("Barrel_Right", PrimitiveType.Cylinder, content, new Vector3(2.6f, 0.65f, 3.2f),
                new Vector3(0.65f, 0.65f, 0.65f), wood, true);
            CreatePart("Crate_Left", PrimitiveType.Cube, content, new Vector3(-2.6f, 0.45f, 1.1f),
                new Vector3(1.1f, 0.9f, 1.1f), wood, true);

            Transform cache = NewGroup("CellarCache", content);
            CreatePart("CacheFloor", PrimitiveType.Cube, cache, new Vector3(0f, 0.18f, 1.5f),
                new Vector3(1.8f, 0.20f, 1.3f), wood, true);
            CreatePart("CacheWallLeft", PrimitiveType.Cube, cache, new Vector3(-0.84f, 0.50f, 1.5f),
                new Vector3(0.12f, 0.65f, 1.3f), wood, true);
            CreatePart("CacheWallRight", PrimitiveType.Cube, cache, new Vector3(0.84f, 0.50f, 1.5f),
                new Vector3(0.12f, 0.65f, 1.3f), wood, true);
            CreatePart("CacheWallBack", PrimitiveType.Cube, cache, new Vector3(0f, 0.50f, 2.09f),
                new Vector3(1.8f, 0.65f, 0.12f), wood, true);
            CreatePart("CacheWallFront", PrimitiveType.Cube, cache, new Vector3(0f, 0.50f, 0.91f),
                new Vector3(1.8f, 0.65f, 0.12f), wood, true);
            Transform lid = CreatePart("CacheLid_Moving", PrimitiveType.Cube, cache, new Vector3(0f, 0.82f, 1.5f),
                new Vector3(1.9f, 0.14f, 1.4f), iron, true);
            CreatePart("CacheBand", PrimitiveType.Cube, lid, Vector3.zero, new Vector3(0.20f, 1.05f, 1.04f), gold, false);

            Rigidbody rustyKey = CreateRustyKey(content, new Vector3(0f, 0.62f, 1.5f), iron, gold);
            Transform feedback = CreateFeedback(content, "CellarFalseFeedback", new Vector3(0f, 2.6f, 4.75f),
                "FALSE RELIC\nTHE MOON IS NOT HERE.", new Color(0.35f, 1f, 0.48f), stone);
            state.Configure(state.QuestController, 0, feedback.gameObject);

            Transform lever = CreatePart("CellarLever", PrimitiveType.Cube, content, new Vector3(-2.2f, 1.25f, -0.2f),
                new Vector3(0.35f, 0.80f, 0.35f), gold, true);
            GetOrAdd<XRSimpleInteractable>(lever.gameObject);
            ManorChamberReveal reveal = GetOrAdd<ManorChamberReveal>(lever.gameObject);
            reveal.Configure(state, lid, lid.localPosition + new Vector3(0f, 0.85f, 0.45f),
                new Vector3(-65f, 0f, 0f), 1.0f, rustyKey);
            EditorUtility.SetDirty(state);
            EditorUtility.SetDirty(reveal);
        }

        private static void BuildBoneCloset(Transform chamber, ManorChamberInteraction state, Material stone,
            Material wood, Material iron, Material gold, Material glow)
        {
            DestroyChildIfPresent(chamber, "Content_Section04");
            Transform content = NewGroup("Content_Section04", chamber);
            CreatePointLight("Bone_RedLight", content, new Vector3(0f, 3.5f, 0.5f),
                new Color(1f, 0.12f, 0.08f), 120f, 6f);

            CreatePart("Skull", PrimitiveType.Sphere, content, new Vector3(-1.9f, 0.45f, 2.7f),
                new Vector3(0.55f, 0.42f, 0.48f), stone, true);
            for (int i = 0; i < 5; i++)
            {
                Transform bone = CreatePart($"Bone_{i + 1}", PrimitiveType.Capsule, content,
                    new Vector3(-2.7f + i * 1.15f, 0.18f, 3.6f), new Vector3(0.12f, 0.55f, 0.12f), stone, true);
                bone.localRotation = Quaternion.Euler(0f, 0f, 78f - i * 8f);
            }

            Transform compartment = NewGroup("BoneCompartment", content);
            CreatePart("CompartmentBack", PrimitiveType.Cube, compartment, new Vector3(0f, 0.9f, 2.45f),
                new Vector3(1.8f, 1.8f, 0.12f), wood, true);
            CreatePart("CompartmentLeft", PrimitiveType.Cube, compartment, new Vector3(-0.84f, 0.9f, 2.0f),
                new Vector3(0.12f, 1.8f, 1.0f), wood, true);
            CreatePart("CompartmentRight", PrimitiveType.Cube, compartment, new Vector3(0.84f, 0.9f, 2.0f),
                new Vector3(0.12f, 1.8f, 1.0f), wood, true);
            CreatePart("CompartmentFloor", PrimitiveType.Cube, compartment, new Vector3(0f, 0.08f, 2.0f),
                new Vector3(1.8f, 0.12f, 1.0f), wood, true);
            Transform door = CreatePart("CompartmentDoor_Moving", PrimitiveType.Cube, compartment,
                new Vector3(0f, 0.9f, 1.43f), new Vector3(1.65f, 1.55f, 0.12f), iron, true);
            Rigidbody woodenFang = CreateWoodenFang(content, new Vector3(0f, 0.72f, 1.85f), wood, gold);
            Transform feedback = CreateFeedback(content, "BoneFalseFeedback", new Vector3(0f, 2.6f, 4.75f),
                "FALSE RELIC\nWOOD CANNOT BITE.", new Color(1f, 0.30f, 0.20f), stone);
            state.Configure(state.QuestController, 1, feedback.gameObject);

            Transform button = CreatePart("BoneRevealButton", PrimitiveType.Cylinder, content,
                new Vector3(2.2f, 1.20f, 1.3f), new Vector3(0.34f, 0.16f, 0.34f), glow, true);
            button.localRotation = Quaternion.Euler(90f, 0f, 0f);
            GetOrAdd<XRSimpleInteractable>(button.gameObject);
            ManorChamberReveal reveal = GetOrAdd<ManorChamberReveal>(button.gameObject);
            reveal.Configure(state, door, door.localPosition + new Vector3(0.95f, 0f, 0.3f),
                new Vector3(0f, -105f, 0f), 1.0f, woodenFang);
            EditorUtility.SetDirty(state);
            EditorUtility.SetDirty(reveal);
        }

        private static Rigidbody CreateRustyKey(Transform parent, Vector3 position, Material iron, Material gold)
        {
            Transform root = NewGroup("RustyKey_RedHerring", parent);
            root.localPosition = position;
            BoxCollider collider = root.gameObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.55f, 0.18f, 1.15f);
            CreatePart("KeyShaft", PrimitiveType.Cube, root, Vector3.zero, new Vector3(0.12f, 0.10f, 0.85f), iron, false);
            CreatePart("KeyHead", PrimitiveType.Cylinder, root, new Vector3(0f, 0f, 0.48f),
                new Vector3(0.28f, 0.06f, 0.28f), gold, false).localRotation = Quaternion.Euler(90f, 0f, 0f);
            CreatePart("KeyToothA", PrimitiveType.Cube, root, new Vector3(0.18f, 0f, -0.37f),
                new Vector3(0.30f, 0.10f, 0.12f), iron, false);
            CreatePart("KeyToothB", PrimitiveType.Cube, root, new Vector3(-0.15f, 0f, -0.50f),
                new Vector3(0.25f, 0.10f, 0.12f), iron, false);
            return ConfigureRelic(root.gameObject, "RustyKey_RedHerring", 0.65f);
        }

        private static Rigidbody CreateWoodenFang(Transform parent, Vector3 position, Material wood, Material gold)
        {
            Transform root = NewGroup("WoodenFang_RedHerring", parent);
            root.localPosition = position;
            CapsuleCollider collider = root.gameObject.AddComponent<CapsuleCollider>();
            collider.height = 1.05f;
            collider.radius = 0.18f;
            CreatePart("FangBody", PrimitiveType.Capsule, root, Vector3.zero,
                new Vector3(0.22f, 0.52f, 0.22f), wood, false);
            CreatePart("FangGrip", PrimitiveType.Cylinder, root, new Vector3(0f, 0.48f, 0f),
                new Vector3(0.30f, 0.08f, 0.30f), gold, false);
            root.localRotation = Quaternion.Euler(0f, 0f, -18f);
            return ConfigureRelic(root.gameObject, "WoodenFang_RedHerring", 0.40f);
        }

        private static Rigidbody ConfigureRelic(GameObject relic, string id, float mass)
        {
            Rigidbody body = relic.AddComponent<Rigidbody>();
            body.mass = mass;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            relic.AddComponent<XRGrabInteractable>();
            ManorKeyArtifact identity = relic.AddComponent<ManorKeyArtifact>();
            identity.Configure(id);
            return body;
        }

        private static Transform CreateFeedback(Transform parent, string name, Vector3 position, string message, Color color, Material backing)
        {
            Transform root = NewGroup(name, parent);
            root.localPosition = position;
            CreatePart("Backing", PrimitiveType.Cube, root, Vector3.zero, new Vector3(3.8f, 1.15f, 0.10f), backing, false);
            GameObject textObject = new GameObject("Message");
            textObject.transform.SetParent(root, false);
            textObject.transform.localPosition = new Vector3(0f, 0f, -0.065f);
            textObject.transform.localRotation = Quaternion.identity;
            TextMeshPro text = textObject.AddComponent<TextMeshPro>();
            text.text = message;
            text.fontSize = 2.20f;
            text.enableAutoSizing = true;
            text.fontSizeMin = 1.00f;
            text.fontSizeMax = 2.20f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = color;
            text.rectTransform.sizeDelta = new Vector2(3.5f, 0.9f);
            return root;
        }

        private static void CreatePointLight(string name, Transform parent, Vector3 position, Color color, float intensity, float range)
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

        private static Transform CreatePart(string name, PrimitiveType type, Transform parent, Vector3 position,
            Vector3 scale, Material material, bool keepCollider)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            if (material != null) part.GetComponent<Renderer>().sharedMaterial = material;
            if (!keepCollider) UnityEngine.Object.DestroyImmediate(part.GetComponent<Collider>());
            return part.transform;
        }

        private static Transform NewGroup(string name, Transform parent)
        {
            GameObject group = new GameObject(name);
            group.transform.SetParent(parent, false);
            return group.transform;
        }

        private static Material LoadMaterial(string name) => AssetDatabase.LoadAssetAtPath<Material>($"Assets/MichaelManor/Materials/{name}.mat");

        private static GameObject FindSceneObject(Scene scene, string name) => scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(true)).First(transform => transform.name == name).gameObject;

        private static void DestroyChildIfPresent(Transform parent, string name)
        {
            Transform found = parent.Find(name);
            if (found != null) UnityEngine.Object.DestroyImmediate(found.gameObject);
        }

        private static T GetOrAdd<T>(GameObject target) where T : Component
        {
            T found = target.GetComponent<T>();
            return found != null ? found : target.AddComponent<T>();
        }
    }
}
