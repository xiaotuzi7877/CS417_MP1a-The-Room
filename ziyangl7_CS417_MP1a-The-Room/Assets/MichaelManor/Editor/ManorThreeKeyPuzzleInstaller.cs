using System.Linq;
using MichaelManor;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MichaelManorEditor
{
    public static class ManorThreeKeyPuzzleInstaller
    {
        private const string ScenePath = "Assets/Scenes/MichaelManorHall.unity";

        [MenuItem("Tools/Michael Manor/Install Three Key Release Puzzles")]
        public static void Install()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ManorThreeStagePuzzle ritual = Object.FindFirstObjectByType<ManorThreeStagePuzzle>(FindObjectsInactive.Include);
            ManorMoonCryptPuzzle moon = Object.FindFirstObjectByType<ManorMoonCryptPuzzle>(FindObjectsInactive.Include);
            ManorKeyArtifact fang = Artifact("SilverFang"); ManorKeyArtifact blood = Artifact("BloodSigil");
            Transform decor = Find(scene, "Decor"); Transform systems = Find(scene, "Systems");
            Transform bloodVault = Find(scene, "BloodSigilVault");
            if (ritual == null || moon == null || fang == null || blood == null || decor == null || systems == null || bloodVault == null)
            { Debug.LogError("Three Key Puzzle install is missing ritual, keys, Moon Crypt, Blood Vault, Decor, or Systems."); return; }

            Transform oldHall = decor.Find("SilverFangReleasePuzzle"); if (oldHall != null) Object.DestroyImmediate(oldHall.gameObject);
            Transform hallRoot = Group("SilverFangReleasePuzzle", decor);
            Transform fangBarrier = BuildFangDisplayCase(hallRoot, fang.transform, GetOrCreateGlassMaterial(), Mat("BlackIron"));
            TextMeshPro hallStatus = Text("PortraitSequenceStatus", hallRoot, fang.transform.position + new Vector3(0f, 1.55f, -0.72f),
                "BAT  ->  WOLF  ->  MOON\nSEQUENCE  0 / 3", new Color(0.75f, 0.88f, 1f), 0.42f);
            ManorKeyReleasePuzzle first = hallRoot.gameObject.AddComponent<ManorKeyReleasePuzzle>();
            first.Configure(ritual, -1, new[] { 0, 1, 2 }, fangBarrier, fangBarrier.localPosition + new Vector3(0f, 2.2f, 0f), fang, hallStatus);
            string[] signs = { "BAT", "WOLF", "MOON" };
            for (int i = 0; i < 3; i++)
            {
                Vector3 pos = fang.transform.position + new Vector3(-1f + i, -0.38f, -0.92f);
                Transform button = Part("PortraitRune_" + signs[i], PrimitiveType.Cylinder, hallRoot, pos, new Vector3(0.26f, 0.10f, 0.26f), Mat("CelestialMoon"), true, true);
                button.rotation = Quaternion.Euler(90f, 0f, 0f); button.gameObject.AddComponent<XRSimpleInteractable>();
                button.gameObject.AddComponent<ManorKeyReleaseButton>().Configure(first, i);
                AddFeedbackLight(button, new Color(0.20f, 0.88f, 1f));
                Text("Label_" + signs[i], hallRoot, pos + new Vector3(0f, 0.42f, 0f), signs[i], Color.white, 0.26f);
            }

            Transform oldFinal = bloodVault.Find("BloodSigilLeverPuzzle"); if (oldFinal != null) Object.DestroyImmediate(oldFinal.gameObject);
            Transform finalRoot = Group("BloodSigilLeverPuzzle", bloodVault);
            Transform bloodBarrier = Part("BloodSigil_FinalSeal", PrimitiveType.Cube, finalRoot,
                blood.transform.position + new Vector3(0f, 0.1f, -0.58f), new Vector3(1.35f, 1.75f, 0.18f), Mat("BloodVelvet"), true, true);
            TextMeshPro finalStatus = Text("LeverSequenceStatus", finalRoot, blood.transform.position + new Vector3(0f, 1.6f, -0.7f),
                "LEFT  ->  RIGHT\nSEALED UNTIL THE CELESTIAL LOCK", new Color(1f, 0.45f, 0.32f), 0.30f);
            ManorKeyReleasePuzzle third = finalRoot.gameObject.AddComponent<ManorKeyReleasePuzzle>();
            third.Configure(ritual, 2, new[] { 0, 1 }, bloodBarrier, bloodBarrier.localPosition + new Vector3(0f, 2.3f, 0f), blood, finalStatus);
            for (int i = 0; i < 2; i++)
            {
                Vector3 pos = blood.transform.position + new Vector3(i == 0 ? -0.62f : 0.62f, -0.42f, -0.82f);
                Transform lever = Part(i == 0 ? "Lever_LEFT" : "Lever_RIGHT", PrimitiveType.Cylinder, finalRoot, pos,
                    new Vector3(0.16f, 0.48f, 0.16f), Mat("AntiqueGold"), true, true);
                lever.rotation = Quaternion.Euler(0f, 0f, i == 0 ? 22f : -22f); lever.gameObject.AddComponent<XRSimpleInteractable>();
                lever.gameObject.AddComponent<ManorKeyReleaseButton>().Configure(third, i);
            }

            GameObject trackerObject = systems.Find("PuzzleProgressTracker")?.gameObject;
            if (trackerObject == null) { trackerObject = new GameObject("PuzzleProgressTracker"); trackerObject.transform.SetParent(systems, false); }
            ManorPuzzleProgressTracker tracker = trackerObject.GetComponent<ManorPuzzleProgressTracker>();
            if (tracker == null) tracker = trackerObject.AddComponent<ManorPuzzleProgressTracker>();
            Transform boardParent = ritual.ProgressText.transform.parent;
            Transform oldBoard = boardParent.Find("PuzzleAndClueCounter"); if (oldBoard != null) Object.DestroyImmediate(oldBoard.gameObject);
            TextMeshPro board = Text("PuzzleAndClueCounter", boardParent, ritual.ProgressText.transform.position,
                "PUZZLES  0 / 3     CLUES  0 / 3", new Color(1f, 0.76f, 0.30f), 0.38f);
            board.transform.localPosition = new Vector3(0f, -0.78f, 0.081f);
            board.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            board.rectTransform.sizeDelta = new Vector2(2.95f, 0.24f);
            board.enableAutoSizing = true;
            board.fontSizeMin = 0.25f;
            board.fontSizeMax = 0.38f;
            tracker.Configure(first, moon, third, ritual, board);

            string[] clueNames = { "Clue_01_Watcher", "Clue_02_Heavens", "Clue_03_Exit" };
            for (int i = 0; i < clueNames.Length; i++)
            {
                Transform plaque = Find(scene, clueNames[i]); if (plaque == null) continue;
                if (plaque.GetComponent<Collider>() == null) plaque.gameObject.AddComponent<BoxCollider>();
                if (plaque.GetComponent<XRSimpleInteractable>() == null) plaque.gameObject.AddComponent<XRSimpleInteractable>();
                ManorClueDiscovery discovery = plaque.GetComponent<ManorClueDiscovery>();
                if (discovery == null) discovery = plaque.gameObject.AddComponent<ManorClueDiscovery>();
                discovery.Configure(tracker, i); EditorUtility.SetDirty(discovery);
            }

            EditorUtility.SetDirty(first); EditorUtility.SetDirty(third); EditorUtility.SetDirty(tracker);
            EditorSceneManager.MarkSceneDirty(scene); ManorTextOrientationFixer.Apply(); EditorSceneManager.SaveScene(scene);
            Debug.Log("Installed three genuine key-release puzzles plus 3/3 puzzle and clue discovery scoreboard.");
        }

        /// The display top used a dome-shaped capsule collider and the Fang stood upright on it, so the
        /// Fang tipped over and rolled onto the floor at start and after every reset. Give the top a
        /// flat collider matching its visible disc and lay the Fang flat on it.
        [MenuItem("Tools/Michael Manor/Fix Silver Fang Display")]
        public static void FixSilverFangDisplay()
        {
            Scene scene = SceneManager.GetActiveScene();
            Transform top = Find(scene, "DisplayTop");
            ManorKeyArtifact fang = Artifact("SilverFang");
            if (top == null || fang == null) { Debug.LogError("Silver Fang display fix: DisplayTop or Silver Fang missing."); return; }

            CapsuleCollider dome = top.GetComponent<CapsuleCollider>();
            if (dome != null) Object.DestroyImmediate(dome);
            BoxCollider flat = top.GetComponent<BoxCollider>();
            if (flat == null) flat = top.gameObject.AddComponent<BoxCollider>();
            flat.center = Vector3.zero;
            flat.size = new Vector3(1f, 2f, 1f);   // unit cylinder bounds: flat disc of the visible size

            CapsuleCollider capsule = fang.GetComponent<CapsuleCollider>();
            float surface = top.GetComponent<Renderer>().bounds.max.y;
            Quaternion lying = Quaternion.Euler(0f, 0f, 90f);           // capsule axis horizontal
            Vector3 centerOffset = lying * Vector3.Scale(capsule.center, fang.transform.lossyScale);
            Vector3 topCenter = top.GetComponent<Renderer>().bounds.center;
            float radius = capsule.radius * fang.transform.lossyScale.x;
            fang.transform.SetPositionAndRotation(
                new Vector3(topCenter.x, surface + radius + 0.005f, topCenter.z) - centerOffset, lying);

            // The Fang is longer than the tabletop and its mass sits off-centre, so a dynamic body
            // slowly slides off. Keep it still on display; it gets normal physics once released.
            fang.GetComponent<Rigidbody>().isKinematic = true;
            if (fang.GetComponent<ManorRestUntilGrabbed>() == null) fang.gameObject.AddComponent<ManorRestUntilGrabbed>();

            EditorUtility.SetDirty(top.gameObject);
            EditorUtility.SetDirty(fang.transform);
            EditorSceneManager.MarkSceneDirty(scene);
            ManorTextOrientationFixer.Apply(); EditorSceneManager.SaveScene(scene);
            Debug.Log("Silver Fang now rests flat on a flat display top.");
        }

        [MenuItem("Tools/Michael Manor/Fix VR Playtest Sections 1-3")]
        public static void FixVrPlaytestSectionsOneToThree()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ManorThreeStagePuzzleInstaller.ApplyEntryMissionWallLayout(scene);

            Transform hallRoot = Find(scene, "SilverFangReleasePuzzle");
            if (hallRoot == null)
            {
                Debug.LogError("VR playtest fix stopped: SilverFangReleasePuzzle was not found.");
                return;
            }

            string[] signs = { "BAT", "WOLF", "MOON" };
            foreach (string sign in signs)
            {
                Transform button = hallRoot.Find("PortraitRune_" + sign);
                Transform label = hallRoot.Find("Label_" + sign);
                if (button == null || label == null) continue;

                label.position = button.position + Vector3.up * 0.42f;
                TextMeshPro text = label.GetComponent<TextMeshPro>();
                if (text != null)
                {
                    text.rectTransform.sizeDelta = new Vector2(0.95f, 0.30f);
                    text.enableWordWrapping = false;
                    text.alignment = TextAlignmentOptions.Center;
                    EditorUtility.SetDirty(text);
                }

                Light light = button.GetComponentInChildren<Light>(true);
                if (light == null)
                {
                    AddFeedbackLight(button, new Color(0.20f, 0.88f, 1f));
                    light = button.GetComponentInChildren<Light>(true);
                }
                if (light != null)
                {
                    light.type = LightType.Point;
                    light.range = 1.2f;
                    light.intensity = 0f;
                    light.shadows = LightShadows.None;
                    EditorUtility.SetDirty(light);
                }
                EditorUtility.SetDirty(button);
                EditorUtility.SetDirty(label);
            }

            ManorTextOrientationFixer.Apply();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("VR PLAYTEST SECTIONS 1-3 FIXED: mission wall layout, Fang labels, and button feedback lights.");
        }

        [MenuItem("Tools/Michael Manor/Install Transparent Silver Fang Case")]
        public static void InstallTransparentSilverFangCase()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Transform hallRoot = Find(scene, "SilverFangReleasePuzzle");
            ManorKeyArtifact fang = Artifact("SilverFang");
            ManorKeyReleasePuzzle puzzle = hallRoot != null ? hallRoot.GetComponent<ManorKeyReleasePuzzle>() : null;
            if (hallRoot == null || fang == null || puzzle == null)
            {
                Debug.LogError("Transparent Fang case install stopped: puzzle root, Silver Fang, or release puzzle is missing.");
                return;
            }

            foreach (string oldName in new[] { "SilverFang_DisplayCase", "SilverFang_DisplayCase_Shutter" })
            {
                Transform old = hallRoot.Find(oldName);
                if (old != null) Object.DestroyImmediate(old.gameObject);
            }

            Transform displayCase = BuildFangDisplayCase(hallRoot, fang.transform, GetOrCreateGlassMaterial(), Mat("BlackIron"));
            SerializedObject serializedPuzzle = new SerializedObject(puzzle);
            serializedPuzzle.FindProperty("barrier").objectReferenceValue = displayCase;
            serializedPuzzle.FindProperty("openLocalPosition").vector3Value = displayCase.localPosition + new Vector3(0f, 2.2f, 0f);
            serializedPuzzle.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(puzzle);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("VR PLAYTEST SECTION 4 FIXED: transparent six-sided Fang case installed and wired to the release puzzle.");
        }

        [MenuItem("Tools/Michael Manor/Validate Transparent Silver Fang Case")]
        public static void ValidateTransparentSilverFangCase()
        {
            Transform displayCase = GameObject.Find("SilverFang_DisplayCase")?.transform;
            ManorKeyArtifact fang = Artifact("SilverFang");
            ManorKeyReleasePuzzle puzzle = GameObject.Find("SilverFangReleasePuzzle")?.GetComponent<ManorKeyReleasePuzzle>();
            string[] panels = { "GlassFront", "GlassBack", "GlassLeft", "GlassRight", "GlassTop", "GlassBottom" };
            bool valid = displayCase != null && fang != null && puzzle != null && puzzle.Barrier == displayCase;
            valid &= valid && panels.All(name =>
            {
                Transform panel = displayCase.Find(name);
                Renderer renderer = panel != null ? panel.GetComponent<Renderer>() : null;
                return panel != null && panel.GetComponent<BoxCollider>() != null && renderer != null &&
                       renderer.sharedMaterial != null && renderer.sharedMaterial.color.a < 0.5f;
            });

            if (valid)
            {
                Bounds fangBounds = CombinedBounds(fang.transform);
                Renderer front = displayCase.Find("GlassFront").GetComponent<Renderer>();
                Renderer back = displayCase.Find("GlassBack").GetComponent<Renderer>();
                Renderer left = displayCase.Find("GlassLeft").GetComponent<Renderer>();
                Renderer right = displayCase.Find("GlassRight").GetComponent<Renderer>();
                Renderer top = displayCase.Find("GlassTop").GetComponent<Renderer>();
                Renderer bottom = displayCase.Find("GlassBottom").GetComponent<Renderer>();
                valid &= left.bounds.max.x <= fangBounds.min.x && right.bounds.min.x >= fangBounds.max.x &&
                         bottom.bounds.max.y <= fangBounds.min.y && top.bounds.min.y >= fangBounds.max.y &&
                         front.bounds.max.z <= fangBounds.min.z && back.bounds.min.z >= fangBounds.max.z;
            }

            if (valid) Debug.Log("TRANSPARENT FANG CASE PASS: six transparent collider panels fully enclose the artifact and move as one barrier.");
            else Debug.LogError("Transparent Fang case validation failed.");
        }

        [MenuItem("Tools/Michael Manor/Test Three Key Release Puzzles (Play Mode)")]
        public static void TestInPlayMode()
        {
            if (!Application.isPlaying) { Debug.LogError("Enter Play Mode before testing Three Key Release puzzles."); return; }
            ManorKeyReleasePuzzle first = GameObject.Find("SilverFangReleasePuzzle")?.GetComponent<ManorKeyReleasePuzzle>();
            ManorKeyReleasePuzzle third = Object.FindObjectsByType<ManorKeyReleasePuzzle>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(p => p != first);
            ManorMoonCryptPuzzle moon = Object.FindFirstObjectByType<ManorMoonCryptPuzzle>(FindObjectsInactive.Include);
            ManorPuzzleProgressTracker tracker = Object.FindFirstObjectByType<ManorPuzzleProgressTracker>(FindObjectsInactive.Include);
            ManorKeyArtifact fangArtifact = Artifact("SilverFang");
            ManorClueDiscovery[] clues = Object.FindObjectsByType<ManorClueDiscovery>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            ManorKeyReleaseButton[] fangButtons = first != null
                ? first.GetComponentsInChildren<ManorKeyReleaseButton>(true).OrderBy(button => button.name).ToArray()
                : new ManorKeyReleaseButton[0];
            bool valid = first != null && third != null && moon != null && tracker != null && clues.Length == 3 &&
                         fangButtons.Length == 3 && fangButtons.All(button => button.FeedbackLight != null);
            if (valid)
            {
                XRGrabInteractable fangGrab = fangArtifact != null ? fangArtifact.GetComponent<XRGrabInteractable>() : null;
                valid &= fangGrab != null && !fangGrab.enabled;
                valid &= !fangButtons[1].PressForTest() && first.SequencePosition == 0 &&
                         fangButtons[1].FeedbackLight.intensity > 0f &&
                         fangButtons[1].FeedbackLight.color.r > fangButtons[1].FeedbackLight.color.b;
                first.ResetPuzzle();
                valid &= fangButtons[0].PressForTest() && first.SequencePosition == 1 &&
                         fangButtons[0].FeedbackLight.intensity > 0f &&
                         fangButtons[0].FeedbackLight.color.b > fangButtons[0].FeedbackLight.color.r;
                first.ResetPuzzle();
                first.SolveForTest(); moon.PressButton(0); moon.PressButton(1); moon.PressButton(2);
                valid &= fangGrab.enabled;
                valid &= !third.Press(0); third.ForceSolveForTest();
                foreach (ManorClueDiscovery clue in clues) valid &= clue.DiscoverForTest();
                tracker.RecalculateForTest();
                valid &= first.IsSolved && moon.IsSolved && third.IsSolved && tracker.PuzzlesSolved == 3 && tracker.CluesFound == 3;
                Object.FindFirstObjectByType<ManorThreeStagePuzzle>(FindObjectsInactive.Include).ResetPuzzle();
                tracker.RecalculateForTest(); valid &= tracker.PuzzlesSolved == 0 && tracker.CluesFound == 0;
            }
            if (valid) Debug.Log("THREE KEY RELEASE PUZZLES PASS: press animation feedback, wrong-order rejection, three releases, 3/3 clues, scoreboard, and reset verified.");
            else Debug.LogError("Three Key Release Puzzle test failed.");
        }

        private static Transform Group(string name, Transform parent) { Transform t = new GameObject(name).transform; t.SetParent(parent, false); return t; }
        private static Transform Part(string name, PrimitiveType type, Transform parent, Vector3 world, Vector3 scale, Material material, bool collider, bool worldPosition)
        { GameObject g = GameObject.CreatePrimitive(type); g.name = name; g.transform.SetParent(parent, worldPosition); g.transform.position = world; g.transform.localScale = scale; if (material != null) g.GetComponent<Renderer>().sharedMaterial = material; if (!collider) Object.DestroyImmediate(g.GetComponent<Collider>()); return g.transform; }
        private static TextMeshPro Text(string name, Transform parent, Vector3 world, string value, Color color, float size)
        { GameObject g = new GameObject(name); g.transform.SetParent(parent, true); g.transform.position = world; g.transform.rotation = Quaternion.Euler(0f, 180f, 0f); TextMeshPro t = g.AddComponent<TextMeshPro>(); t.text = value; t.fontSize = size; t.fontStyle = FontStyles.Bold; t.alignment = TextAlignmentOptions.Center; t.color = color; t.rectTransform.sizeDelta = new Vector2(4.5f, 1.2f); return t; }
        private static void AddFeedbackLight(Transform button, Color color)
        {
            GameObject lightObject = new GameObject("FeedbackLight");
            lightObject.transform.SetParent(button, false);
            lightObject.transform.localPosition = Vector3.up * 0.65f;
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.range = 1.2f;
            light.intensity = 0f;
            light.shadows = LightShadows.None;
        }

        private static Transform BuildFangDisplayCase(Transform parent, Transform fang, Material glass, Material frame)
        {
            Bounds bounds = CombinedBounds(fang);
            float width = Mathf.Max(1.70f, bounds.size.x + 0.46f);
            float height = Mathf.Max(1.20f, bounds.size.y + 0.46f);
            float depth = Mathf.Max(1.00f, bounds.size.z + 0.46f);
            const float glassThickness = 0.055f;

            Transform root = Group("SilverFang_DisplayCase", parent);
            root.position = bounds.center;
            LocalPart("GlassFront", root, new Vector3(0f, 0f, -depth * 0.5f), new Vector3(width, height, glassThickness), glass);
            LocalPart("GlassBack", root, new Vector3(0f, 0f, depth * 0.5f), new Vector3(width, height, glassThickness), glass);
            LocalPart("GlassLeft", root, new Vector3(-width * 0.5f, 0f, 0f), new Vector3(glassThickness, height, depth), glass);
            LocalPart("GlassRight", root, new Vector3(width * 0.5f, 0f, 0f), new Vector3(glassThickness, height, depth), glass);
            LocalPart("GlassTop", root, new Vector3(0f, height * 0.5f, 0f), new Vector3(width, glassThickness, depth), glass);
            LocalPart("GlassBottom", root, new Vector3(0f, -height * 0.5f, 0f), new Vector3(width, glassThickness, depth), glass);

            float rail = 0.045f;
            float frontZ = -depth * 0.5f - glassThickness;
            LocalPart("FrameTop", root, new Vector3(0f, height * 0.5f, frontZ), new Vector3(width + rail, rail, rail), frame, false);
            LocalPart("FrameBottom", root, new Vector3(0f, -height * 0.5f, frontZ), new Vector3(width + rail, rail, rail), frame, false);
            LocalPart("FrameLeft", root, new Vector3(-width * 0.5f, 0f, frontZ), new Vector3(rail, height, rail), frame, false);
            LocalPart("FrameRight", root, new Vector3(width * 0.5f, 0f, frontZ), new Vector3(rail, height, rail), frame, false);
            return root;
        }

        private static Transform LocalPart(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material, bool collider = true)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            Renderer renderer = part.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            if (!collider) Object.DestroyImmediate(part.GetComponent<Collider>());
            return part.transform;
        }

        private static Bounds CombinedBounds(Transform root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            Bounds result = new Bounds(root.position, Vector3.zero);
            bool initialized = false;
            foreach (Renderer renderer in renderers)
            {
                if (!initialized) { result = renderer.bounds; initialized = true; }
                else result.Encapsulate(renderer.bounds);
            }
            return result;
        }

        private static Material GetOrCreateGlassMaterial()
        {
            const string path = "Assets/MichaelManor/Materials/RitualGlass.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                material = new Material(shader) { name = "RitualGlass" };
                AssetDatabase.CreateAsset(material, path);
            }
            Color tint = new Color(0.22f, 0.72f, 0.88f, 0.22f);
            material.SetColor("_BaseColor", tint);
            material.SetColor("_Color", tint);
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.SetFloat("_Smoothness", 0.88f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.DisableKeyword("_SURFACE_TYPE_OPAQUE");
            material.renderQueue = (int)RenderQueue.Transparent;
            EditorUtility.SetDirty(material);
            return material;
        }
        private static Material Mat(string name) => AssetDatabase.LoadAssetAtPath<Material>("Assets/MichaelManor/Materials/" + name + ".mat");
        private static Transform Find(Scene scene, string name) => scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).FirstOrDefault(t => t.name == name);
        private static ManorKeyArtifact Artifact(string id) => Object.FindObjectsByType<ManorKeyArtifact>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(a => a.ArtifactId == id);
    }
}
