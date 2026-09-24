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
    public static class ManorBlacklightInstaller
    {
        private const string ScenePath="Assets/Scenes/MichaelManorHall.unity";
        private const string MaterialPath="Assets/MichaelManor/Materials/BlacklightWriting.mat";
        [MenuItem("Tools/Michael Manor/Install Directional Blacklight Clue")]
        public static void Install()
        {
            Scene scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);Transform decor=Find(scene,"Decor");
            if(decor==null){Debug.LogError("Decor root missing.");return;}Transform old=decor.Find("BlacklightInspection");if(old!=null)Object.DestroyImmediate(old.gameObject);
            Transform root=new GameObject("BlacklightInspection").transform;root.SetParent(decor,false);

            GameObject prop=new GameObject("GrabbableBlacklight");prop.transform.SetParent(root,false);prop.transform.SetPositionAndRotation(new Vector3(2.8f,1.05f,-8.7f),Quaternion.Euler(0f,25f,0f));
            CapsuleCollider collider=prop.AddComponent<CapsuleCollider>();collider.height=0.9f;collider.radius=0.18f;collider.direction=2;
            Rigidbody body=prop.AddComponent<Rigidbody>();body.mass=0.55f;body.interpolation=RigidbodyInterpolation.Interpolate;body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
            prop.AddComponent<XRGrabInteractable>();
            Part("Handle",PrimitiveType.Cylinder,prop.transform,new Vector3(0f,0f,-0.18f),new Vector3(0.18f,0.42f,0.18f),Mat("BlackIron"),Quaternion.Euler(90f,0f,0f));
            Part("UVTube",PrimitiveType.Cylinder,prop.transform,new Vector3(0f,0f,0.30f),new Vector3(0.28f,0.18f,0.28f),Mat("SpectralGlow"),Quaternion.Euler(90f,0f,0f));
            Transform origin=new GameObject("BlacklightBeamOrigin").transform;origin.SetParent(prop.transform,false);origin.localPosition=new Vector3(0f,0f,0.55f);
            Light light=origin.gameObject.AddComponent<Light>();light.type=LightType.Spot;light.color=new Color(0.45f,0.08f,1f);light.intensity=18f;light.range=7f;light.spotAngle=28f;

            Transform plaque=new GameObject("BlacklightHiddenPlaque").transform;plaque.SetParent(root,false);plaque.SetPositionAndRotation(new Vector3(7.72f,2.4f,-1.5f),Quaternion.Euler(0f,-90f,0f));
            Part("PlaqueBacking",PrimitiveType.Cube,plaque,Vector3.zero,new Vector3(4.2f,1.5f,0.10f),Mat("DarkWood"),Quaternion.identity);
            TextMeshPro instruction=Text(plaque,"BlacklightInstruction",new Vector3(0f,0.48f,-0.065f),"POINT THE UV LAMP HERE",0.27f,new Color(0.72f,0.42f,1f));
            TextMeshPro hidden=Text(plaque,"BlacklightHiddenMessage",new Vector3(0f,-0.12f,-0.068f),"THE BLOOD SEAL OBEYS\nLEFT, THEN RIGHT",0.40f,Color.white);
            Shader shader=Shader.Find("MichaelManor/BlacklightRevealText");if(shader==null){Debug.LogError("BlacklightRevealText shader missing.");return;}
            Material material=AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);if(material==null){material=new Material(shader);AssetDatabase.CreateAsset(material,MaterialPath);}
            material.shader=shader;material.SetTexture("_MainTex",hidden.fontMaterial.GetTexture("_MainTex"));material.SetColor("_FaceColor",new Color(0.72f,0.25f,1f,1f));EditorUtility.SetDirty(material);hidden.fontSharedMaterial=material;
            prop.AddComponent<ManorBlacklightController>().Configure(origin,light,hidden,7f);
            EditorSceneManager.MarkSceneDirty(scene);ManorTextOrientationFixer.Apply(); EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Debug.Log("Installed grabbable directional Blacklight and UV-only clue writing.");
        }

        [MenuItem("Tools/Michael Manor/Test Directional Blacklight (Play Mode)")]
        public static void TestInPlayMode()
        {
            if(!Application.isPlaying){Debug.LogError("Enter Play Mode before testing Blacklight.");return;}
            ManorBlacklightController b=Object.FindFirstObjectByType<ManorBlacklightController>(FindObjectsInactive.Include);
            bool valid=b!=null&&b.GetComponent<Rigidbody>()&&b.GetComponent<Collider>()&&b.GetComponent<XRGrabInteractable>()&&b.HiddenMessage!=null&&b.HiddenMessage.fontSharedMaterial.shader.name=="MichaelManor/BlacklightRevealText";
            if(valid){float ahead=b.VisibilityAt(b.BeamOrigin.position+b.BeamOrigin.forward*2f);float behind=b.VisibilityAt(b.BeamOrigin.position-b.BeamOrigin.forward*2f);valid=ahead>0.5f&&behind<0.01f;}
            if(valid)Debug.Log("BLACKLIGHT PASS: grabbable UV prop reveals writing only inside its forward beam.");else Debug.LogError("Blacklight test failed.");
        }
        private static void Part(string n,PrimitiveType type,Transform p,Vector3 pos,Vector3 scale,Material m,Quaternion rot){GameObject g=GameObject.CreatePrimitive(type);g.name=n;g.transform.SetParent(p,false);g.transform.localPosition=pos;g.transform.localRotation=rot;g.transform.localScale=scale;Object.DestroyImmediate(g.GetComponent<Collider>());if(m!=null)g.GetComponent<Renderer>().sharedMaterial=m;}
        private static TextMeshPro Text(Transform p,string n,Vector3 pos,string value,float size,Color c){GameObject g=new GameObject(n);g.transform.SetParent(p,false);g.transform.localPosition=pos;TextMeshPro t=g.AddComponent<TextMeshPro>();t.text=value;t.fontSize=size;t.fontStyle=FontStyles.Bold;t.alignment=TextAlignmentOptions.Center;t.color=c;t.rectTransform.sizeDelta=new Vector2(4f,0.9f);return t;}
        private static Material Mat(string n)=>AssetDatabase.LoadAssetAtPath<Material>("Assets/MichaelManor/Materials/"+n+".mat");
        private static Transform Find(Scene s,string n)=>s.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).FirstOrDefault(t=>t.name==n);
    }
}
