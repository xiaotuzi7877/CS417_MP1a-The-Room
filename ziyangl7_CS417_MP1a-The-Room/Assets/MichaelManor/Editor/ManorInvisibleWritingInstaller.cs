using System.Linq;
using MichaelManor;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MichaelManorEditor
{
    public static class ManorInvisibleWritingInstaller
    {
        private const string ScenePath = "Assets/Scenes/MichaelManorHall.unity";
        private const string MaterialPath = "Assets/MichaelManor/Materials/AngleRevealWriting.mat";

        [MenuItem("Tools/Michael Manor/Install Angle Revealed Writing")]
        public static void Install()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Transform decor = Find(scene, "Decor"); if (decor == null) { Debug.LogError("Decor root missing."); return; }
            Transform old = decor.Find("InvisibleWritingInspection"); if (old != null) Object.DestroyImmediate(old.gameObject);
            Transform root = new GameObject("InvisibleWritingInspection").transform; root.SetParent(decor, false);
            root.SetPositionAndRotation(new Vector3(7.72f, 2.35f, 5.8f), Quaternion.Euler(0f, -90f, 0f));

            GameObject backing = GameObject.CreatePrimitive(PrimitiveType.Cube); backing.name = "ObliqueWritingPlaque";
            backing.transform.SetParent(root, false); backing.transform.localScale = new Vector3(4.4f, 1.5f, 0.10f);
            backing.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/MichaelManor/Materials/BlackIron.mat");

            TextMeshPro instruction = Text(root, "InspectionInstruction", new Vector3(0f, 0.48f, -0.065f), "VIEW FROM THE SIDE", 0.28f, new Color(1f,0.72f,0.25f));
            TextMeshPro hidden = Text(root, "AngleHiddenWriting", new Vector3(0f, -0.14f, -0.068f), "THE RAVEN WATCHES\nTHE LEFT LEVER", 0.44f, Color.white);
            Shader shader = Shader.Find("MichaelManor/AngleRevealText");
            if (shader == null) { Debug.LogError("AngleRevealText shader missing."); return; }
            Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null) { material = new Material(shader); AssetDatabase.CreateAsset(material, MaterialPath); }
            material.shader = shader; material.SetTexture("_MainTex", hidden.fontMaterial.GetTexture("_MainTex"));
            material.SetColor("_FaceColor", new Color(0.42f,0.92f,1f,1f)); EditorUtility.SetDirty(material);
            hidden.fontSharedMaterial = material;
            root.gameObject.AddComponent<ManorAngleRevealMarker>().Configure(hidden);
            EditorSceneManager.MarkSceneDirty(scene); ManorTextOrientationFixer.Apply(); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log("Installed fragment-shader angle revealed writing inspection plaque.");
        }

        [MenuItem("Tools/Michael Manor/Test Angle Revealed Writing (Play Mode)")]
        public static void TestInPlayMode()
        {
            if (!Application.isPlaying) { Debug.LogError("Enter Play Mode before testing invisible writing."); return; }
            ManorAngleRevealMarker marker = Object.FindFirstObjectByType<ManorAngleRevealMarker>(FindObjectsInactive.Include);
            bool valid = marker != null && marker.HiddenWriting != null && marker.HiddenWriting.fontSharedMaterial != null &&
                         marker.HiddenWriting.fontSharedMaterial.shader.name == "MichaelManor/AngleRevealText";
            if (valid)
            {
                float front = marker.VisibilityFrom(marker.transform.position + marker.transform.forward * 3f);
                float side = marker.VisibilityFrom(marker.transform.position + marker.transform.right * 3f);
                valid = front < 0.05f && side > 0.95f;
            }
            if (valid) Debug.Log("INVISIBLE WRITING PASS: fragment shader hides frontal text and reveals it from an oblique angle.");
            else Debug.LogError("Invisible Writing test failed.");
        }

        private static TextMeshPro Text(Transform parent,string name,Vector3 position,string value,float size,Color color)
        { GameObject g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.localPosition=position;TextMeshPro t=g.AddComponent<TextMeshPro>();t.text=value;t.fontSize=size;t.fontStyle=FontStyles.Bold;t.alignment=TextAlignmentOptions.Center;t.color=color;t.rectTransform.sizeDelta=new Vector2(4.1f,0.9f);return t; }
        private static Transform Find(Scene scene,string name)=>scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).FirstOrDefault(t=>t.name==name);
    }
}
