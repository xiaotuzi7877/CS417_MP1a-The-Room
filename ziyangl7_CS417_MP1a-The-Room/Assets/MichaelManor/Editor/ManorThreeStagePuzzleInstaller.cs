using System.Linq;
using MichaelManor;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace MichaelManorEditor
{
    /// <summary>
    /// Installs the three-lock manor ritual without rebuilding or overwriting the hand-arranged room.
    /// Safe to rerun: only the generated RitualSequence hierarchy is replaced.
    /// </summary>
    public static class ManorThreeStagePuzzleInstaller
    {
        private const string ScenePath = "Assets/Scenes/MichaelManorHall.unity";

        private sealed class SocketBuildResult
        {
            public XRSocketInteractor Socket;
            public Light StatusLight;
        }

        [MenuItem("Tools/Michael Manor/Install Three Stage Ritual")]
        public static void InstallThreeStageRitual()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject puzzle = FindSceneObject(scene, "Puzzle");
            GameObject silverFang = FindSceneObject(scene, "SilverFang");
            GameObject finalSocketObject = FindSceneObject(scene, "SilverFangSocket") ??
                                           FindSceneObject(scene, "BloodSigilDoorSocket");
            GameObject exitDoor = FindSceneObject(scene, "ExitDoor");
            GameObject doorSeal = FindSceneObject(scene, "SilverFangDoorSeal");
            GameObject movingOrrery = FindSceneObject(scene, "MovingOrbitAssembly");
            GameObject watcherPortrait = FindSceneObject(scene, "Portrait_Left_0");
            WinCelebrationController celebration = Object.FindAnyObjectByType<WinCelebrationController>();

            if (puzzle == null || silverFang == null || finalSocketObject == null ||
                exitDoor == null || doorSeal == null || movingOrrery == null ||
                watcherPortrait == null || celebration == null)
            {
                Debug.LogError("Three-stage ritual install stopped: a required manor object is missing.");
                return;
            }

            Transform oldSequence = puzzle.transform.Find("RitualSequence");
            if (oldSequence != null)
            {
                Object.DestroyImmediate(oldSequence.gameObject);
            }

            Material darkWood = LoadMaterial("DarkWood");
            Material blackIron = LoadMaterial("BlackIron");
            Material gold = LoadMaterial("AntiqueGold");
            Material stone = LoadMaterial("ManorStone");
            Material blood = LoadMaterial("BloodVelvet");
            Material moon = LoadMaterial("SpectralGlow");
            Material silver = LoadMaterial("SilverFang");

            Transform sequenceRoot = NewGroup("RitualSequence", puzzle.transform);
            Transform locksRoot = NewGroup("Locks", sequenceRoot);
            Transform keysRoot = NewGroup("Keys", sequenceRoot);
            Transform cluesRoot = NewGroup("CluesAndProgress", sequenceRoot);

            // Lock I: an existing wall portrait slides up and reveals the Moonstone.
            Transform watcher = NewGroup("Lock_01_WatcherPortrait", locksRoot);
            watcher.position = new Vector3(-8.05f, 1.80f, -9.3f);
            watcher.rotation = Quaternion.identity;
            Transform movingPortrait = watcherPortrait.transform;
            CreatePart("WatcherEye", PrimitiveType.Sphere, watcher, new Vector3(0.08f, 0.38f, 0f),
                new Vector3(0.13f, 0.18f, 0.22f), moon, false);

            SocketBuildResult watcherSocket = CreateSocket(
                "SilverFangWatcherSocket", watcher, Vector3.zero,
                Quaternion.Euler(0f, 90f, 0f), blackIron, moon);

            GameObject moonstone = new GameObject("Moonstone");
            moonstone.transform.SetParent(keysRoot, true);
            moonstone.transform.position = new Vector3(-8.35f, 5.75f, -9.3f);
            BuildMoonstone(moonstone.transform, moon, gold);
            ConfigureKey(moonstone, "Moonstone", 0.24f);
            moonstone.SetActive(false);

            // Lock II: returning the Moonstone to the orrery opens a reliquary on the table.
            Transform heavenLock = NewGroup("Lock_02_CelestialConsole", locksRoot);
            heavenLock.position = new Vector3(0f, 0f, 0.30f);
            CreatePart("ConsoleBase", PrimitiveType.Cylinder, heavenLock, new Vector3(0f, 0.28f, 0f),
                new Vector3(0.62f, 0.28f, 0.62f), stone, true);
            CreatePart("ConsoleStem", PrimitiveType.Cylinder, heavenLock, new Vector3(0f, 0.92f, 0f),
                new Vector3(0.24f, 0.66f, 0.24f), blackIron, true);
            CreatePart("MoonBasin", PrimitiveType.Cylinder, heavenLock, new Vector3(0f, 1.48f, 0f),
                new Vector3(0.52f, 0.10f, 0.52f), gold, false);
            SocketBuildResult heavenSocket = CreateSocket(
                "MoonstoneOrrerySocket", heavenLock, new Vector3(0f, 1.62f, 0f),
                Quaternion.identity, gold, moon);

            Transform reliquary = NewGroup("BloodSigilReliquary", locksRoot);
            reliquary.position = new Vector3(5.20f, 1.08f, 3.0f);
            CreatePart("ReliquaryBase", PrimitiveType.Cube, reliquary, Vector3.zero,
                new Vector3(0.75f, 0.28f, 0.55f), darkWood, true);
            Transform lid = NewGroup("ReliquaryLid_Moving", reliquary);
            lid.localPosition = new Vector3(0f, 0.32f, -0.25f);
            CreatePart("Lid", PrimitiveType.Cube, lid, new Vector3(0f, 0f, 0.25f),
                new Vector3(0.82f, 0.10f, 0.58f), gold, false);
            CreatePart("BloodRune", PrimitiveType.Sphere, lid, new Vector3(0f, 0.08f, 0.25f),
                new Vector3(0.18f, 0.05f, 0.18f), blood, false);

            GameObject bloodSigil = new GameObject("BloodSigil");
            bloodSigil.transform.SetParent(keysRoot, true);
            bloodSigil.transform.position = new Vector3(5.20f, 1.52f, 3.0f);
            BuildBloodSigil(bloodSigil.transform, blood, gold, blackIron);
            ConfigureKey(bloodSigil, "BloodSigil", 0.22f);
            bloodSigil.SetActive(false);

            // Lock III reuses the ornate final pedestal but now accepts the Blood Sigil.
            ManorPuzzleSocket oldPuzzle = finalSocketObject.GetComponent<ManorPuzzleSocket>();
            if (oldPuzzle != null)
            {
                Object.DestroyImmediate(oldPuzzle);
            }

            finalSocketObject.name = "BloodSigilDoorSocket";
            XRSocketInteractor finalSocket = finalSocketObject.GetComponent<XRSocketInteractor>();
            Light finalLight = FindSceneObject(scene, "PedestalStatusLight")?.GetComponent<Light>();
            if (finalSocket == null)
            {
                Debug.LogError("Final pedestal is missing its XR Socket Interactor.");
                return;
            }

            finalSocket.enabled = false;

            // In-world directions and scoring board remain visible in VR, not only in the editor.
            Transform board = NewGroup("RitualProgressBoard", cluesRoot);
            board.position = new Vector3(1.55f, 2.65f, -15.28f);
            board.rotation = Quaternion.Euler(0f, 0f, 0f);
            CreatePart("BoardBacking", PrimitiveType.Cube, board, Vector3.zero,
                new Vector3(3.20f, 1.80f, 0.10f), darkWood, false);
            CreatePart("BoardFrameTop", PrimitiveType.Cube, board, new Vector3(0f, 0.96f, 0.07f),
                new Vector3(3.42f, 0.08f, 0.14f), gold, false);
            CreatePart("BoardFrameBottom", PrimitiveType.Cube, board, new Vector3(0f, -0.96f, 0.07f),
                new Vector3(3.42f, 0.08f, 0.14f), gold, false);
            CreatePart("BoardFrameLeft", PrimitiveType.Cube, board, new Vector3(-1.72f, 0f, 0.07f),
                new Vector3(0.08f, 1.92f, 0.14f), gold, false);
            CreatePart("BoardFrameRight", PrimitiveType.Cube, board, new Vector3(1.72f, 0f, 0.07f),
                new Vector3(0.08f, 1.92f, 0.14f), gold, false);
            TextMeshPro progress = CreateWorldText("ProgressText", board, new Vector3(0f, 0.20f, 0.08f),
                new Vector2(2.95f, 0.98f), 1.15f, "RITUAL PROGRESS  0 / 3\nKEYS HIDDEN  2\nLOCKS REMAINING  3\nRITUAL CLUES IN HALL  3", gold);
            TextMeshPro instructions = CreateWorldText("CurrentClueText", board, new Vector3(0f, -0.58f, 0.08f),
                new Vector2(2.95f, 0.52f), 0.72f,
                "STEP I\nTAKE THE SILVER FANG FROM THE TABLE - PLACE IT IN THE GLOWING WATCHER LOCK", moon);

            CreatePlaque(cluesRoot, "Clue_01_Watcher", new Vector3(-7.98f, 2.65f, -9.3f),
                Quaternion.Euler(0f, -90f, 0f), "I  FANG -> WATCHER", gold, darkWood);
            CreatePlaque(cluesRoot, "Clue_02_Heavens", new Vector3(0f, 1.18f, -0.62f),
                Quaternion.Euler(0f, 0f, 0f), "II  MOON -> HEAVENS", gold, darkWood);
            CreatePlaque(cluesRoot, "Clue_03_Exit", new Vector3(0f, 2.65f, 11.02f),
                Quaternion.identity, "III  BLOOD -> EXIT", gold, darkWood);

            ManorKeyArtifact silverArtifact = silverFang.GetComponent<ManorKeyArtifact>();
            ManorKeyArtifact moonArtifact = moonstone.GetComponent<ManorKeyArtifact>();
            ManorKeyArtifact bloodArtifact = bloodSigil.GetComponent<ManorKeyArtifact>();

            ManorThreeStagePuzzle controller = sequenceRoot.gameObject.AddComponent<ManorThreeStagePuzzle>();
            controller.Configure(
                new[] { watcherSocket.Socket, heavenSocket.Socket, finalSocket },
                new[] { "SilverFang", "Moonstone", "BloodSigil" },
                new[] { silverArtifact, moonArtifact, bloodArtifact },
                new Transform[] { movingPortrait, lid, null },
                new[] { movingPortrait.localPosition + Vector3.up * 1.65f, lid.localPosition, Vector3.zero },
                new[] { movingPortrait.localEulerAngles, new Vector3(-112f, 0f, 0f), Vector3.zero },
                new Transform[] { null, movingOrrery.transform, null },
                new[] { Vector3.zero, movingOrrery.transform.localEulerAngles + new Vector3(0f, 270f, 0f), Vector3.zero },
                new[] { watcherSocket.StatusLight, heavenSocket.StatusLight, finalLight },
                progress,
                instructions,
                exitDoor.transform,
                doorSeal.transform,
                celebration);

            ManorPresentationShortcuts shortcuts = Object.FindAnyObjectByType<ManorPresentationShortcuts>();
            if (shortcuts == null)
            {
                shortcuts = celebration.gameObject.AddComponent<ManorPresentationShortcuts>();
            }
            shortcuts.Configure(controller, celebration);

            ApplyEntryMissionWallLayout(scene);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Installed three-stage ritual: Silver Fang, Moonstone, Blood Sigil, then escape.");
        }

        [MenuItem("Tools/Michael Manor/Layout Entry Mission Wall")]
        public static void LayoutEntryMissionWall()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ApplyEntryMissionWallLayout(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Entry view now faces the mission wall with both instruction panels aligned.");
        }

        [MenuItem("Tools/Michael Manor/Test Three Stage Ritual In Play Mode")]
        public static void TestThreeStageRitualInPlayMode()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Enter Play Mode before running the three-stage ritual test.");
                return;
            }

            ManorThreeStagePuzzle puzzle = Object.FindAnyObjectByType<ManorThreeStagePuzzle>();
            if (puzzle == null)
            {
                Debug.LogError("Three-stage ritual controller was not found in Play Mode.");
                return;
            }

            puzzle.SolveAllForPresentation();
            Debug.Log("Started the Play Mode three-stage ritual test.");
        }

        private static SocketBuildResult CreateSocket(
            string name,
            Transform parent,
            Vector3 localPosition,
            Quaternion localRotation,
            Material metal,
            Material glow)
        {
            Transform root = NewGroup(name, parent);
            root.localPosition = localPosition;
            root.localRotation = localRotation;
            SphereCollider trigger = root.gameObject.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 0.38f;
            XRSocketInteractor socket = root.gameObject.AddComponent<XRSocketInteractor>();
            socket.socketSnappingRadius = 0.32f;
            Transform attach = NewGroup("Attach", root);
            socket.attachTransform = attach;
            socket.enabled = false;

            CreateDecorativeRing("LockRing", root, 0.42f, 0.08f, metal);
            GameObject status = new GameObject("StatusLight");
            status.transform.SetParent(root, false);
            status.transform.localPosition = new Vector3(0f, 0.14f, 0f);
            Light light = status.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.32f, 0.72f, 1f);
            light.intensity = 5.2f;
            light.range = 2.4f;
            CreatePart("RuneGlow", PrimitiveType.Sphere, root, new Vector3(0f, 0f, 0.02f),
                Vector3.one * 0.12f, glow, false);
            return new SocketBuildResult { Socket = socket, StatusLight = light };
        }

        private static void ApplyEntryMissionWallLayout(Scene scene)
        {
            GameObject rig = FindSceneObject(scene, "XR Origin (XR Rig)");
            GameObject controls = FindSceneObject(scene, "ControlsCanvas_WorldSpace");
            GameObject boardObject = FindSceneObject(scene, "RitualProgressBoard");
            if (rig == null || controls == null || boardObject == null)
            {
                Debug.LogError("Entry mission wall layout is missing the XR rig or an instruction panel.");
                return;
            }

            rig.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

            controls.transform.position = new Vector3(-1.65f, 2.65f, -15.30f);
            controls.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            controls.transform.localScale = Vector3.one * 0.00285f;
            TextMeshProUGUI controlsText = controls.GetComponentInChildren<TextMeshProUGUI>(true);
            if (controlsText != null)
            {
                controlsText.text =
                    "<size=54><color=#E6B76A>MICHAEL MANOR</color></size>\n" +
                    "<size=31><color=#D7C8B6>VR CONTROLS</color></size>\n\n" +
                    "<size=24>Grip  -  pick up and hold a ritual artifact\n" +
                    "Release inside a glowing ring  -  insert the artifact\n" +
                    "Blue lock light  -  current ritual destination\n" +
                    "Red lock flash  -  wrong artifact or wrong order\n" +
                    "Right Trigger  -  launch orbiting relic\n" +
                    "Left Primary  -  change hall light\n" +
                    "Right Secondary  -  outside view / return\n" +
                    "Right Primary  -  quit</size>";
            }

            Transform board = boardObject.transform;
            board.position = new Vector3(1.55f, 2.65f, -15.28f);
            board.rotation = Quaternion.identity;
            SetLocalTransform(board.Find("BoardBacking"), Vector3.zero, new Vector3(3.20f, 1.80f, 0.10f));
            SetLocalTransform(board.Find("BoardFrameTop"), new Vector3(0f, 0.96f, 0.07f), new Vector3(3.42f, 0.08f, 0.14f));
            SetLocalTransform(board.Find("BoardFrameBottom"), new Vector3(0f, -0.96f, 0.07f), new Vector3(3.42f, 0.08f, 0.14f));

            Material gold = LoadMaterial("AntiqueGold");
            Transform leftFrame = board.Find("BoardFrameLeft");
            if (leftFrame == null)
            {
                leftFrame = CreatePart("BoardFrameLeft", PrimitiveType.Cube, board, Vector3.zero,
                    Vector3.one, gold, false).transform;
            }
            SetLocalTransform(leftFrame, new Vector3(-1.72f, 0f, 0.07f), new Vector3(0.08f, 1.92f, 0.14f));

            Transform rightFrame = board.Find("BoardFrameRight");
            if (rightFrame == null)
            {
                rightFrame = CreatePart("BoardFrameRight", PrimitiveType.Cube, board, Vector3.zero,
                    Vector3.one, gold, false).transform;
            }
            SetLocalTransform(rightFrame, new Vector3(1.72f, 0f, 0.07f), new Vector3(0.08f, 1.92f, 0.14f));

            TextMeshPro progress = board.Find("ProgressText")?.GetComponent<TextMeshPro>();
            if (progress != null)
            {
                progress.transform.localPosition = new Vector3(0f, 0.20f, 0.08f);
                progress.rectTransform.sizeDelta = new Vector2(2.95f, 0.98f);
                progress.fontSize = 1.15f;
                progress.fontStyle = FontStyles.Bold;
                progress.alignment = TextAlignmentOptions.Center;
                progress.color = new Color(1f, 0.76f, 0.28f, 1f);
                progress.enableAutoSizing = true;
                progress.fontSizeMin = 0.82f;
                progress.fontSizeMax = 1.15f;
            }

            TextMeshPro clue = board.Find("CurrentClueText")?.GetComponent<TextMeshPro>();
            if (clue != null)
            {
                clue.text =
                    "STEP I\nTAKE THE SILVER FANG FROM THE TABLE - PLACE IT IN THE GLOWING WATCHER LOCK";
                clue.transform.localPosition = new Vector3(0f, -0.58f, 0.08f);
                clue.rectTransform.sizeDelta = new Vector2(2.95f, 0.52f);
                clue.fontSize = 0.72f;
                clue.fontStyle = FontStyles.Bold;
                clue.alignment = TextAlignmentOptions.Center;
                clue.color = new Color(0.58f, 0.88f, 1f, 1f);
                clue.enableWordWrapping = true;
                clue.enableAutoSizing = true;
                clue.fontSizeMin = 0.48f;
                clue.fontSizeMax = 0.72f;
            }
        }

        private static void SetLocalTransform(Transform target, Vector3 position, Vector3 scale)
        {
            if (target == null)
            {
                return;
            }

            target.localPosition = position;
            target.localScale = scale;
        }

        private static void ConfigureKey(GameObject key, string id, float colliderRadius)
        {
            SphereCollider collider = key.AddComponent<SphereCollider>();
            collider.radius = colliderRadius;
            Rigidbody body = key.AddComponent<Rigidbody>();
            body.mass = 0.28f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            XRGrabInteractable grab = key.AddComponent<XRGrabInteractable>();
            grab.useDynamicAttach = true;
            ManorKeyArtifact artifact = key.AddComponent<ManorKeyArtifact>();
            artifact.Configure(id);
        }

        private static void BuildMoonstone(Transform root, Material glow, Material gold)
        {
            CreatePart("MoonCore", PrimitiveType.Sphere, root, Vector3.zero,
                new Vector3(0.34f, 0.34f, 0.18f), glow, false);
            for (int i = 0; i < 3; i++)
            {
                Transform band = NewGroup("GoldOrbit_" + i, root);
                band.localRotation = Quaternion.Euler(28f + i * 44f, i * 55f, 0f);
                CreateDecorativeRing("BandSegments", band, 0.38f, 0.035f, gold);
            }
        }

        private static void CreateDecorativeRing(
            string name, Transform parent, float radius, float thickness, Material material)
        {
            Transform ring = NewGroup(name, parent);
            const int segmentCount = 16;
            for (int i = 0; i < segmentCount; i++)
            {
                float angle = i * Mathf.PI * 2f / segmentCount;
                GameObject segment = CreatePart(
                    "Segment_" + i,
                    PrimitiveType.Cube,
                    ring,
                    new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f),
                    new Vector3(thickness, radius * 0.20f, thickness),
                    material,
                    false);
                segment.transform.localRotation = Quaternion.Euler(0f, 0f, -angle * Mathf.Rad2Deg);
            }
        }

        private static void BuildBloodSigil(Transform root, Material blood, Material gold, Material iron)
        {
            GameObject disc = CreatePart("SigilDisc", PrimitiveType.Cylinder, root, Vector3.zero,
                new Vector3(0.32f, 0.07f, 0.32f), iron, false);
            disc.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            CreatePart("BloodGem", PrimitiveType.Sphere, root, new Vector3(0f, 0f, -0.10f),
                new Vector3(0.18f, 0.18f, 0.08f), blood, false);
            for (int i = 0; i < 4; i++)
            {
                float angle = i * Mathf.PI * 0.5f;
                CreatePart("GoldRune_" + i, PrimitiveType.Cube, root,
                    new Vector3(Mathf.Cos(angle) * 0.23f, Mathf.Sin(angle) * 0.23f, -0.12f),
                    new Vector3(0.08f, 0.18f, 0.04f), gold, false).transform.localRotation =
                    Quaternion.Euler(0f, 0f, -angle * Mathf.Rad2Deg);
            }
        }

        private static void CreatePlaque(
            Transform parent, string name, Vector3 position, Quaternion rotation,
            string message, Material textMaterial, Material backing)
        {
            Transform plaque = NewGroup(name, parent);
            plaque.position = position;
            plaque.rotation = rotation;
            CreatePart("Backing", PrimitiveType.Cube, plaque, Vector3.zero,
                new Vector3(0.82f, 0.18f, 0.045f), backing, false);
            CreateWorldText("Text", plaque, new Vector3(0f, 0f, -0.07f),
                new Vector2(1.5f, 0.30f), 1.4f, message, textMaterial);
        }

        private static TextMeshPro CreateWorldText(
            string name, Transform parent, Vector3 localPosition, Vector2 size,
            float fontSize, string message, Material colorSource)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            textObject.transform.localPosition = localPosition;
            textObject.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            TextMeshPro text = textObject.AddComponent<TextMeshPro>();
            text.text = message;
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.enableWordWrapping = false;
            text.rectTransform.sizeDelta = size;
            text.color = colorSource != null && colorSource.HasProperty("_BaseColor")
                ? colorSource.GetColor("_BaseColor")
                : Color.white;
            return text;
        }

        private static GameObject CreatePart(
            string name, PrimitiveType type, Transform parent, Vector3 localPosition,
            Vector3 localScale, Material material, bool keepCollider)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            Renderer renderer = part.GetComponent<Renderer>();
            if (renderer != null && material != null)
            {
                renderer.sharedMaterial = material;
            }

            if (!keepCollider)
            {
                Collider collider = part.GetComponent<Collider>();
                if (collider != null)
                {
                    Object.DestroyImmediate(collider);
                }
            }

            return part;
        }

        private static Transform NewGroup(string name, Transform parent)
        {
            Transform group = new GameObject(name).transform;
            group.SetParent(parent, false);
            return group;
        }

        private static Material LoadMaterial(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/MichaelManor/Materials/" + name + ".mat");
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
