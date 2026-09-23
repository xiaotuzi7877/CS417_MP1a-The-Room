using System.Collections.Generic;
using System.Linq;
using MichaelManor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace MichaelManorEditor
{
    /// <summary>
    /// Keeps the Silver Fang presentation independent from the full-room builder so the
    /// artifact can be upgraded without rebuilding (and overwriting) a hand-edited room.
    /// </summary>
    public static class ManorPuzzleEnhancer
    {
        private const string ScenePath = "Assets/Scenes/MichaelManorHall.unity";
        private const string ModelFolder = "Assets/MichaelManor/Models";
        private const string FangMeshPath = ModelFolder + "/SilverFangBlade.asset";
        private const string OutlineMaterialPath = "Assets/Materials/OutlineMaterial.mat";

        [MenuItem("Tools/Michael Manor/Enhance Silver Fang Artifact")]
        public static void EnhanceSilverFangArtifact()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject fang = FindSceneObject(scene, "SilverFang");
            if (fang == null)
            {
                Debug.LogError("SilverFang was not found in MichaelManorHall.");
                return;
            }

            Material silver = LoadMaterial("SilverFang");
            Material gold = LoadMaterial("AntiqueGold");
            Material blackIron = LoadMaterial("BlackIron");
            Material moonstone = LoadMaterial("SpectralGlow");
            Material outline = AssetDatabase.LoadAssetAtPath<Material>(OutlineMaterialPath);

            BuildSilverFangVisual(fang.transform, silver, gold, blackIron, moonstone, outline);

            fang.transform.localPosition = new Vector3(-5.2f, 1.65f, -10.5f);
            fang.transform.localRotation = Quaternion.Euler(8f, -20f, -12f);
            fang.transform.localScale = Vector3.one;

            Rigidbody body = fang.GetComponent<Rigidbody>();
            if (body != null)
            {
                body.mass = 0.35f;
                body.centerOfMass = new Vector3(0f, -0.08f, 0f);
                body.interpolation = RigidbodyInterpolation.Interpolate;
                body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }

            XRGrabInteractable grab = fang.GetComponent<XRGrabInteractable>();
            if (grab != null)
            {
                grab.attachTransform = fang.transform.Find("GrabAttach");
                grab.useDynamicAttach = false;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Enhanced the Silver Fang with a curved blade, gothic guard, grip, and moonstone.");
        }

        [MenuItem("Tools/Michael Manor/Enhance Silver Fang Pedestal and Finale")]
        public static void EnhanceSilverFangPedestalAndFinale()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject pedestalObject = FindSceneObject(scene, "SilverFang_Pedestal");
            GameObject socketObject = FindSceneObject(scene, "SilverFangSocket");
            GameObject doorObject = FindSceneObject(scene, "ExitDoor");
            if (pedestalObject == null || socketObject == null || doorObject == null)
            {
                Debug.LogError("The Silver Fang pedestal, socket, or exit door is missing.");
                return;
            }

            Material stone = LoadMaterial("ManorStone");
            Material gold = LoadMaterial("AntiqueGold");
            Material blackIron = LoadMaterial("BlackIron");
            Material moonstone = LoadMaterial("SpectralGlow");
            Material silver = LoadMaterial("SilverFang");
            Material outline = AssetDatabase.LoadAssetAtPath<Material>(OutlineMaterialPath);

            Transform model = BuildGothicPedestalModel(
                pedestalObject.transform,
                stone,
                gold,
                blackIron,
                moonstone,
                outline);
            Transform doorSeal = BuildDoorSeal(doorObject.transform, silver, gold, blackIron, moonstone, outline);

            XRSocketInteractor socket = socketObject.GetComponent<XRSocketInteractor>();
            Transform insertionAnchor = socketObject.transform.Find("InsertionAnchor");
            if (insertionAnchor == null)
            {
                insertionAnchor = new GameObject("InsertionAnchor").transform;
                insertionAnchor.SetParent(socketObject.transform, false);
            }

            insertionAnchor.localPosition = Vector3.zero;
            insertionAnchor.localRotation = Quaternion.Euler(0f, 0f, -15f);
            socket.attachTransform = insertionAnchor;
            socket.socketSnappingRadius = 0.38f;

            Transform feedback = pedestalObject.transform.Find("PedestalSolvedGlow");
            Light statusLight = pedestalObject.transform.Find("PedestalStatusLight")?.GetComponent<Light>();
            Renderer[] runes = feedback != null
                ? feedback.GetComponentsInChildren<Renderer>(true)
                : model.GetComponentsInChildren<Renderer>(true)
                    .Where(renderer => renderer.name.StartsWith("SocketRune"))
                    .ToArray();

            AudioSource audioSource = socketObject.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = socketObject.AddComponent<AudioSource>();
            }

            audioSource.clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/click.wav");
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
            audioSource.minDistance = 1.5f;
            audioSource.maxDistance = 14f;
            audioSource.volume = 0.65f;
            audioSource.pitch = 0.72f;

            ManorPuzzleSocket puzzle = socketObject.GetComponent<ManorPuzzleSocket>();
            puzzle.Configure(
                socket,
                "SilverFang",
                doorObject.transform,
                feedback != null ? feedback.gameObject : null,
                statusLight,
                insertionAnchor,
                doorSeal,
                runes,
                audioSource);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Enhanced the Silver Fang pedestal, insertion sequence, door seal, and finale.");
        }

        [MenuItem("Tools/Michael Manor/Install Presentation Shortcuts")]
        public static void InstallPresentationShortcuts()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ManorPuzzleSocket puzzle = Object.FindAnyObjectByType<ManorPuzzleSocket>();
            WinCelebrationController celebration = Object.FindAnyObjectByType<WinCelebrationController>();
            if (puzzle == null || celebration == null)
            {
                Debug.LogError("Puzzle or Win Celebration controller is missing from MichaelManorHall.");
                return;
            }

            ManorPresentationShortcuts shortcuts = celebration.GetComponent<ManorPresentationShortcuts>();
            if (shortcuts == null)
            {
                shortcuts = celebration.gameObject.AddComponent<ManorPresentationShortcuts>();
            }

            shortcuts.Configure(puzzle, celebration);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Installed presentation shortcuts: K unlock, W win celebration, R reset.");
        }

        public static void BuildSilverFangVisual(
            Transform root,
            Material silver,
            Material gold,
            Material blackIron,
            Material moonstone,
            Material outline = null)
        {
            foreach (Transform child in root.Cast<Transform>().ToArray())
            {
                if (child.name == "ArtifactVisual" || child.name == "GrabAttach")
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }

            foreach (MeshRenderer renderer in root.GetComponents<MeshRenderer>())
            {
                Object.DestroyImmediate(renderer);
            }

            foreach (MeshFilter filter in root.GetComponents<MeshFilter>())
            {
                Object.DestroyImmediate(filter);
            }

            foreach (Collider collider in root.GetComponents<Collider>())
            {
                Object.DestroyImmediate(collider);
            }

            Transform visual = new GameObject("ArtifactVisual").transform;
            visual.SetParent(root, false);

            GameObject blade = new GameObject("CurvedSilverFang");
            blade.transform.SetParent(visual, false);
            blade.AddComponent<MeshFilter>().sharedMesh = EnsureFangMesh();
            MeshRenderer bladeRenderer = blade.AddComponent<MeshRenderer>();
            AssignMaterials(bladeRenderer, silver, outline);

            CreatePart("Grip", PrimitiveType.Cylinder, visual, new Vector3(0f, 0.39f, 0f),
                new Vector3(0.075f, 0.13f, 0.075f), blackIron, outline);
            CreatePart("GripPommel", PrimitiveType.Sphere, visual, new Vector3(0f, 0.54f, 0f),
                new Vector3(0.12f, 0.08f, 0.12f), gold, outline);
            CreatePart("FangCollar", PrimitiveType.Cylinder, visual, new Vector3(0f, 0.20f, 0f),
                new Vector3(0.14f, 0.055f, 0.14f), gold, outline);
            CreatePart("Moonstone", PrimitiveType.Sphere, visual, new Vector3(0f, 0.39f, -0.075f),
                new Vector3(0.075f, 0.105f, 0.045f), moonstone, outline);

            // A small, layered bat-wing guard makes the silhouette readable from a VR distance.
            CreateWing("GuardWing_Left_Outer", visual, -0.18f, 0.23f, 28f, blackIron, outline);
            CreateWing("GuardWing_Left_Inner", visual, -0.105f, 0.19f, -18f, gold, outline);
            CreateWing("GuardWing_Right_Outer", visual, 0.18f, 0.23f, -28f, blackIron, outline);
            CreateWing("GuardWing_Right_Inner", visual, 0.105f, 0.19f, 18f, gold, outline);

            for (int i = 0; i < 3; i++)
            {
                CreatePart(
                    $"GripBand_{i + 1:00}",
                    PrimitiveType.Cylinder,
                    visual,
                    new Vector3(0f, 0.29f + i * 0.09f, 0f),
                    new Vector3(0.086f, 0.012f, 0.086f),
                    gold,
                    outline);
            }

            CapsuleCollider bladeCollider = root.gameObject.AddComponent<CapsuleCollider>();
            bladeCollider.direction = 1;
            bladeCollider.center = new Vector3(0.055f, -0.10f, 0f);
            bladeCollider.radius = 0.105f;
            bladeCollider.height = 0.92f;

            BoxCollider guardCollider = root.gameObject.AddComponent<BoxCollider>();
            guardCollider.center = new Vector3(0f, 0.24f, 0f);
            guardCollider.size = new Vector3(0.55f, 0.18f, 0.18f);

            Transform attach = new GameObject("GrabAttach").transform;
            attach.SetParent(root, false);
            attach.localPosition = new Vector3(0f, 0.40f, 0f);
            attach.localRotation = Quaternion.Euler(0f, 0f, 180f);
        }

        public static Transform BuildGothicPedestalModel(
            Transform pedestal,
            Material stone,
            Material gold,
            Material blackIron,
            Material moonstone,
            Material outline = null)
        {
            foreach (string legacyName in new[] { "PedestalBase", "PedestalStem", "PedestalTop" })
            {
                Transform legacy = pedestal.Find(legacyName);
                if (legacy != null)
                {
                    legacy.gameObject.SetActive(false);
                }
            }

            Transform existing = pedestal.Find("GothicPedestalModel");
            if (existing != null)
            {
                existing.gameObject.SetActive(true);
                return existing;
            }

            Transform model = new GameObject("GothicPedestalModel").transform;
            model.SetParent(pedestal, false);

            CreatePart("ObsidianFoot", PrimitiveType.Cylinder, model, new Vector3(0f, 0.16f, 0f),
                new Vector3(1.25f, 0.16f, 1.25f), blackIron, outline);
            CreatePart("StoneBase", PrimitiveType.Cylinder, model, new Vector3(0f, 0.38f, 0f),
                new Vector3(1.02f, 0.11f, 1.02f), stone, outline);
            CreatePart("GoldBaseBand", PrimitiveType.Cylinder, model, new Vector3(0f, 0.53f, 0f),
                new Vector3(0.86f, 0.045f, 0.86f), gold, outline);
            CreatePart("CarvedColumn", PrimitiveType.Cylinder, model, new Vector3(0f, 1.05f, 0f),
                new Vector3(0.48f, 0.50f, 0.48f), stone, outline);
            CreatePart("ColumnIronCore", PrimitiveType.Cylinder, model, new Vector3(0f, 1.08f, 0f),
                new Vector3(0.34f, 0.54f, 0.34f), blackIron, outline);
            CreatePart("GothicCapital", PrimitiveType.Cylinder, model, new Vector3(0f, 1.60f, 0f),
                new Vector3(0.76f, 0.12f, 0.76f), stone, outline);
            CreatePart("RuneBasin", PrimitiveType.Cylinder, model, new Vector3(0f, 1.82f, 0f),
                new Vector3(0.90f, 0.10f, 0.90f), blackIron, outline);
            CreatePart("BasinRim", PrimitiveType.Cylinder, model, new Vector3(0f, 1.92f, 0f),
                new Vector3(0.78f, 0.055f, 0.78f), gold, outline);

            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI * 2f / 8f;
                CreatePart(
                    $"SocketRune_{i + 1:00}",
                    PrimitiveType.Sphere,
                    model,
                    new Vector3(Mathf.Cos(angle) * 0.60f, 2.01f, Mathf.Sin(angle) * 0.60f),
                    new Vector3(0.09f, 0.045f, 0.09f),
                    moonstone,
                    outline);
            }

            for (int i = 0; i < 4; i++)
            {
                float angle = i * Mathf.PI * 0.5f + Mathf.PI * 0.25f;
                Vector3 position = new Vector3(Mathf.Cos(angle) * 0.72f, 1.76f, Mathf.Sin(angle) * 0.72f);
                GameObject spire = CreatePart(
                    $"GothicSpire_{i + 1:00}",
                    PrimitiveType.Cylinder,
                    model,
                    position,
                    new Vector3(0.075f, 0.32f, 0.075f),
                    blackIron,
                    outline);
                spire.transform.localRotation = Quaternion.Euler(0f, -angle * Mathf.Rad2Deg, 8f);
                CreatePart(
                    $"SpireGem_{i + 1:00}",
                    PrimitiveType.Sphere,
                    model,
                    position + Vector3.up * 0.38f,
                    Vector3.one * 0.095f,
                    moonstone,
                    outline);
            }

            GameObject leftProng = CreatePart("FangCradle_Left", PrimitiveType.Cube, model,
                new Vector3(-0.18f, 2.05f, 0f), new Vector3(0.07f, 0.32f, 0.10f), gold, outline);
            leftProng.transform.localRotation = Quaternion.Euler(0f, 0f, -18f);
            GameObject rightProng = CreatePart("FangCradle_Right", PrimitiveType.Cube, model,
                new Vector3(0.18f, 2.05f, 0f), new Vector3(0.07f, 0.32f, 0.10f), gold, outline);
            rightProng.transform.localRotation = Quaternion.Euler(0f, 0f, 18f);

            return model;
        }

        public static Transform BuildDoorSeal(
            Transform door,
            Material silver,
            Material gold,
            Material blackIron,
            Material moonstone,
            Material outline = null)
        {
            Transform existing = door.Find("SilverFangDoorSeal");
            if (existing != null)
            {
                existing.gameObject.SetActive(true);
                return existing;
            }

            Transform seal = new GameObject("SilverFangDoorSeal").transform;
            seal.SetParent(door, false);
            seal.localPosition = new Vector3(0f, 3.55f, 15.19f);

            GameObject disc = CreatePart("SealDisc", PrimitiveType.Cylinder, seal, Vector3.zero,
                new Vector3(0.62f, 0.075f, 0.62f), blackIron, outline);
            disc.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            GameObject innerDisc = CreatePart("SealInnerGold", PrimitiveType.Cylinder, seal, new Vector3(0f, 0f, -0.09f),
                new Vector3(0.48f, 0.045f, 0.48f), gold, outline);
            innerDisc.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            GameObject crest = new GameObject("FangCrest");
            crest.transform.SetParent(seal, false);
            crest.transform.localPosition = new Vector3(-0.04f, 0.08f, -0.16f);
            crest.transform.localScale = Vector3.one * 0.72f;
            crest.AddComponent<MeshFilter>().sharedMesh = EnsureFangMesh();
            AssignMaterials(crest.AddComponent<MeshRenderer>(), silver, outline);

            for (int i = 0; i < 10; i++)
            {
                float angle = i * Mathf.PI * 2f / 10f;
                CreatePart(
                    $"SealRune_{i + 1:00}",
                    PrimitiveType.Sphere,
                    seal,
                    new Vector3(Mathf.Cos(angle) * 0.74f, Mathf.Sin(angle) * 0.74f, -0.13f),
                    Vector3.one * 0.085f,
                    moonstone,
                    outline);
            }

            return seal;
        }

        private static void CreateWing(
            string name,
            Transform parent,
            float x,
            float y,
            float zRotation,
            Material material,
            Material outline)
        {
            GameObject wing = CreatePart(
                name,
                PrimitiveType.Cube,
                parent,
                new Vector3(x, y, 0f),
                new Vector3(0.22f, 0.055f, 0.10f),
                material,
                outline);
            wing.transform.localRotation = Quaternion.Euler(0f, 0f, zRotation);

            GameObject talon = CreatePart(
                name + "_Talon",
                PrimitiveType.Sphere,
                parent,
                new Vector3(x * 1.62f, y - 0.045f, 0f),
                new Vector3(0.055f, 0.11f, 0.055f),
                material,
                outline);
            talon.transform.localRotation = Quaternion.Euler(0f, 0f, -zRotation * 0.65f);
        }

        private static GameObject CreatePart(
            string name,
            PrimitiveType type,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Material material,
            Material outline)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            Collider collider = part.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            AssignMaterials(part.GetComponent<MeshRenderer>(), material, outline);
            return part;
        }

        private static Mesh EnsureFangMesh()
        {
            if (!AssetDatabase.IsValidFolder(ModelFolder))
            {
                AssetDatabase.CreateFolder("Assets/MichaelManor", "Models");
            }

            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(FangMeshPath);
            if (mesh == null)
            {
                mesh = new Mesh { name = "SilverFangBlade" };
                AssetDatabase.CreateAsset(mesh, FangMeshPath);
            }

            const int ringCount = 20;
            const int sideCount = 14;
            List<Vector3> vertices = new List<Vector3>(ringCount * sideCount + 2);
            List<int> triangles = new List<int>((ringCount - 1) * sideCount * 6 + sideCount * 6);

            for (int ring = 0; ring < ringCount; ring++)
            {
                float t = ring / (float)(ringCount - 1);
                float y = Mathf.Lerp(0.18f, -0.58f, t);
                float x = 0.02f + 0.15f * t * t;
                float radius = Mathf.Lerp(0.115f, 0.012f, Mathf.Pow(t, 1.18f));
                float flatten = Mathf.Lerp(0.82f, 0.58f, t);

                for (int side = 0; side < sideCount; side++)
                {
                    float angle = side * Mathf.PI * 2f / sideCount;
                    vertices.Add(new Vector3(
                        x + Mathf.Cos(angle) * radius,
                        y,
                        Mathf.Sin(angle) * radius * flatten));
                }
            }

            for (int ring = 0; ring < ringCount - 1; ring++)
            {
                for (int side = 0; side < sideCount; side++)
                {
                    int nextSide = (side + 1) % sideCount;
                    int a = ring * sideCount + side;
                    int b = ring * sideCount + nextSide;
                    int c = (ring + 1) * sideCount + side;
                    int d = (ring + 1) * sideCount + nextSide;
                    triangles.Add(a);
                    triangles.Add(c);
                    triangles.Add(b);
                    triangles.Add(b);
                    triangles.Add(c);
                    triangles.Add(d);
                }
            }

            int baseCenter = vertices.Count;
            vertices.Add(new Vector3(0.02f, 0.18f, 0f));
            int tipCenter = vertices.Count;
            vertices.Add(new Vector3(0.17f, -0.59f, 0f));
            for (int side = 0; side < sideCount; side++)
            {
                int next = (side + 1) % sideCount;
                triangles.Add(baseCenter);
                triangles.Add(next);
                triangles.Add(side);

                int tip = (ringCount - 1) * sideCount;
                triangles.Add(tipCenter);
                triangles.Add(tip + side);
                triangles.Add(tip + next);
            }

            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            EditorUtility.SetDirty(mesh);
            return mesh;
        }

        private static void AssignMaterials(Renderer renderer, Material primary, Material outline)
        {
            renderer.sharedMaterials = outline != null
                ? new[] { primary, outline }
                : new[] { primary };
        }

        private static Material LoadMaterial(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Material>($"Assets/MichaelManor/Materials/{name}.mat");
        }

        private static GameObject FindSceneObject(Scene scene, string name)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Transform match = root.GetComponentsInChildren<Transform>(true)
                    .FirstOrDefault(transform => transform.name == name);
                if (match != null)
                {
                    return match.gameObject;
                }
            }

            return null;
        }
    }
}
