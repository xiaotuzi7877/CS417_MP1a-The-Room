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
    public static class ManorMagnifyingGlassInstaller
    {
        private const string ScenePath="Assets/Scenes/MichaelManorHall.unity";
        private const string TexturePath="Assets/MichaelManor/Materials/MagnifyingLensView.renderTexture";
        private const string MaterialPath="Assets/MichaelManor/Materials/MagnifyingLensSurface.mat";

        [MenuItem("Tools/Michael Manor/Install VR Magnifying Glass")]
        public static void Install()
        {
            Scene scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);Transform decor=Find(scene,"Decor");
            if(decor==null){Debug.LogError("Decor root missing.");return;}Transform old=decor.Find("MagnifyingGlassInspection");if(old!=null)Object.DestroyImmediate(old.gameObject);
            Transform group=new GameObject("MagnifyingGlassInspection").transform;group.SetParent(decor,false);
            GameObject root=new GameObject("GrabbableMagnifyingGlass");root.transform.SetParent(group,false);root.transform.SetPositionAndRotation(new Vector3(2.1f,1.05f,-6.8f),Quaternion.Euler(0f,180f,0f));
            BoxCollider collider=root.AddComponent<BoxCollider>();collider.size=new Vector3(0.95f,1.65f,0.18f);
            Rigidbody body=root.AddComponent<Rigidbody>();body.mass=0.58f;body.interpolation=RigidbodyInterpolation.Interpolate;body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
            root.AddComponent<XRGrabInteractable>();
            Part("LensRim",PrimitiveType.Cylinder,root.transform,new Vector3(0f,0.34f,0f),new Vector3(0.60f,0.07f,0.60f),Mat("AntiqueGold"),Quaternion.Euler(90f,0f,0f));
            Part("LensHandle",PrimitiveType.Cylinder,root.transform,new Vector3(0f,-0.58f,0f),new Vector3(0.12f,0.55f,0.12f),Mat("DarkWood"),Quaternion.identity);
            GameObject surfaceObject=GameObject.CreatePrimitive(PrimitiveType.Quad);surfaceObject.name="MagnifyingLensSurface";surfaceObject.transform.SetParent(root.transform,false);surfaceObject.transform.localPosition=new Vector3(0f,0.34f,-0.08f);surfaceObject.transform.localRotation=Quaternion.Euler(0f,180f,0f);surfaceObject.transform.localScale=new Vector3(0.94f,0.94f,1f);Object.DestroyImmediate(surfaceObject.GetComponent<Collider>());

            RenderTexture texture=AssetDatabase.LoadAssetAtPath<RenderTexture>(TexturePath);if(texture==null){texture=new RenderTexture(512,512,16,RenderTextureFormat.ARGB32);texture.name="MagnifyingLensView";AssetDatabase.CreateAsset(texture,TexturePath);}
            Shader shader=Shader.Find("Universal Render Pipeline/Unlit");Material material=AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);if(material==null){material=new Material(shader);AssetDatabase.CreateAsset(material,MaterialPath);}
            material.shader=shader;material.SetTexture("_BaseMap",texture);material.SetTextureScale("_BaseMap",Vector2.one);material.SetTextureOffset("_BaseMap",Vector2.zero);EditorUtility.SetDirty(material);
            Renderer surface=surfaceObject.GetComponent<Renderer>();surface.sharedMaterial=material;
            GameObject cameraObject=new GameObject("MagnifyingCamera");cameraObject.transform.SetParent(root.transform,false);cameraObject.transform.localPosition=new Vector3(0f,0.34f,-0.15f);cameraObject.transform.localRotation=Quaternion.Euler(0f,180f,0f);
            Camera camera=cameraObject.AddComponent<Camera>();camera.fieldOfView=22f;camera.nearClipPlane=0.08f;camera.farClipPlane=35f;camera.targetTexture=texture;camera.depth=-6f;
            root.AddComponent<ManorMagnifyingGlass>().Configure(camera,surface,texture);
            Text(group,"MagnifierLabel",new Vector3(2.1f,2.05f,-6.8f),"MAGNIFYING GLASS\nNARROW 22 DEGREE VIEW",0.27f,new Color(0.52f,0.92f,1f));
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Debug.Log("Installed grabbable RenderTexture magnifying glass with narrow field of view.");
        }

        [MenuItem("Tools/Michael Manor/Test VR Magnifying Glass (Play Mode)")]
        public static void TestInPlayMode()
        {
            if(!Application.isPlaying){Debug.LogError("Enter Play Mode before testing Magnifying Glass.");return;}
            ManorMagnifyingGlass lens=Object.FindFirstObjectByType<ManorMagnifyingGlass>(FindObjectsInactive.Include);
            bool valid=lens!=null&&lens.GetComponent<Rigidbody>()&&lens.GetComponent<Collider>()&&lens.GetComponent<XRGrabInteractable>()&&lens.LensCamera!=null&&lens.RenderTexture!=null&&lens.LensCamera.targetTexture==lens.RenderTexture&&lens.LensCamera.fieldOfView<=25f&&lens.LensSurface!=null&&lens.LensSurface.sharedMaterial.GetTexture("_BaseMap")==lens.RenderTexture;
            if(valid)Debug.Log("MAGNIFYING GLASS PASS: grabbable 22-degree camera feeds its lens RenderTexture for visible magnification.");else Debug.LogError("Magnifying Glass test failed.");
        }
        private static void Part(string n,PrimitiveType type,Transform p,Vector3 pos,Vector3 scale,Material m,Quaternion rot){GameObject g=GameObject.CreatePrimitive(type);g.name=n;g.transform.SetParent(p,false);g.transform.localPosition=pos;g.transform.localRotation=rot;g.transform.localScale=scale;Object.DestroyImmediate(g.GetComponent<Collider>());if(m!=null)g.GetComponent<Renderer>().sharedMaterial=m;}
        private static TextMeshPro Text(Transform p,string n,Vector3 world,string value,float size,Color c){GameObject g=new GameObject(n);g.transform.SetParent(p,true);g.transform.position=world;g.transform.rotation=Quaternion.Euler(0f,180f,0f);TextMeshPro t=g.AddComponent<TextMeshPro>();t.text=value;t.fontSize=size;t.fontStyle=FontStyles.Bold;t.alignment=TextAlignmentOptions.Center;t.color=c;t.rectTransform.sizeDelta=new Vector2(3.6f,0.8f);return t;}
        private static Material Mat(string n)=>AssetDatabase.LoadAssetAtPath<Material>("Assets/MichaelManor/Materials/"+n+".mat");
        private static Transform Find(Scene s,string n)=>s.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).FirstOrDefault(t=>t.name==n);
    }
}
