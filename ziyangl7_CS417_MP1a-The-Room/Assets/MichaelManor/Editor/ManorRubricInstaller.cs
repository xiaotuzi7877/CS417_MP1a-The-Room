using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MichaelManorEditor
{
    public static class ManorRubricInstaller
    {
        private const string ScenePath = "Assets/Scenes/MichaelManorHall.unity";
        private const string OutlineMaterialPath = "Assets/Materials/OutlineMaterial.mat";
        private const string PresentationRootName = "Rubric_Presentation";

        [MenuItem("Tools/Michael Manor/Install Rubric Presentation")]
        public static void InstallPresentationRequirements()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Transform systems = FindOrCreateRoot(scene, "Systems");

            Transform existing = FindSceneObject(scene, PresentationRootName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            Transform presentation = new GameObject(PresentationRootName).transform;
            presentation.SetParent(systems, false);

            CreateControlsCanvas(presentation);
            CreateCentralCeilingLight(presentation);
            ApplySilverFangOutline(scene);
            MakeManorFirstBuildScene();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Installed rubric presentation requirements in MichaelManorHall.");
        }

        private static void CreateControlsCanvas(Transform parent)
        {
            GameObject canvasObject = new GameObject(
                "ControlsCanvas_WorldSpace",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(parent, false);

            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
            canvasRect.position = new Vector3(-8.05f, 2.65f, -10.5f);
            canvasRect.rotation = Quaternion.Euler(0f, 90f, 0f);
            canvasRect.sizeDelta = new Vector2(1000f, 700f);
            canvasRect.localScale = Vector3.one * 0.00235f;

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 12;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 2f;
            scaler.referencePixelsPerUnit = 100f;

            GameObject backgroundObject = new GameObject("Background", typeof(RectTransform), typeof(Image));
            backgroundObject.transform.SetParent(canvasRect, false);
            RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
            Stretch(backgroundRect, Vector2.zero);
            Image background = backgroundObject.GetComponent<Image>();
            background.color = new Color(0.025f, 0.012f, 0.018f, 0.94f);

            GameObject borderObject = new GameObject("Border", typeof(RectTransform), typeof(Image), typeof(Outline));
            borderObject.transform.SetParent(backgroundRect, false);
            RectTransform borderRect = borderObject.GetComponent<RectTransform>();
            Stretch(borderRect, new Vector2(18f, 18f));
            borderObject.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
            Outline border = borderObject.GetComponent<Outline>();
            border.effectColor = new Color(0.70f, 0.34f, 0.08f, 0.95f);
            border.effectDistance = new Vector2(5f, -5f);

            GameObject textObject = new GameObject("ControlsText", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(backgroundRect, false);
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            Stretch(textRect, new Vector2(55f, 42f));

            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            text.text =
                "<size=58><color=#E6B76A>MICHAEL MANOR</color></size>\n" +
                "<size=34><color=#D7C8B6>VR CONTROLS</color></size>\n\n" +
                "<size=29>Grip  -  take the Silver Fang\n" +
                "Place Fang  -  unlock the exit\n" +
                "Right Trigger  -  launch orbiting relic\n" +
                "Left Primary  -  change hall light\n" +
                "Right Secondary  -  outside view / return\n" +
                "Right Primary  -  quit</size>";
            text.color = new Color(0.93f, 0.90f, 0.84f, 1f);
            text.alignment = TextAlignmentOptions.Center;
            text.enableWordWrapping = false;
            text.raycastTarget = false;
        }

        private static void CreateCentralCeilingLight(Transform parent)
        {
            GameObject lightObject = new GameObject("CeilingPointLight_Rubric");
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.position = new Vector3(0f, 10.35f, 0f);

            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.70f, 0.43f);
            light.intensity = 260f;
            light.range = 23f;
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.55f;
        }

        private static void ApplySilverFangOutline(Scene scene)
        {
            Transform silverFang = FindSceneObject(scene, "SilverFang");
            Material outline = AssetDatabase.LoadAssetAtPath<Material>(OutlineMaterialPath);
            if (silverFang == null || outline == null)
            {
                Debug.LogWarning("Could not apply the Silver Fang outline.");
                return;
            }

            foreach (Renderer renderer in silverFang.GetComponentsInChildren<Renderer>(true))
            {
                List<Material> materials = renderer.sharedMaterials
                    .Where(material => material != null && material != outline)
                    .ToList();
                materials.Add(outline);
                renderer.sharedMaterials = materials.ToArray();
            }
        }

        private static void MakeManorFirstBuildScene()
        {
            List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
            EditorBuildSettingsScene manor = scenes.FirstOrDefault(scene => scene.path == ScenePath);
            if (manor == null)
            {
                manor = new EditorBuildSettingsScene(ScenePath, true);
            }

            manor.enabled = true;
            scenes.RemoveAll(scene => scene.path == ScenePath);
            scenes.Insert(0, manor);
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static Transform FindOrCreateRoot(Scene scene, string name)
        {
            GameObject root = scene.GetRootGameObjects().FirstOrDefault(gameObject => gameObject.name == name);
            return root != null ? root.transform : new GameObject(name).transform;
        }

        private static Transform FindSceneObject(Scene scene, string name)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Transform match = root.GetComponentsInChildren<Transform>(true)
                    .FirstOrDefault(child => child.name == name);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private static void Stretch(RectTransform rect, Vector2 inset)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = inset;
            rect.offsetMax = -inset;
        }
    }
}
