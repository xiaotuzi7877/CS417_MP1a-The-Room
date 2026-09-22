using System.Collections.Generic;
using System.IO;
using System.Linq;
using MichaelManor;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace MichaelManorEditor
{
    public static class ManorSceneBuilder
    {
        private const string SourceScenePath = "Assets/Scenes/SampleScene.unity";
        private const string TargetScenePath = "Assets/Scenes/MichaelManorHall.unity";
        private const string MaterialFolder = "Assets/MichaelManor/Materials";

        private static readonly string[] LegacyRootNames =
        {
            "Room",
            "RoomLight",
            "ControlsCanvas",
            "GameManager",
            "Decorations",
            "OutsidePoint",
            "LightBustPoint",
            "LightBurstPoint",
            "InsideBurstPoint",
            "OutsideBurstPoint"
        };

        private static readonly string[] GeneratedRootNames =
        {
            "Manor_Architecture",
            "Manor_Decor",
            "Manor_Lighting",
            "Manor_Puzzle",
            "Manor_Integration",
            "Celestial_Orrery"
        };

        [InitializeOnLoadMethod]
        private static void BuildInitialSceneWhenMissing()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(TargetScenePath) == null)
            {
                EditorApplication.delayCall += RebuildManorHall;
            }
        }

        [MenuItem("Tools/Michael Manor/Rebuild Manor Hall")]
        public static void RebuildManorHall()
        {
            EnsureFolder("Assets", "MichaelManor");
            EnsureFolder("Assets/MichaelManor", "Materials");
            EnsureFolder("Assets", "Scenes");

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(SourceScenePath) == null)
            {
                Debug.LogError($"Source scene not found: {SourceScenePath}");
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(TargetScenePath) == null)
            {
                if (!AssetDatabase.CopyAsset(SourceScenePath, TargetScenePath))
                {
                    Debug.LogError("Could not create the manor scene copy.");
                    return;
                }
            }

            AssetDatabase.Refresh();
            Scene scene = EditorSceneManager.OpenScene(TargetScenePath, OpenSceneMode.Single);

            DeleteRoots(scene, LegacyRootNames);
            DeleteRoots(scene, GeneratedRootNames);

            Material plaster = EnsureMaterial("AgedPlaster", new Color(0.33f, 0.30f, 0.28f), 0f, 0.18f);
            Material damagedPlaster = EnsureMaterial("DamagedPlaster", new Color(0.20f, 0.18f, 0.17f), 0f, 0.10f);
            Material stone = EnsureMaterial("ManorStone", new Color(0.13f, 0.14f, 0.16f), 0f, 0.15f);
            Material darkWood = EnsureMaterial("DarkWood", new Color(0.105f, 0.045f, 0.028f), 0f, 0.30f);
            Material woodHighlight = EnsureMaterial("WoodHighlight", new Color(0.22f, 0.075f, 0.035f), 0f, 0.25f);
            Material velvet = EnsureMaterial("BloodVelvet", new Color(0.27f, 0.008f, 0.018f), 0f, 0.48f);
            Material gold = EnsureMaterial("AntiqueGold", new Color(0.48f, 0.28f, 0.07f), 0.62f, 0.38f);
            Material blackIron = EnsureMaterial("BlackIron", new Color(0.025f, 0.027f, 0.033f), 0.78f, 0.28f);
            Material silver = EnsureMaterial("SilverFang", new Color(0.66f, 0.72f, 0.80f), 0.82f, 0.72f);
            Material portraitRed = EnsureMaterial("PortraitCrimson", new Color(0.24f, 0.025f, 0.035f), 0f, 0.22f);
            Material portraitBlue = EnsureMaterial("PortraitMidnight", new Color(0.025f, 0.055f, 0.12f), 0f, 0.22f);
            Material candleGlow = EnsureMaterial(
                "CandleGlow",
                new Color(0.8f, 0.24f, 0.03f),
                0f,
                0.2f,
                new Color(4.8f, 1.25f, 0.18f));
            Material spectralGlow = EnsureMaterial(
                "SpectralGlow",
                new Color(0.05f, 0.20f, 0.32f),
                0.15f,
                0.45f,
                new Color(0.15f, 1.4f, 3.5f));

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.025f, 0.03f, 0.045f);
            RenderSettings.fogDensity = 0.008f;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.07f, 0.09f, 0.15f);
            RenderSettings.ambientEquatorColor = new Color(0.055f, 0.035f, 0.045f);
            RenderSettings.ambientGroundColor = new Color(0.018f, 0.014f, 0.014f);

            Transform architecture = NewRoot("Manor_Architecture");
            Transform decor = NewRoot("Manor_Decor");
            Transform lighting = NewRoot("Manor_Lighting");
            Transform puzzle = NewRoot("Manor_Puzzle");
            Transform integration = NewRoot("Manor_Integration");

            BuildArchitecture(architecture, plaster, damagedPlaster, stone, darkWood, woodHighlight, blackIron);
            BuildDecor(decor, velvet, darkWood, gold, portraitRed, portraitBlue, stone);
            BuildLighting(lighting, blackIron, gold, candleGlow);
            BuildOrrery(scene, gold, blackIron, spectralGlow);
            BuildPuzzle(puzzle, integration, stone, darkWood, gold, silver, spectralGlow, candleGlow);
            PositionXrRig(scene);
            ConfigureXrEventSystem(scene);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AddSceneToBuildSettings(TargetScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(TargetScenePath);
            Debug.Log("Michael Manor Hall rebuilt successfully. The original SampleScene was preserved.");
        }

        [MenuItem("Tools/Michael Manor/Preview Puzzle Completion")]
        private static void PreviewPuzzleCompletion()
        {
            ManorPuzzleSocket puzzleSocket = Object.FindFirstObjectByType<ManorPuzzleSocket>();
            if (puzzleSocket != null)
            {
                puzzleSocket.SolvePuzzle();
            }
        }

        [MenuItem("Tools/Michael Manor/Preview Puzzle Completion", true)]
        private static bool CanPreviewPuzzleCompletion()
        {
            return EditorApplication.isPlaying;
        }

        private static void BuildArchitecture(
            Transform parent,
            Material plaster,
            Material damagedPlaster,
            Material stone,
            Material darkWood,
            Material woodHighlight,
            Material blackIron)
        {
            CreatePrimitive("Floor_Base", PrimitiveType.Cube, parent, new Vector3(0f, -0.3f, 0f), new Vector3(18f, 0.6f, 32f), stone);
            CreatePrimitive("Floor_Wood", PrimitiveType.Cube, parent, new Vector3(0f, 0.03f, 0f), new Vector3(17.2f, 0.08f, 31.2f), darkWood, false);

            for (int i = -8; i <= 8; i++)
            {
                float x = i;
                CreatePrimitive(
                    $"Floor_Plank_{i + 8:00}",
                    PrimitiveType.Cube,
                    parent,
                    new Vector3(x, 0.085f, 0f),
                    new Vector3(0.035f, 0.012f, 31.1f),
                    woodHighlight,
                    false);
            }

            CreatePrimitive("Wall_Left", PrimitiveType.Cube, parent, new Vector3(-9f, 5f, 0f), new Vector3(0.45f, 10f, 32f), plaster);
            CreatePrimitive("Wall_Right", PrimitiveType.Cube, parent, new Vector3(9f, 5f, 0f), new Vector3(0.45f, 10f, 32f), plaster);
            CreatePrimitive("Wall_Entry", PrimitiveType.Cube, parent, new Vector3(0f, 5f, -16f), new Vector3(18f, 10f, 0.45f), damagedPlaster);

            CreatePrimitive("Wall_Exit_Left", PrimitiveType.Cube, parent, new Vector3(-5.6f, 5f, 16f), new Vector3(6.8f, 10f, 0.45f), damagedPlaster);
            CreatePrimitive("Wall_Exit_Right", PrimitiveType.Cube, parent, new Vector3(5.6f, 5f, 16f), new Vector3(6.8f, 10f, 0.45f), damagedPlaster);
            CreatePrimitive("Wall_Exit_Arch", PrimitiveType.Cube, parent, new Vector3(0f, 8.2f, 16f), new Vector3(4.4f, 3.6f, 0.45f), stone);

            CreatePrimitive("Ceiling", PrimitiveType.Cube, parent, new Vector3(0f, 10.1f, 0f), new Vector3(18.4f, 0.35f, 32.4f), damagedPlaster);

            for (int i = -3; i <= 3; i++)
            {
                float z = i * 4.25f;
                CreatePrimitive($"Ceiling_Beam_{i + 3:00}", PrimitiveType.Cube, parent, new Vector3(0f, 9.72f, z), new Vector3(18.1f, 0.38f, 0.42f), darkWood, false);
            }

            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * 8.66f;
                for (int i = -2; i <= 2; i++)
                {
                    float z = i * 6.2f;
                    CreatePrimitive($"Pilaster_{side}_{i}", PrimitiveType.Cylinder, parent, new Vector3(x, 4.5f, z), new Vector3(0.48f, 4.5f, 0.48f), stone);
                    CreatePrimitive($"PilasterBase_{side}_{i}", PrimitiveType.Cube, parent, new Vector3(x, 0.4f, z), new Vector3(1.2f, 0.8f, 1.2f), stone);
                    CreatePrimitive($"PilasterCap_{side}_{i}", PrimitiveType.Cube, parent, new Vector3(x, 8.6f, z), new Vector3(1.3f, 0.55f, 1.3f), stone, false);
                }

                CreatePrimitive($"Wainscot_{side}", PrimitiveType.Cube, parent, new Vector3(x, 1.65f, 0f), new Vector3(0.18f, 2.8f, 30.8f), darkWood, false);
                CreatePrimitive($"ChairRail_{side}", PrimitiveType.Cube, parent, new Vector3(x - side * 0.05f, 3.1f, 0f), new Vector3(0.22f, 0.18f, 31f), woodHighlight, false);
            }

            Transform door = new GameObject("ExitDoor_Root").transform;
            door.SetParent(parent, false);
            CreatePrimitive("ExitDoor", PrimitiveType.Cube, door, new Vector3(0f, 2.75f, 15.78f), new Vector3(4.35f, 5.5f, 0.42f), darkWood);
            for (int i = -1; i <= 1; i++)
            {
                CreatePrimitive($"DoorIron_{i}", PrimitiveType.Cube, door, new Vector3(i * 1.25f, 2.75f, 15.52f), new Vector3(0.14f, 5.1f, 0.12f), blackIron, false);
            }

            CreatePrimitive("DoorCrossbar", PrimitiveType.Cube, door, new Vector3(0f, 2.75f, 15.48f), new Vector3(4.1f, 0.16f, 0.13f), blackIron, false);
        }

        private static void BuildDecor(
            Transform parent,
            Material velvet,
            Material darkWood,
            Material gold,
            Material portraitRed,
            Material portraitBlue,
            Material stone)
        {
            float[] paintingZ = { -10f, -3.2f, 4.2f, 11f };
            for (int i = 0; i < paintingZ.Length; i++)
            {
                CreatePainting(
                    $"Portrait_Left_{i}",
                    parent,
                    new Vector3(-8.72f, 5.8f, paintingZ[i]),
                    Quaternion.Euler(0f, 90f, 0f),
                    i % 2 == 0 ? portraitRed : portraitBlue,
                    gold);

                CreatePainting(
                    $"Portrait_Right_{i}",
                    parent,
                    new Vector3(8.72f, 5.8f, paintingZ[i] + 1.4f),
                    Quaternion.Euler(0f, -90f, 0f),
                    i % 2 == 0 ? portraitBlue : portraitRed,
                    gold);
            }

            CreateSofa("VelvetSofa_Left", parent, new Vector3(-5.7f, 0f, -1.5f), Quaternion.Euler(0f, 90f, 0f), velvet, darkWood);
            CreateSofa("VelvetSofa_Right", parent, new Vector3(5.7f, 0f, 3.8f), Quaternion.Euler(0f, -90f, 0f), velvet, darkWood);

            Transform table = new GameObject("OccultReadingTable").transform;
            table.SetParent(parent, false);
            table.localPosition = new Vector3(0f, 0f, -5.5f);
            CreatePrimitive("TableTop", PrimitiveType.Cylinder, table, new Vector3(0f, 1.05f, 0f), new Vector3(1.65f, 0.12f, 1.65f), darkWood);
            CreatePrimitive("TableStem", PrimitiveType.Cylinder, table, new Vector3(0f, 0.55f, 0f), new Vector3(0.22f, 0.55f, 0.22f), gold);
            CreatePrimitive("TableBase", PrimitiveType.Cylinder, table, new Vector3(0f, 0.12f, 0f), new Vector3(0.75f, 0.12f, 0.75f), stone);

            CreatePrimitive("Runner", PrimitiveType.Cube, parent, new Vector3(0f, 0.11f, 1.5f), new Vector3(3.4f, 0.025f, 23f), portraitRed, false);
        }

        private static void BuildLighting(Transform parent, Material blackIron, Material gold, Material candleGlow)
        {
            GameObject moonlightObject = new GameObject("Moonlight");
            moonlightObject.transform.SetParent(parent, false);
            moonlightObject.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
            Light moonlight = moonlightObject.AddComponent<Light>();
            moonlight.type = LightType.Directional;
            moonlight.color = new Color(0.27f, 0.38f, 0.62f);
            moonlight.intensity = 0.34f;
            moonlight.shadows = LightShadows.Soft;

            CreateChandelier("Chandelier_North", parent, new Vector3(0f, 8.55f, 6.5f), blackIron, gold, candleGlow);
            CreateChandelier("Chandelier_South", parent, new Vector3(0f, 8.55f, -7.5f), blackIron, gold, candleGlow);

            float[] zPositions = { -11f, -3.5f, 4f, 11f };
            foreach (float z in zPositions)
            {
                CreateSconce(parent, new Vector3(-8.35f, 4.1f, z), candleGlow, gold);
                CreateSconce(parent, new Vector3(8.35f, 4.1f, z), candleGlow, gold);
            }
        }

        private static void BuildOrrery(Scene scene, Material gold, Material blackIron, Material spectralGlow)
        {
            Transform orrery = NewRoot("Celestial_Orrery");
            orrery.position = new Vector3(0f, 8.05f, 0f);

            CreateOrbitRing("OrbitRing_Horizontal", orrery, Quaternion.identity, 3.7f, gold);
            CreateOrbitRing("OrbitRing_TiltA", orrery, Quaternion.Euler(62f, 0f, 18f), 3.15f, gold);
            CreateOrbitRing("OrbitRing_TiltB", orrery, Quaternion.Euler(0f, 0f, 72f), 2.65f, blackIron);

            GameObject sun = CreatePrimitive("Sun", PrimitiveType.Sphere, orrery, Vector3.zero, Vector3.one * 0.8f, spectralGlow, false);
            Light sunLight = sun.AddComponent<Light>();
            sunLight.type = LightType.Point;
            sunLight.color = new Color(0.15f, 0.55f, 1f);
            sunLight.intensity = 320f;
            sunLight.range = 8f;

            GameObject planet = FindRoot(scene, "Planet");
            if (planet != null)
            {
                planet.transform.SetParent(orrery, false);
                planet.transform.localPosition = new Vector3(2.45f, 0f, 0f);
                planet.transform.localScale = Vector3.one * 0.7f;
                Renderer renderer = planet.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = spectralGlow;
                }

                Transform moon = planet.transform.Find("Moon");
                if (moon != null)
                {
                    moon.localPosition = new Vector3(1.35f, 0f, 0f);
                    moon.localScale = Vector3.one * 0.35f;
                }
            }

            GameObject comet = FindRoot(scene, "Comet");
            if (comet != null)
            {
                comet.transform.SetParent(orrery, false);
                comet.transform.localPosition = new Vector3(4.25f, 0.1f, 0f);
                comet.transform.localScale = Vector3.one * 0.24f;
                OrbitComet orbitComet = comet.GetComponent<OrbitComet>();
                if (orbitComet != null)
                {
                    orbitComet.planet = sun.transform;
                    orbitComet.gravity = 0.32f;
                }
            }
        }

        private static void BuildPuzzle(
            Transform puzzleParent,
            Transform integrationParent,
            Material stone,
            Material darkWood,
            Material gold,
            Material silver,
            Material spectralGlow,
            Material candleGlow)
        {
            Transform pedestal = new GameObject("SilverFang_Pedestal").transform;
            pedestal.SetParent(puzzleParent, false);
            pedestal.localPosition = new Vector3(0f, 0f, 10.6f);
            CreatePrimitive("PedestalBase", PrimitiveType.Cylinder, pedestal, new Vector3(0f, 0.3f, 0f), new Vector3(1.1f, 0.3f, 1.1f), stone);
            CreatePrimitive("PedestalStem", PrimitiveType.Cylinder, pedestal, new Vector3(0f, 1.05f, 0f), new Vector3(0.48f, 0.78f, 0.48f), stone);
            CreatePrimitive("PedestalTop", PrimitiveType.Cylinder, pedestal, new Vector3(0f, 1.72f, 0f), new Vector3(0.9f, 0.14f, 0.9f), gold);

            GameObject socketObject = new GameObject("SilverFangSocket");
            socketObject.transform.SetParent(pedestal, false);
            socketObject.transform.localPosition = new Vector3(0f, 2.02f, 0f);
            SphereCollider trigger = socketObject.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 0.55f;
            XRSocketInteractor socket = socketObject.AddComponent<XRSocketInteractor>();
            socket.socketSnappingRadius = 0.25f;
            Transform attach = new GameObject("Attach").transform;
            attach.SetParent(socketObject.transform, false);
            attach.localRotation = Quaternion.Euler(0f, 0f, -15f);
            socket.attachTransform = attach;

            GameObject statusLightObject = new GameObject("PedestalStatusLight");
            statusLightObject.transform.SetParent(pedestal, false);
            statusLightObject.transform.localPosition = new Vector3(0f, 2.15f, 0f);
            Light statusLight = statusLightObject.AddComponent<Light>();
            statusLight.type = LightType.Point;
            statusLight.color = new Color(0.35f, 0.05f, 0.05f);
            statusLight.intensity = 85f;
            statusLight.range = 3.2f;

            Transform successFeedback = new GameObject("PedestalSolvedGlow").transform;
            successFeedback.SetParent(pedestal, false);
            successFeedback.localPosition = new Vector3(0f, 2.05f, 0f);
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI * 2f / 8f;
                CreatePrimitive(
                    $"Rune_{i:00}",
                    PrimitiveType.Sphere,
                    successFeedback,
                    new Vector3(Mathf.Cos(angle) * 0.72f, 0f, Mathf.Sin(angle) * 0.72f),
                    Vector3.one * 0.12f,
                    spectralGlow,
                    false);
            }

            successFeedback.gameObject.SetActive(false);

            GameObject fang = CreatePrimitive(
                "SilverFang",
                PrimitiveType.Capsule,
                puzzleParent,
                new Vector3(-5.2f, 1.35f, -10.5f),
                new Vector3(0.24f, 0.68f, 0.24f),
                silver);
            fang.transform.rotation = Quaternion.Euler(0f, 0f, -18f);
            Rigidbody fangBody = fang.AddComponent<Rigidbody>();
            fangBody.mass = 0.2f;
            fangBody.interpolation = RigidbodyInterpolation.Interpolate;
            fangBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            fang.AddComponent<XRGrabInteractable>();
            ManorKeyArtifact artifact = fang.AddComponent<ManorKeyArtifact>();
            artifact.Configure("SilverFang");

            Transform display = new GameObject("FangDisplayTable").transform;
            display.SetParent(puzzleParent, false);
            display.localPosition = new Vector3(-5.2f, 0f, -10.5f);
            CreatePrimitive("DisplayTop", PrimitiveType.Cylinder, display, new Vector3(0f, 1.0f, 0f), new Vector3(0.72f, 0.12f, 0.72f), darkWood);
            CreatePrimitive("DisplayStem", PrimitiveType.Cylinder, display, new Vector3(0f, 0.52f, 0f), new Vector3(0.16f, 0.5f, 0.16f), gold);
            CreatePrimitive("DisplayBase", PrimitiveType.Cylinder, display, new Vector3(0f, 0.12f, 0f), new Vector3(0.48f, 0.12f, 0.48f), stone);

            GameObject doorObject = GameObject.Find("ExitDoor_Root");
            ManorPuzzleSocket puzzleSocket = socketObject.AddComponent<ManorPuzzleSocket>();
            puzzleSocket.Configure(socket, "SilverFang", doorObject != null ? doorObject.transform : null, successFeedback.gameObject, statusLight);

            Transform celebrationRoot = new GameObject("WinCelebration_Visuals").transform;
            celebrationRoot.SetParent(integrationParent, false);
            celebrationRoot.localPosition = Vector3.zero;

            GameObject winTextObject = new GameObject("WinText");
            winTextObject.transform.SetParent(celebrationRoot, false);
            winTextObject.transform.localPosition = new Vector3(0f, 6.25f, 14.85f);
            winTextObject.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            TextMeshPro winText = winTextObject.AddComponent<TextMeshPro>();
            winText.text = "YOU ESCAPED THE MANOR";
            winText.fontSize = 2.2f;
            winText.alignment = TextAlignmentOptions.Center;
            winText.color = new Color(0.78f, 0.90f, 1f);
            winText.rectTransform.sizeDelta = new Vector2(12f, 2.2f);

            ParticleSystem leftBurst = CreateCelebrationParticles("VictoryBurst_Left", celebrationRoot, new Vector3(-2.4f, 2.4f, 13.2f), spectralGlow);
            ParticleSystem rightBurst = CreateCelebrationParticles("VictoryBurst_Right", celebrationRoot, new Vector3(2.4f, 2.4f, 13.2f), candleGlow);

            GameObject victoryLightObject = new GameObject("VictoryLight");
            victoryLightObject.transform.SetParent(celebrationRoot, false);
            victoryLightObject.transform.localPosition = new Vector3(0f, 4.5f, 13.5f);
            Light victoryLight = victoryLightObject.AddComponent<Light>();
            victoryLight.type = LightType.Point;
            victoryLight.color = new Color(0.25f, 0.65f, 1f);
            victoryLight.intensity = 900f;
            victoryLight.range = 12f;

            WinCelebrationController celebration = integrationParent.gameObject.AddComponent<WinCelebrationController>();
            celebration.Configure(celebrationRoot.gameObject, new[] { leftBurst, rightBurst });
            UnityEventTools.AddPersistentListener(puzzleSocket.OnSolved, celebration.TriggerWin);
            celebrationRoot.gameObject.SetActive(false);
        }

        private static void PositionXrRig(Scene scene)
        {
            GameObject xrRig = scene.GetRootGameObjects().FirstOrDefault(go => go.name.StartsWith("XR Origin"));
            if (xrRig == null)
            {
                Debug.LogWarning("XR Origin was not found in the copied scene.");
                return;
            }

            xrRig.transform.position = new Vector3(0f, 0f, -12.5f);
            xrRig.transform.rotation = Quaternion.identity;
        }

        private static void ConfigureXrEventSystem(Scene scene)
        {
            GameObject eventSystem = FindRoot(scene, "EventSystem");
            if (eventSystem == null)
            {
                return;
            }

            InputSystemUIInputModule desktopModule = eventSystem.GetComponent<InputSystemUIInputModule>();
            if (desktopModule != null)
            {
                Object.DestroyImmediate(desktopModule);
            }

            if (eventSystem.GetComponent<XRUIInputModule>() == null)
            {
                eventSystem.AddComponent<XRUIInputModule>();
            }
        }

        private static void CreatePainting(
            string name,
            Transform parent,
            Vector3 position,
            Quaternion rotation,
            Material portrait,
            Material frame)
        {
            Transform root = new GameObject(name).transform;
            root.SetParent(parent, false);
            root.position = position;
            root.rotation = rotation;
            CreatePrimitive("Canvas", PrimitiveType.Cube, root, Vector3.zero, new Vector3(2.65f, 2.15f, 0.10f), portrait, false);
            CreatePrimitive("FrameTop", PrimitiveType.Cube, root, new Vector3(0f, 1.2f, -0.08f), new Vector3(3.15f, 0.18f, 0.18f), frame, false);
            CreatePrimitive("FrameBottom", PrimitiveType.Cube, root, new Vector3(0f, -1.2f, -0.08f), new Vector3(3.15f, 0.18f, 0.18f), frame, false);
            CreatePrimitive("FrameLeft", PrimitiveType.Cube, root, new Vector3(-1.48f, 0f, -0.08f), new Vector3(0.18f, 2.55f, 0.18f), frame, false);
            CreatePrimitive("FrameRight", PrimitiveType.Cube, root, new Vector3(1.48f, 0f, -0.08f), new Vector3(0.18f, 2.55f, 0.18f), frame, false);
        }

        private static void CreateSofa(
            string name,
            Transform parent,
            Vector3 position,
            Quaternion rotation,
            Material velvet,
            Material wood)
        {
            Transform root = new GameObject(name).transform;
            root.SetParent(parent, false);
            root.position = position;
            root.rotation = rotation;
            CreatePrimitive("Seat", PrimitiveType.Cube, root, new Vector3(0f, 0.72f, 0f), new Vector3(3.4f, 0.48f, 1.15f), velvet);
            CreatePrimitive("Back", PrimitiveType.Cube, root, new Vector3(0f, 1.45f, 0.48f), new Vector3(3.4f, 1.55f, 0.30f), velvet);
            CreatePrimitive("ArmLeft", PrimitiveType.Cube, root, new Vector3(-1.65f, 1.0f, 0f), new Vector3(0.32f, 0.85f, 1.25f), velvet);
            CreatePrimitive("ArmRight", PrimitiveType.Cube, root, new Vector3(1.65f, 1.0f, 0f), new Vector3(0.32f, 0.85f, 1.25f), velvet);
            CreatePrimitive("LegLeft", PrimitiveType.Cube, root, new Vector3(-1.28f, 0.23f, 0f), new Vector3(0.22f, 0.46f, 0.22f), wood);
            CreatePrimitive("LegRight", PrimitiveType.Cube, root, new Vector3(1.28f, 0.23f, 0f), new Vector3(0.22f, 0.46f, 0.22f), wood);
        }

        private static void CreateChandelier(
            string name,
            Transform parent,
            Vector3 position,
            Material iron,
            Material gold,
            Material glow)
        {
            Transform root = new GameObject(name).transform;
            root.SetParent(parent, false);
            root.position = position;
            CreatePrimitive("Chain", PrimitiveType.Cylinder, root, new Vector3(0f, 0.8f, 0f), new Vector3(0.07f, 0.85f, 0.07f), iron, false);
            CreatePrimitive("Hub", PrimitiveType.Sphere, root, Vector3.zero, Vector3.one * 0.36f, gold, false);

            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI * 2f / 8f;
                Vector3 armPosition = new Vector3(Mathf.Cos(angle) * 1.4f, -0.1f, Mathf.Sin(angle) * 1.4f);
                GameObject candle = CreatePrimitive($"Candle_{i:00}", PrimitiveType.Cylinder, root, armPosition, new Vector3(0.11f, 0.32f, 0.11f), iron, false);
                CreatePrimitive("Flame", PrimitiveType.Sphere, candle.transform, new Vector3(0f, 0.8f, 0f), new Vector3(0.55f, 0.32f, 0.55f), glow, false);
            }

            GameObject lightObject = new GameObject("WarmLight");
            lightObject.transform.SetParent(root, false);
            lightObject.transform.localPosition = new Vector3(0f, -0.15f, 0f);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.48f, 0.20f);
            light.intensity = 620f;
            light.range = 13f;
            // Point-light shadows require six shadow maps per chandelier. Keep the
            // atmospheric directional shadow while avoiding that cost in VR.
            light.shadows = LightShadows.None;
            lightObject.AddComponent<ManorLightFlicker>();
        }

        private static void CreateSconce(Transform parent, Vector3 position, Material glow, Material metal)
        {
            Transform root = new GameObject($"Sconce_{position.x}_{position.z}").transform;
            root.SetParent(parent, false);
            root.position = position;
            CreatePrimitive("Mount", PrimitiveType.Cube, root, Vector3.zero, new Vector3(0.20f, 0.75f, 0.55f), metal, false);
            CreatePrimitive("Flame", PrimitiveType.Sphere, root, new Vector3(0f, 0.65f, 0f), new Vector3(0.20f, 0.34f, 0.20f), glow, false);
            Light light = root.gameObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.37f, 0.14f);
            light.intensity = 180f;
            light.range = 6.5f;
            root.gameObject.AddComponent<ManorLightFlicker>();
        }

        private static ParticleSystem CreateCelebrationParticles(string name, Transform parent, Vector3 position, Material material)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            ParticleSystem particles = go.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.loop = false;
            main.duration = 2.5f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.6f, 3.2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2.2f, 5.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.18f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.4f, 0.75f, 1f), new Color(1f, 0.25f, 0.1f));
            main.maxParticles = 220;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 110) });

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 28f;
            shape.radius = 0.45f;

            ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            return particles;
        }

        private static void CreateOrbitRing(string name, Transform parent, Quaternion rotation, float radius, Material material)
        {
            GameObject ringObject = new GameObject(name);
            ringObject.transform.SetParent(parent, false);
            ringObject.transform.localRotation = rotation;
            LineRenderer ring = ringObject.AddComponent<LineRenderer>();
            ring.useWorldSpace = false;
            ring.loop = true;
            ring.positionCount = 64;
            ring.startWidth = 0.045f;
            ring.endWidth = 0.045f;
            ring.sharedMaterial = material;
            for (int i = 0; i < ring.positionCount; i++)
            {
                float angle = i * Mathf.PI * 2f / ring.positionCount;
                ring.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
            }
        }

        private static GameObject CreatePrimitive(
            string name,
            PrimitiveType type,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Material material,
            bool keepCollider = true)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = localScale;
            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            if (!keepCollider)
            {
                Collider collider = go.GetComponent<Collider>();
                if (collider != null)
                {
                    UnityEngine.Object.DestroyImmediate(collider);
                }
            }

            return go;
        }

        private static Material EnsureMaterial(
            string name,
            Color color,
            float metallic,
            float smoothness,
            Color? emission = null)
        {
            string path = $"{MaterialFolder}/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }

            material.SetColor("_BaseColor", color);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            if (emission.HasValue)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emission.Value);
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            else
            {
                material.DisableKeyword("_EMISSION");
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static void DeleteRoots(Scene scene, IEnumerable<string> names)
        {
            HashSet<string> nameSet = new HashSet<string>(names);
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (nameSet.Contains(root.name))
                {
                    UnityEngine.Object.DestroyImmediate(root);
                }
            }
        }

        private static Transform NewRoot(string name)
        {
            return new GameObject(name).transform;
        }

        private static GameObject FindRoot(Scene scene, string name)
        {
            return scene.GetRootGameObjects().FirstOrDefault(go => go.name == name);
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = Path.Combine(parent, child).Replace('\\', '/');
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.All(scene => scene.path != scenePath))
            {
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }
        }
    }
}
