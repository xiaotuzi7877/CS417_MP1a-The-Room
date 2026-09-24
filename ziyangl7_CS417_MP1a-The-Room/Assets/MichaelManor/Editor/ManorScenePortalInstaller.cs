using System.IO;
using System.Linq;
using MichaelManor;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

namespace MichaelManorEditor
{
    /// Connected Scenes: a glowing portal in the opened exit doorway that loads Minh's room, plus
    /// a carry prefab for every grabbable so held items follow the player into the next scene.
    public static class ManorScenePortalInstaller
    {
        private const string PrefabFolder = "Assets/MichaelManor/Prefabs/Carry";
        private const string MinhScenePath = "Assets/Scenes/SampleScene.unity";

        [MenuItem("Tools/Michael Manor/Install Minh Room Portal")]
        public static void Install()
        {
            Scene scene = SceneManager.GetActiveScene();
            ManorThreeStagePuzzle ritual = Object.FindFirstObjectByType<ManorThreeStagePuzzle>(FindObjectsInactive.Include);
            Transform puzzleRoot = scene.GetRootGameObjects().FirstOrDefault(r => r.name == "Puzzle")?.transform;
            if (ritual == null || puzzleRoot == null)
            {
                Debug.LogError("Minh portal install stopped: open MichaelManorHall first (ritual or Puzzle root missing).");
                return;
            }

            BuildPortal(puzzleRoot, ritual);
            ManorTextOrientationFixer.Apply();          // before prefabs, so relic labels carry their billboard
            int prefabs = BuildCarryPrefabs();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log($"Installed Minh room portal and {prefabs} carry prefabs.");
        }

        private static void BuildPortal(Transform parent, ManorThreeStagePuzzle ritual)
        {
            Transform old = parent.Find("MinhRoomPortal");
            if (old != null) Object.DestroyImmediate(old.gameObject);
            Transform root = new GameObject("MinhRoomPortal").transform;
            root.SetParent(parent, false);
            root.position = new Vector3(0f, 0f, 15.3f);

            Material glow = Mat("SpectralGlow");
            Material gold = Mat("AntiqueGold");
            Transform swirl = Part("PortalSwirl", PrimitiveType.Cylinder, root, new Vector3(0f, 2.35f, 0.45f),
                new Vector3(3.1f, 0.02f, 3.1f), glow);
            swirl.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var glowRenderers = new System.Collections.Generic.List<Renderer> { swirl.GetComponent<Renderer>() };
            for (int i = 0; i < 4; i++)
            {
                // Spokes sit on the disc so its rotation reads clearly.
                // Gold spokes just in front of the disc (local -y faces the hall) make the spin visible.
                Transform spoke = Part($"SwirlSpoke_{i + 1}", PrimitiveType.Cube, swirl, new Vector3(0f, -3f, 0f),
                    new Vector3(0.95f, 1.5f, 0.035f), gold);
                spoke.localRotation = Quaternion.Euler(0f, i * 45f, 0f);
            }
            for (int i = 0; i < 18; i++)
            {
                float angle = i / 18f * Mathf.PI * 2f;
                Part($"PortalRing_{i + 1:00}", PrimitiveType.Cube, root,
                    new Vector3(Mathf.Cos(angle) * 1.7f, 2.35f + Mathf.Sin(angle) * 1.7f, 0.4f),
                    new Vector3(0.34f, 0.34f, 0.2f), gold).localRotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg);
            }

            var lightObject = new GameObject("PortalLight");
            lightObject.transform.SetParent(root, false);
            lightObject.transform.localPosition = new Vector3(0f, 2.35f, -0.9f);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.6f, 0.4f, 1f);
            light.range = 7f;
            light.intensity = 0f;

            var labelObject = new GameObject("PortalLabel");
            labelObject.transform.SetParent(root, false);
            labelObject.transform.localPosition = new Vector3(0f, 4.75f, -0.1f);
            TextMeshPro label = labelObject.AddComponent<TextMeshPro>();
            label.text = "TO MINH'S ROOM\n<size=60%>WALK IN - HELD ITEMS COME WITH YOU</size>";
            label.fontSize = 2.2f;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.color = new Color(0.85f, 0.75f, 1f);
            label.rectTransform.sizeDelta = new Vector2(4.2f, 1.1f);
            label.enableWordWrapping = false;

            ManorScenePortal portal = root.gameObject.AddComponent<ManorScenePortal>();
            WinCelebrationController celebration = Object.FindFirstObjectByType<WinCelebrationController>(FindObjectsInactive.Include);
            portal.Configure(ritual, celebration, MinhScenePath, swirl, glowRenderers.ToArray(), light, label);
            EditorUtility.SetDirty(portal);
        }

        private static int BuildCarryPrefabs()
        {
            Directory.CreateDirectory(PrefabFolder);
            int count = 0;
            foreach (XRGrabInteractable grab in Object.FindObjectsByType<XRGrabInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (grab.GetComponent<Rigidbody>() == null) continue;
                GameObject original = grab.gameObject;
                ManorCarryableItem marker = original.GetComponent<ManorCarryableItem>();
                if (marker != null) Object.DestroyImmediate(marker);

                GameObject copy = Object.Instantiate(original);
                copy.name = original.name;
                copy.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                copy.transform.localScale = original.transform.lossyScale;
                copy.SetActive(true);
                Sanitize(copy);
                Rigidbody body = copy.GetComponent<Rigidbody>();
                body.isKinematic = false;
                body.useGravity = true;

                string path = $"{PrefabFolder}/{original.name}_Carry.prefab";
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(copy, path);
                Object.DestroyImmediate(copy);

                original.AddComponent<ManorCarryableItem>().Configure(prefab);
                EditorUtility.SetDirty(original);
                count++;
            }
            return count;
        }

        /// Keep only what makes the item look, collide, and grab; drop scene-bound behaviour.
        private static void Sanitize(GameObject root)
        {
            foreach (Camera camera in root.GetComponentsInChildren<Camera>(true)) Object.DestroyImmediate(camera);
            for (int pass = 0; pass < 4; pass++)
            {
                foreach (MonoBehaviour behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (behaviour == null || behaviour is XRGrabInteractable || behaviour is XRGeneralGrabTransformer ||
                        behaviour is TMP_Text || behaviour is ManorKeyArtifact || behaviour is ManorFaceViewer) continue;
                    Object.DestroyImmediate(behaviour);
                }
            }
        }

        private static Transform Part(string name, PrimitiveType type, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            Object.DestroyImmediate(part.GetComponent<Collider>());
            if (material != null) part.GetComponent<Renderer>().sharedMaterial = material;
            return part.transform;
        }

        private static Material Mat(string name) => AssetDatabase.LoadAssetAtPath<Material>($"Assets/MichaelManor/Materials/{name}.mat");
    }
}
