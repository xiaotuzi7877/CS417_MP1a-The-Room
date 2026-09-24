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
    public static class ManorRedHerringCollectionInstaller
    {
        private const string ScenePath = "Assets/Scenes/MichaelManorHall.unity";
        private static readonly string[] Chambers = { "Chamber_01_Cellar", "Chamber_02_BoneCloset", "Chamber_03_CoffinVault", "Chamber_04_Portrait", "Chamber_05_MoonCrypt" };
        private static readonly string[] Relics = { "CrackedGoblet", "WolfToken", "BrokenCompass", "IronCoin", "RavenIdol", "BoneCharm", "DuskCrystal", "TornCrown", "SunMedallion", "EmptyBloodVial" };

        [MenuItem("Tools/Michael Manor/Install Full Red Herring Collection")]
        public static void Install()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Material[] materials = { Mat("AntiqueGold"), Mat("BlackIron"), Mat("ManorStone"), Mat("BloodVelvet"), Mat("CelestialMoon") };
            for (int chamberIndex = 0; chamberIndex < Chambers.Length; chamberIndex++)
            {
                Transform chamber = Find(scene, Chambers[chamberIndex]);
                if (chamber == null) { Debug.LogError("Missing " + Chambers[chamberIndex]); return; }
                Transform old = chamber.Find("InspectionRedHerrings"); if (old != null) Object.DestroyImmediate(old.gameObject);
                Transform group = new GameObject("InspectionRedHerrings").transform; group.SetParent(chamber, false);
                for (int slot = 0; slot < 2; slot++)
                {
                    int index = chamberIndex * 2 + slot;
                    CreateRelic(group, index, new Vector3(slot == 0 ? -2.7f : 2.7f, 0.75f, -0.65f + chamberIndex * 0.12f), materials[index % materials.Length]);
                }
            }
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            Debug.Log("Installed ten additional unique false relics for thirteen total Red Herrings.");
        }

        [MenuItem("Tools/Michael Manor/Test Full Red Herring Collection (Play Mode)")]
        public static void TestInPlayMode()
        {
            if (!Application.isPlaying) { Debug.LogError("Enter Play Mode before testing Red Herrings."); return; }
            ManorKeyArtifact[] relics = Object.FindObjectsByType<ManorKeyArtifact>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(a => a.ArtifactId.Contains("RedHerring")).ToArray();
            bool valid = relics.Length == 13 && relics.Select(r => r.ArtifactId).Distinct().Count() == 13 &&
                         relics.All(r => r.GetComponent<Rigidbody>() != null && r.GetComponent<Collider>() != null && r.GetComponent<XRGrabInteractable>() != null);
            if (valid) Debug.Log("RED HERRING COLLECTION PASS: thirteen unique Rigidbody + Collider + XRGrab false relics found.");
            else Debug.LogError("Red Herring collection test failed; found " + relics.Length + " qualifying relics.");
        }

        private static void CreateRelic(Transform parent, int index, Vector3 position, Material material)
        {
            string name = Relics[index] + "_RedHerring";
            GameObject root = new GameObject(name); root.transform.SetParent(parent, false); root.transform.localPosition = position;
            BoxCollider collider = root.AddComponent<BoxCollider>(); collider.size = new Vector3(0.65f, 1f, 0.65f);
            Rigidbody body = root.AddComponent<Rigidbody>(); body.mass = 0.30f + index * 0.045f; body.interpolation = RigidbodyInterpolation.Interpolate; body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            root.AddComponent<XRGrabInteractable>(); root.AddComponent<ManorKeyArtifact>().Configure(name);

            PrimitiveType bodyType = index % 3 == 0 ? PrimitiveType.Cylinder : index % 3 == 1 ? PrimitiveType.Sphere : PrimitiveType.Cube;
            Part("RelicBody", bodyType, root.transform, Vector3.zero, new Vector3(0.38f + (index % 2) * 0.12f, 0.48f, 0.30f), material);
            Part("RelicCrest", index % 2 == 0 ? PrimitiveType.Capsule : PrimitiveType.Cylinder, root.transform,
                new Vector3(0f, 0.52f, 0f), new Vector3(0.16f, 0.28f + (index % 4) * 0.04f, 0.16f), Mat(index % 2 == 0 ? "AntiqueGold" : "BloodVelvet"));
            GameObject labelObject = new GameObject("FalseRelicLabel"); labelObject.transform.SetParent(root.transform, false);
            labelObject.transform.localPosition = new Vector3(0f, 0.92f, 0f); labelObject.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            TextMeshPro label = labelObject.AddComponent<TextMeshPro>(); label.text = "FALSE RELIC"; label.fontSize = 0.16f; label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center; label.color = new Color(1f, 0.24f, 0.18f); label.rectTransform.sizeDelta = new Vector2(1.2f, 0.3f);
        }

        private static void Part(string name, PrimitiveType type, Transform parent, Vector3 position, Vector3 scale, Material material)
        { GameObject p = GameObject.CreatePrimitive(type); p.name = name; p.transform.SetParent(parent, false); p.transform.localPosition = position; p.transform.localScale = scale; Object.DestroyImmediate(p.GetComponent<Collider>()); if (material != null) p.GetComponent<Renderer>().sharedMaterial = material; }
        private static Material Mat(string name) => AssetDatabase.LoadAssetAtPath<Material>("Assets/MichaelManor/Materials/" + name + ".mat");
        private static Transform Find(Scene scene, string name) => scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).FirstOrDefault(t => t.name == name);
    }
}
