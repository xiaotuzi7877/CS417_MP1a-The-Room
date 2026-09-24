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
    public static class ManorHandMirrorInstaller
    {
        private const string ScenePath="Assets/Scenes/MichaelManorHall.unity";
        private const string TexturePath="Assets/MichaelManor/Materials/HandMirrorView.renderTexture";
        private const string MaterialPath="Assets/MichaelManor/Materials/HandMirrorSurface.mat";

        [MenuItem("Tools/Michael Manor/Install Reversed Handmirror")]
        public static void Install()
        {
            Scene scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);Transform decor=Find(scene,"Decor");
            if(decor==null){Debug.LogError("Decor root missing.");return;}Transform old=decor.Find("HandmirrorInspection");if(old!=null)Object.DestroyImmediate(old.gameObject);
            Transform group=new GameObject("HandmirrorInspection").transform;group.SetParent(decor,false);
            GameObject root=new GameObject("GrabbableHandmirror");root.transform.SetParent(group,false);root.transform.SetPositionAndRotation(new Vector3(-2.3f,1.1f,-7.9f),Quaternion.Euler(0f,180f,0f));
            BoxCollider collider=root.AddComponent<BoxCollider>();collider.size=new Vector3(0.95f,1.65f,0.20f);
            Rigidbody body=root.AddComponent<Rigidbody>();body.mass=0.65f;body.interpolation=RigidbodyInterpolation.Interpolate;body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
            root.AddComponent<XRGrabInteractable>();
            Part("MirrorFrame",PrimitiveType.Cylinder,root.transform,new Vector3(0f,0.32f,0f),new Vector3(0.58f,0.08f,0.58f),Mat("AntiqueGold"),Quaternion.Euler(90f,0f,0f));
            Part("MirrorHandle",PrimitiveType.Cylinder,root.transform,new Vector3(0f,-0.58f,0f),new Vector3(0.13f,0.52f,0.13f),Mat("DarkWood"),Quaternion.identity);
            GameObject surfaceObject=GameObject.CreatePrimitive(PrimitiveType.Quad);surfaceObject.name="ReversedMirrorSurface";surfaceObject.transform.SetParent(root.transform,false);surfaceObject.transform.localPosition=new Vector3(0f,0.32f,-0.095f);surfaceObject.transform.localRotation=Quaternion.Euler(0f,180f,0f);surfaceObject.transform.localScale=new Vector3(0.92f,0.92f,1f);Object.DestroyImmediate(surfaceObject.GetComponent<Collider>());

            RenderTexture texture=AssetDatabase.LoadAssetAtPath<RenderTexture>(TexturePath);
            if(texture==null){texture=new RenderTexture(512,512,16,RenderTextureFormat.ARGB32);texture.name="HandMirrorView";AssetDatabase.CreateAsset(texture,TexturePath);}
            Shader shader=Shader.Find("Universal Render Pipeline/Unlit");Material material=AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if(material==null){material=new Material(shader);AssetDatabase.CreateAsset(material,MaterialPath);}material.shader=shader;material.SetTexture("_BaseMap",texture);material.SetTextureScale("_BaseMap",new Vector2(-1f,1f));material.SetTextureOffset("_BaseMap",new Vector2(1f,0f));EditorUtility.SetDirty(material);
            Renderer surface=surfaceObject.GetComponent<Renderer>();surface.sharedMaterial=material;
            GameObject cameraObject=new GameObject("MirrorCamera");cameraObject.transform.SetParent(root.transform,false);cameraObject.transform.localPosition=new Vector3(0f,0.32f,-0.16f);cameraObject.transform.localRotation=Quaternion.Euler(0f,180f,0f);
            Camera camera=cameraObject.AddComponent<Camera>();camera.fieldOfView=58f;camera.nearClipPlane=0.08f;camera.farClipPlane=40f;camera.targetTexture=texture;camera.depth=-5f;
            root.AddComponent<ManorHandMirror>().Configure(camera,surface,texture);

            TextMeshPro clue=Text(group,"ReversedMirrorClue",new Vector3(-7.70f,2.2f,1.8f),"THGIR NEHT ,TFEL",0.38f,new Color(0.68f,0.88f,1f));clue.transform.rotation=Quaternion.Euler(0f,90f,0f);
            Text(group,"HandmirrorLabel",new Vector3(-2.3f,2.05f,-7.9f),"HANDMIRROR\nREAD THE REVERSED CLUE",0.28f,new Color(1f,0.72f,0.28f));
            EditorSceneManager.MarkSceneDirty(scene);ManorTextOrientationFixer.Apply(); EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Debug.Log("Installed grabbable RenderTexture Handmirror with horizontally reversed image.");
        }

        [MenuItem("Tools/Michael Manor/Test Reversed Handmirror (Play Mode)")]
        public static void TestInPlayMode()
        {
            if(!Application.isPlaying){Debug.LogError("Enter Play Mode before testing Handmirror.");return;}
            ManorHandMirror mirror=Object.FindFirstObjectByType<ManorHandMirror>(FindObjectsInactive.Include);
            bool valid=mirror!=null&&mirror.GetComponent<Rigidbody>()&&mirror.GetComponent<Collider>()&&mirror.GetComponent<XRGrabInteractable>()&&mirror.MirrorCamera!=null&&mirror.RenderTexture!=null&&mirror.MirrorCamera.targetTexture==mirror.RenderTexture&&mirror.MirrorSurface!=null;
            if(valid){Material m=mirror.MirrorSurface.sharedMaterial;valid=m.GetTexture("_BaseMap")==mirror.RenderTexture&&m.GetTextureScale("_BaseMap").x<0f;}
            if(valid)Debug.Log("HANDMIRROR PASS: grabbable camera feeds a RenderTexture displayed with horizontal reversal.");else Debug.LogError("Handmirror test failed.");
        }
        private static void Part(string n,PrimitiveType type,Transform p,Vector3 pos,Vector3 scale,Material m,Quaternion rot){GameObject g=GameObject.CreatePrimitive(type);g.name=n;g.transform.SetParent(p,false);g.transform.localPosition=pos;g.transform.localRotation=rot;g.transform.localScale=scale;Object.DestroyImmediate(g.GetComponent<Collider>());if(m!=null)g.GetComponent<Renderer>().sharedMaterial=m;}
        private static TextMeshPro Text(Transform p,string n,Vector3 world,string value,float size,Color c){GameObject g=new GameObject(n);g.transform.SetParent(p,true);g.transform.position=world;g.transform.rotation=Quaternion.Euler(0f,180f,0f);TextMeshPro t=g.AddComponent<TextMeshPro>();t.text=value;t.fontSize=size;t.fontStyle=FontStyles.Bold;t.alignment=TextAlignmentOptions.Center;t.color=c;t.rectTransform.sizeDelta=new Vector2(3.8f,0.8f);return t;}
        private static Material Mat(string n)=>AssetDatabase.LoadAssetAtPath<Material>("Assets/MichaelManor/Materials/"+n+".mat");
        private static Transform Find(Scene s,string n)=>s.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).FirstOrDefault(t=>t.name==n);
    }
}
