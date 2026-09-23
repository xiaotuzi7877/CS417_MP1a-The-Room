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
        private const string WornPlasterFolder = "Assets/MichaelManor/Textures/PolyHaven/WornPlasterWall";
        private const string WornWoodFloorFolder = "Assets/MichaelManor/Textures/PolyHaven/WoodFloorWorn";
        private const string FurnitureFolder = "Assets/MichaelManor/ThirdParty/PolyHaven/Models";
        private const string GothicTableFolder = FurnitureFolder + "/GothicCoffeeTable";
        private const string WoodenChairFolder = FurnitureFolder + "/WoodenChair01";
        private const string WoodenSofaFolder = FurnitureFolder + "/PaintedWoodenSofa";
        private const string WoodenCabinetFolder = FurnitureFolder + "/PaintedWoodenCabinet";
        private const string LanternFolder = FurnitureFolder + "/LanternChandelier01";
        private const string LanternModelPath = LanternFolder + "/lantern_chandelier_01_1k.fbx";

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
            "Celestial_Orrery",
            "Architecture",
            "Furniture",
            "Decor",
            "Lighting",
            "Puzzle",
            "Systems",
            "ExitDoor"
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
            ConfigurePbrTextureImports();
            Scene scene = EditorSceneManager.OpenScene(TargetScenePath, OpenSceneMode.Single);

            PreserveSourceObjectsFromSystems(scene);
            DeleteRoots(scene, LegacyRootNames);
            DeleteRoots(scene, GeneratedRootNames);

            Material plaster = EnsureTexturedMaterial(
                "AgedPlaster",
                new Color(0.62f, 0.58f, 0.54f),
                0.08f,
                0.18f,
                new Vector2(6f, 3f),
                $"{WornPlasterFolder}/worn_plaster_wall_diff_2k.jpg",
                $"{WornPlasterFolder}/worn_plaster_wall_nor_gl_2k.jpg",
                $"{WornPlasterFolder}/worn_plaster_wall_ao_2k.jpg");
            Material damagedPlaster = EnsureTexturedMaterial(
                "DamagedPlaster",
                new Color(0.40f, 0.35f, 0.33f),
                0.12f,
                0.12f,
                new Vector2(5f, 3f),
                $"{WornPlasterFolder}/worn_plaster_wall_diff_2k.jpg",
                $"{WornPlasterFolder}/worn_plaster_wall_nor_gl_2k.jpg",
                $"{WornPlasterFolder}/worn_plaster_wall_ao_2k.jpg");
            Material stone = EnsureMaterial("ManorStone", new Color(0.13f, 0.14f, 0.16f), 0f, 0.15f);
            Material floorWood = EnsureTexturedMaterial(
                "WornWoodFloor",
                new Color(0.52f, 0.39f, 0.31f),
                0.04f,
                0.24f,
                new Vector2(6f, 10f),
                $"{WornWoodFloorFolder}/wood_floor_worn_diff_2k.jpg",
                $"{WornWoodFloorFolder}/wood_floor_worn_nor_gl_2k.jpg",
                $"{WornWoodFloorFolder}/wood_floor_worn_ao_2k.jpg");
            Material darkWood = EnsureMaterial("DarkWood", new Color(0.105f, 0.045f, 0.028f), 0f, 0.30f);
            Material woodHighlight = EnsureMaterial("WoodHighlight", new Color(0.22f, 0.075f, 0.035f), 0f, 0.25f);
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
            Material orbitGlow = EnsureMaterial(
                "CelestialOrbitGlow",
                new Color(0.42f, 0.30f, 0.10f),
                0.55f,
                0.62f,
                new Color(1.8f, 0.72f, 0.12f));
            Material celestialSun = EnsureMaterial(
                "CelestialSun",
                new Color(0.88f, 0.25f, 0.025f),
                0.05f,
                0.50f,
                new Color(5.2f, 1.25f, 0.12f));
            Material celestialPlanet = EnsureMaterial(
                "CelestialPlanet",
                new Color(0.025f, 0.20f, 0.58f),
                0.18f,
                0.58f,
                new Color(0.08f, 0.62f, 2.2f));
            Material celestialMoon = EnsureMaterial(
                "CelestialMoon",
                new Color(0.58f, 0.64f, 0.72f),
                0.42f,
                0.66f,
                new Color(0.58f, 0.72f, 1.15f));
            Material celestialComet = EnsureMaterial(
                "CelestialComet",
                new Color(0.46f, 0.018f, 0.09f),
                0.12f,
                0.48f,
                new Color(2.8f, 0.08f, 0.22f));
            Material gothicTable = EnsureFurnitureMaterial(
                "Furniture_GothicTable",
                new Color(0.72f, 0.62f, 0.56f),
                0.08f,
                0.32f,
                $"{GothicTableFolder}/textures/gothic_coffee_table_diff_1k.jpg",
                $"{GothicTableFolder}/textures/gothic_coffee_table_nor_gl_1k.exr");
            Material woodenChair = EnsureFurnitureMaterial(
                "Furniture_WoodenChair",
                new Color(0.68f, 0.58f, 0.52f),
                0.06f,
                0.28f,
                $"{WoodenChairFolder}/textures/WoodenChair_01_diff_1k.jpg",
                $"{WoodenChairFolder}/textures/WoodenChair_01_nor_gl_1k.exr");
            Material woodenSofa = EnsureFurnitureMaterial(
                "Furniture_WoodenSofa",
                new Color(0.74f, 0.52f, 0.50f),
                0.04f,
                0.24f,
                $"{WoodenSofaFolder}/textures/painted_wooden_sofa_diff_1k.jpg",
                $"{WoodenSofaFolder}/textures/painted_wooden_sofa_nor_gl_1k.exr");
            Material woodenCabinet = EnsureFurnitureMaterial(
                "Furniture_WoodenCabinet",
                new Color(0.70f, 0.56f, 0.48f),
                0.10f,
                0.26f,
                $"{WoodenCabinetFolder}/textures/painted_wooden_cabinet_diff_1k.jpg",
                $"{WoodenCabinetFolder}/textures/painted_wooden_cabinet_nor_gl_1k.exr");
            Material lanternMetal = EnsureLanternMetalMaterial();
            Material lanternGlass = EnsureLanternGlassMaterial();

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.025f, 0.03f, 0.045f);
            RenderSettings.fogDensity = 0.008f;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.07f, 0.09f, 0.15f);
            RenderSettings.ambientEquatorColor = new Color(0.055f, 0.035f, 0.045f);
            RenderSettings.ambientGroundColor = new Color(0.018f, 0.014f, 0.014f);

            Transform architecture = NewRoot("Architecture");
            Transform furniture = NewRoot("Furniture");
            Transform decor = NewRoot("Decor");
            Transform lighting = NewRoot("Lighting");
            Transform puzzle = NewRoot("Puzzle");
            Transform systems = NewRoot("Systems");
            Transform exitDoor = NewRoot("ExitDoor");
            Transform winFlow = NewGroup("WinFlow", systems);

            BuildArchitecture(architecture, exitDoor, plaster, damagedPlaster, stone, floorWood, darkWood, woodHighlight, blackIron);
            BuildDecor(decor, furniture, gold, portraitRed, portraitBlue, gothicTable, woodenChair, woodenSofa, woodenCabinet);
            BuildLighting(lighting, blackIron, gold, candleGlow, lanternMetal, lanternGlass);
            BuildOrrery(
                gold,
                blackIron,
                orbitGlow,
                celestialSun,
                celestialPlanet,
                celestialMoon,
                celestialComet);
            BuildPuzzle(puzzle, winFlow, stone, darkWood, gold, silver, spectralGlow, candleGlow);
            PositionXrRig(scene);
            ConfigureXrEventSystem(scene);
            MoveSourceObjectUnder(scene, "Global Volume", systems);
            MoveSourceObjectUnder(scene, "EventSystem", systems);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AddSceneToBuildSettings(TargetScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(TargetScenePath);
            Debug.Log("Michael Manor Hall rebuilt successfully. The original SampleScene was preserved.");
        }

        [MenuItem("Tools/Michael Manor/Realign Portraits")]
        public static void RealignPortraits()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                Debug.LogError("Open Michael Manor Hall before realigning its portraits.");
                return;
            }

            float[] paintingZ = { -9.3f, -3.1f, 3.1f, 9.3f };
            int portraitsUpdated = 0;

            for (int i = 0; i < paintingZ.Length; i++)
            {
                portraitsUpdated += RealignPortrait(
                    $"Portrait_Left_{i}",
                    new Vector3(-8.72f, 5.8f, paintingZ[i]),
                    Quaternion.Euler(0f, 90f, 0f));
                portraitsUpdated += RealignPortrait(
                    $"Portrait_Right_{i}",
                    new Vector3(8.72f, 5.8f, paintingZ[i]),
                    Quaternion.Euler(0f, -90f, 0f));
            }

            if (portraitsUpdated == 0)
            {
                Debug.LogError("No manor portraits were found in the open scene.");
                return;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"Realigned {portraitsUpdated} portraits between the wall columns.");
        }

        private static int RealignPortrait(string objectName, Vector3 position, Quaternion rotation)
        {
            GameObject portrait = GameObject.Find(objectName);
            if (portrait == null)
            {
                Debug.LogWarning($"Could not find portrait: {objectName}");
                return 0;
            }

            Undo.RecordObject(portrait.transform, "Realign Manor Portraits");
            portrait.transform.SetPositionAndRotation(position, rotation);
            return 1;
        }

        [MenuItem("Tools/Michael Manor/Install Gothic Wall Lanterns")]
        public static void InstallGothicWallLanterns()
        {
            Scene scene = SceneManager.GetActiveScene();
            GameObject wallSconcesObject = GameObject.Find("Wall_Sconces");
            if (!scene.IsValid() || !scene.isLoaded || wallSconcesObject == null)
            {
                Debug.LogError("Open Michael Manor Hall before installing its wall lanterns.");
                return;
            }

            AssetDatabase.Refresh();
            ConfigureLanternImports();
            GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(LanternModelPath);
            if (modelAsset == null)
            {
                Debug.LogError($"Gothic lantern model was not found: {LanternModelPath}");
                return;
            }

            Material blackIron = EnsureMaterial("BlackIron", new Color(0.025f, 0.027f, 0.033f), 0.78f, 0.28f);
            Material gold = EnsureMaterial("AntiqueGold", new Color(0.48f, 0.28f, 0.07f), 0.62f, 0.38f);
            Material lanternMetal = EnsureLanternMetalMaterial();
            Material lanternGlass = EnsureLanternGlassMaterial();
            Transform wallSconces = wallSconcesObject.transform;

            while (wallSconces.childCount > 0)
            {
                Undo.DestroyObjectImmediate(wallSconces.GetChild(0).gameObject);
            }

            float[] zPositions = { -9.3f, -3.1f, 3.1f, 9.3f };
            foreach (float z in zPositions)
            {
                CreateSconce(wallSconces, new Vector3(-8.35f, 3.9f, z), blackIron, gold, lanternMetal, lanternGlass);
                CreateSconce(wallSconces, new Vector3(8.35f, 3.9f, z), blackIron, gold, lanternMetal, lanternGlass);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Installed eight gothic wall lanterns with warm VR-friendly lighting.");
        }

        [MenuItem("Tools/Michael Manor/Preview Puzzle Completion")]
        private static void PreviewPuzzleCompletion()
        {
            ManorPuzzleSocket puzzleSocket = Object.FindAnyObjectByType<ManorPuzzleSocket>();
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
            Transform door,
            Material plaster,
            Material damagedPlaster,
            Material stone,
            Material floorWood,
            Material darkWood,
            Material woodHighlight,
            Material blackIron)
        {
            Transform floors = NewGroup("Floors", parent);
            Transform walls = NewGroup("Walls", parent);
            Transform ceiling = NewGroup("Ceiling", parent);
            Transform wallDetails = NewGroup("Wall_Details", parent);

            CreatePrimitive("Floor_Base", PrimitiveType.Cube, floors, new Vector3(0f, -0.3f, 0f), new Vector3(18f, 0.6f, 32f), stone);
            CreatePrimitive("Floor_Wood", PrimitiveType.Cube, floors, new Vector3(0f, 0.03f, 0f), new Vector3(17.2f, 0.08f, 31.2f), floorWood, false);
            CreatePrimitive("FloorBorder_Left", PrimitiveType.Cube, floors, new Vector3(-8.45f, 0.10f, 0f), new Vector3(0.22f, 0.12f, 31.1f), darkWood, false);
            CreatePrimitive("FloorBorder_Right", PrimitiveType.Cube, floors, new Vector3(8.45f, 0.10f, 0f), new Vector3(0.22f, 0.12f, 31.1f), darkWood, false);

            CreatePrimitive("Wall_Left", PrimitiveType.Cube, walls, new Vector3(-9f, 6.6f, 0f), new Vector3(0.45f, 13.2f, 32f), plaster);
            CreatePrimitive("Wall_Right", PrimitiveType.Cube, walls, new Vector3(9f, 6.6f, 0f), new Vector3(0.45f, 13.2f, 32f), plaster);
            CreatePrimitive("Wall_Entry", PrimitiveType.Cube, walls, new Vector3(0f, 6.6f, -16f), new Vector3(18f, 13.2f, 0.45f), damagedPlaster);

            CreatePrimitive("Wall_Exit_Left", PrimitiveType.Cube, walls, new Vector3(-5.6f, 6.6f, 16f), new Vector3(6.8f, 13.2f, 0.45f), damagedPlaster);
            CreatePrimitive("Wall_Exit_Right", PrimitiveType.Cube, walls, new Vector3(5.6f, 6.6f, 16f), new Vector3(6.8f, 13.2f, 0.45f), damagedPlaster);
            CreatePrimitive("Wall_Exit_Arch", PrimitiveType.Cube, walls, new Vector3(0f, 10.1f, 16f), new Vector3(4.4f, 6.2f, 0.45f), stone);

            CreatePrimitive("Ceiling_Surface", PrimitiveType.Cube, ceiling, new Vector3(0f, 13.35f, 0f), new Vector3(18.4f, 0.4f, 32.4f), damagedPlaster);

            float[] beamPositions = { -13f, -8.7f, -4.35f, 4.35f, 8.7f, 13f };
            for (int i = 0; i < beamPositions.Length; i++)
            {
                CreatePrimitive($"Ceiling_Beam_{i:00}", PrimitiveType.Cube, ceiling, new Vector3(0f, 12.92f, beamPositions[i]), new Vector3(18.1f, 0.42f, 0.48f), darkWood, false);
            }

            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * 8.66f;
                for (int i = -2; i <= 2; i++)
                {
                    float z = i * 6.2f;
                    CreatePrimitive($"Pilaster_{side}_{i}", PrimitiveType.Cylinder, wallDetails, new Vector3(x, 5.9f, z), new Vector3(0.48f, 5.9f, 0.48f), stone);
                    CreatePrimitive($"PilasterBase_{side}_{i}", PrimitiveType.Cube, wallDetails, new Vector3(x, 0.4f, z), new Vector3(1.2f, 0.8f, 1.2f), stone);
                    CreatePrimitive($"PilasterCap_{side}_{i}", PrimitiveType.Cube, wallDetails, new Vector3(x, 11.55f, z), new Vector3(1.3f, 0.55f, 1.3f), stone, false);
                }

                CreatePrimitive($"Wainscot_{side}", PrimitiveType.Cube, wallDetails, new Vector3(x, 1.65f, 0f), new Vector3(0.18f, 2.8f, 30.8f), darkWood, false);
                CreatePrimitive($"ChairRail_{side}", PrimitiveType.Cube, wallDetails, new Vector3(x - side * 0.05f, 3.1f, 0f), new Vector3(0.22f, 0.18f, 31f), woodHighlight, false);
                CreatePrimitive($"UpperCornice_{side}", PrimitiveType.Cube, wallDetails, new Vector3(x - side * 0.05f, 11.95f, 0f), new Vector3(0.28f, 0.28f, 31f), woodHighlight, false);
            }

            CreatePrimitive("DoorPanel", PrimitiveType.Cube, door, new Vector3(0f, 2.75f, 15.78f), new Vector3(4.35f, 5.5f, 0.42f), darkWood);
            for (int i = -1; i <= 1; i++)
            {
                CreatePrimitive($"DoorIron_{i}", PrimitiveType.Cube, door, new Vector3(i * 1.25f, 2.75f, 15.52f), new Vector3(0.14f, 5.1f, 0.12f), blackIron, false);
            }

            CreatePrimitive("DoorCrossbar", PrimitiveType.Cube, door, new Vector3(0f, 2.75f, 15.48f), new Vector3(4.1f, 0.16f, 0.13f), blackIron, false);
        }

        private static void BuildDecor(
            Transform decorParent,
            Transform furnitureParent,
            Material gold,
            Material portraitRed,
            Material portraitBlue,
            Material gothicTable,
            Material woodenChair,
            Material woodenSofa,
            Material woodenCabinet)
        {
            Transform portraits = NewGroup("Portraits", decorParent);
            Transform floorDecor = NewGroup("Floor_Decor", decorParent);
            float[] paintingZ = { -9.3f, -3.1f, 3.1f, 9.3f };
            for (int i = 0; i < paintingZ.Length; i++)
            {
                CreatePainting(
                    $"Portrait_Left_{i}",
                    portraits,
                    new Vector3(-8.72f, 5.8f, paintingZ[i]),
                    Quaternion.Euler(0f, 90f, 0f),
                    i % 2 == 0 ? portraitRed : portraitBlue,
                    gold);

                CreatePainting(
                    $"Portrait_Right_{i}",
                    portraits,
                    new Vector3(8.72f, 5.8f, paintingZ[i]),
                    Quaternion.Euler(0f, -90f, 0f),
                    i % 2 == 0 ? portraitBlue : portraitRed,
                    gold);
            }

            string sofaPath = $"{WoodenSofaFolder}/painted_wooden_sofa_1k.fbx";
            string tablePath = $"{GothicTableFolder}/gothic_coffee_table_1k.fbx";
            string chairPath = $"{WoodenChairFolder}/WoodenChair_01_1k.fbx";
            string cabinetPath = $"{WoodenCabinetFolder}/painted_wooden_cabinet_1k.fbx";

            PlaceFurnitureModel(
                "PaintedWoodenSofa_Left",
                sofaPath,
                furnitureParent,
                new Vector3(-7.10f, 0.12f, -3.0f),
                Quaternion.Euler(0f, 90f, 0f),
                1.35f,
                woodenSofa);
            PlaceFurnitureModel(
                "PaintedWoodenSofa_Right",
                sofaPath,
                furnitureParent,
                new Vector3(7.10f, 0.12f, 3.0f),
                Quaternion.Euler(0f, -90f, 0f),
                1.35f,
                woodenSofa);

            PlaceFurnitureModel(
                "GothicCoffeeTable_Left",
                tablePath,
                furnitureParent,
                new Vector3(-5.20f, 0.12f, -3.0f),
                Quaternion.Euler(0f, 90f, 0f),
                0.78f,
                gothicTable);
            PlaceFurnitureModel(
                "GothicCoffeeTable_Right",
                tablePath,
                furnitureParent,
                new Vector3(5.20f, 0.12f, 3.0f),
                Quaternion.Euler(0f, -90f, 0f),
                0.78f,
                gothicTable);

            PlaceFurnitureModel(
                "GothicChair_LeftFacing",
                chairPath,
                furnitureParent,
                new Vector3(-3.55f, 0.12f, -3.0f),
                Quaternion.Euler(0f, -90f, 0f),
                1.75f,
                woodenChair);
            PlaceFurnitureModel(
                "GothicChair_LeftCorner",
                chairPath,
                furnitureParent,
                new Vector3(-5.15f, 0.12f, -4.75f),
                Quaternion.Euler(0f, 0f, 0f),
                1.75f,
                woodenChair);
            PlaceFurnitureModel(
                "GothicChair_RightFacing",
                chairPath,
                furnitureParent,
                new Vector3(3.55f, 0.12f, 3.0f),
                Quaternion.Euler(0f, 90f, 0f),
                1.75f,
                woodenChair);
            PlaceFurnitureModel(
                "GothicChair_RightCorner",
                chairPath,
                furnitureParent,
                new Vector3(5.15f, 0.12f, 4.75f),
                Quaternion.Euler(0f, 180f, 0f),
                1.75f,
                woodenChair);

            PlaceFurnitureModel(
                "PaintedWoodenCabinet_Left",
                cabinetPath,
                furnitureParent,
                new Vector3(-8.20f, 0.12f, 8.7f),
                Quaternion.Euler(0f, 90f, 0f),
                2.65f,
                woodenCabinet);
            PlaceFurnitureModel(
                "PaintedWoodenCabinet_Right",
                cabinetPath,
                furnitureParent,
                new Vector3(8.20f, 0.12f, -8.7f),
                Quaternion.Euler(0f, -90f, 0f),
                2.65f,
                woodenCabinet);

            CreatePrimitive("Runner", PrimitiveType.Cube, floorDecor, new Vector3(0f, 0.11f, 1.5f), new Vector3(3.4f, 0.025f, 23f), portraitRed, false);
        }

        private static void BuildLighting(
            Transform parent,
            Material blackIron,
            Material gold,
            Material candleGlow,
            Material lanternMetal,
            Material lanternGlass)
        {
            Transform ambient = NewGroup("Ambient", parent);
            Transform chandeliers = NewGroup("Chandeliers", parent);
            Transform wallSconces = NewGroup("Wall_Sconces", parent);
            GameObject moonlightObject = new GameObject("Moonlight");
            moonlightObject.transform.SetParent(ambient, false);
            moonlightObject.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
            Light moonlight = moonlightObject.AddComponent<Light>();
            moonlight.type = LightType.Directional;
            moonlight.color = new Color(0.27f, 0.38f, 0.62f);
            moonlight.intensity = 0.34f;
            moonlight.shadows = LightShadows.Soft;

            CreateChandelier("Chandelier_North", chandeliers, new Vector3(0f, 11.45f, 8.7f), blackIron, gold, candleGlow);
            CreateChandelier("Chandelier_South", chandeliers, new Vector3(0f, 11.45f, -8.7f), blackIron, gold, candleGlow);

            float[] zPositions = { -9.3f, -3.1f, 3.1f, 9.3f };
            foreach (float z in zPositions)
            {
                CreateSconce(wallSconces, new Vector3(-8.35f, 3.9f, z), blackIron, gold, lanternMetal, lanternGlass);
                CreateSconce(wallSconces, new Vector3(8.35f, 3.9f, z), blackIron, gold, lanternMetal, lanternGlass);
            }
        }

        private static void BuildOrrery(
            Material gold,
            Material blackIron,
            Material orbitGlow,
            Material celestialSun,
            Material celestialPlanet,
            Material celestialMoon,
            Material celestialComet)
        {
            Transform orrery = NewRoot("Celestial_Orrery");
            orrery.position = new Vector3(0f, 9.35f, 0f);

            CreatePrimitive("SuspensionRod", PrimitiveType.Cylinder, orrery, new Vector3(0f, 1.9f, 0f), new Vector3(0.09f, 1.8f, 0.09f), blackIron, false);
            CreatePrimitive("SuspensionHub", PrimitiveType.Sphere, orrery, new Vector3(0f, 0.35f, 0f), Vector3.one * 0.30f, gold, false);

            Transform movingAssembly = new GameObject("MovingOrbitAssembly").transform;
            movingAssembly.SetParent(orrery, false);

            Transform cometOrbit = CreateOrbitTrack(
                "CometOrbit",
                "OrbitRing_Horizontal",
                movingAssembly,
                Quaternion.identity,
                4.45f,
                orbitGlow);
            Transform moonOrbit = CreateOrbitTrack(
                "MoonOrbit",
                "OrbitRing_TiltA",
                movingAssembly,
                Quaternion.Euler(62f, 0f, 18f),
                3.8f,
                gold);
            Transform planetOrbit = CreateOrbitTrack(
                "PlanetOrbit",
                "OrbitRing_TiltB",
                movingAssembly,
                Quaternion.Euler(0f, 0f, 72f),
                3.2f,
                orbitGlow);

            GameObject sun = CreatePrimitive("Sun", PrimitiveType.Sphere, orrery, Vector3.zero, Vector3.one * 0.9f, celestialSun, false);
            Light sunLight = sun.AddComponent<Light>();
            sunLight.type = LightType.Point;
            sunLight.color = new Color(1f, 0.36f, 0.08f);
            sunLight.intensity = 235f;
            sunLight.range = 8f;

            GameObject planet = CreateOrbitingBody("Planet", planetOrbit, 3.2f, 12f, 0.72f, celestialPlanet);
            GameObject moon = CreateOrbitingBody("Moon", moonOrbit, 3.8f, 158f, 0.48f, celestialMoon);
            GameObject comet = CreateOrbitingBody("Comet", cometOrbit, 4.45f, 244f, 0.36f, celestialComet);

            TrailRenderer cometTrail = comet.AddComponent<TrailRenderer>();
            cometTrail.time = 2.8f;
            cometTrail.minVertexDistance = 0.04f;
            cometTrail.startWidth = 0.28f;
            cometTrail.endWidth = 0.015f;
            cometTrail.sharedMaterial = celestialComet;
            cometTrail.startColor = new Color(1f, 0.16f, 0.28f, 0.92f);
            cometTrail.endColor = new Color(0.22f, 0.01f, 0.04f, 0f);

            ManorOrreryController controller = orrery.gameObject.AddComponent<ManorOrreryController>();
            controller.Configure(
                movingAssembly,
                planetOrbit,
                moonOrbit,
                cometOrbit,
                planet.transform,
                moon.transform,
                comet.transform);
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
            Transform silverFangQuest = NewGroup("SilverFangQuest", puzzleParent);
            Transform pedestal = new GameObject("SilverFang_Pedestal").transform;
            pedestal.SetParent(silverFangQuest, false);
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
                silverFangQuest,
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
            display.SetParent(silverFangQuest, false);
            display.localPosition = new Vector3(-5.2f, 0f, -10.5f);
            CreatePrimitive("DisplayTop", PrimitiveType.Cylinder, display, new Vector3(0f, 1.0f, 0f), new Vector3(0.72f, 0.12f, 0.72f), darkWood);
            CreatePrimitive("DisplayStem", PrimitiveType.Cylinder, display, new Vector3(0f, 0.52f, 0f), new Vector3(0.16f, 0.5f, 0.16f), gold);
            CreatePrimitive("DisplayBase", PrimitiveType.Cylinder, display, new Vector3(0f, 0.12f, 0f), new Vector3(0.48f, 0.12f, 0.48f), stone);

            GameObject doorObject = GameObject.Find("ExitDoor");
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

            Camera xrCamera = xrRig.GetComponentInChildren<Camera>(true);
            if (xrCamera != null)
            {
                // Matches a typical headset's broader view in desktop Play Mode.
                // Active XR runtimes replace this projection with the device FOV.
                xrCamera.fieldOfView = 88f;
            }
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

        private static GameObject PlaceFurnitureModel(
            string name,
            string modelPath,
            Transform parent,
            Vector3 position,
            Quaternion rotation,
            float desiredHeight,
            Material material)
        {
            GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (modelAsset == null)
            {
                Debug.LogError($"Furniture model was not found: {modelPath}");
                return null;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(modelAsset) as GameObject;
            if (instance == null)
            {
                Debug.LogError($"Furniture model could not be instantiated: {modelPath}");
                return null;
            }

            instance.name = name;
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = position;
            // Poly Haven's raw FBX meshes are Z-up. Apply the base correction
            // before the room-facing yaw so every rebuild keeps them upright.
            Quaternion zUpToYUp = Quaternion.Euler(-90f, 0f, 0f);
            instance.transform.localRotation = rotation * zUpToYUp;
            instance.transform.localScale = Vector3.one;
            AssignMaterial(instance, material);

            Bounds initialBounds = CalculateWorldBounds(instance);
            if (initialBounds.size.y > 0.001f)
            {
                float scale = desiredHeight / initialBounds.size.y;
                instance.transform.localScale *= scale;
            }

            Bounds placedBounds = CalculateWorldBounds(instance);
            float floorHeight = parent.TransformPoint(position).y;
            instance.transform.position += Vector3.up * (floorHeight - placedBounds.min.y);

            Bounds localBounds = CalculateLocalBounds(instance);
            BoxCollider collider = instance.GetComponent<BoxCollider>();
            if (collider == null)
            {
                collider = instance.AddComponent<BoxCollider>();
            }

            collider.center = localBounds.center;
            collider.size = localBounds.size;
            return instance;
        }

        private static void AssignMaterial(GameObject root, Material material)
        {
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                Material[] materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = material;
                }

                renderer.sharedMaterials = materials;
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.receiveShadows = true;
            }
        }

        private static Bounds CalculateWorldBounds(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return new Bounds(root.transform.position, Vector3.one);
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }

        private static Bounds CalculateLocalBounds(GameObject root)
        {
            Bounds worldBounds = CalculateWorldBounds(root);
            Vector3 min = worldBounds.min;
            Vector3 max = worldBounds.max;
            Bounds localBounds = new Bounds(root.transform.InverseTransformPoint(min), Vector3.zero);

            for (int x = 0; x <= 1; x++)
            {
                for (int y = 0; y <= 1; y++)
                {
                    for (int z = 0; z <= 1; z++)
                    {
                        Vector3 worldPoint = new Vector3(
                            x == 0 ? min.x : max.x,
                            y == 0 ? min.y : max.y,
                            z == 0 ? min.z : max.z);
                        localBounds.Encapsulate(root.transform.InverseTransformPoint(worldPoint));
                    }
                }
            }

            return localBounds;
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

        private static void CreateSconce(
            Transform parent,
            Vector3 position,
            Material blackIron,
            Material gold,
            Material lanternMetal,
            Material lanternGlass)
        {
            Transform root = new GameObject($"Sconce_{position.x}_{position.z}").transform;
            root.SetParent(parent, false);
            root.position = position;
            float inward = position.x < 0f ? 1f : -1f;

            CreatePrimitive("WallPlate", PrimitiveType.Cube, root, Vector3.zero, new Vector3(0.16f, 0.82f, 0.50f), blackIron, false);
            CreatePrimitive("BracketArm", PrimitiveType.Cube, root, new Vector3(inward * 0.32f, 0.08f, 0f), new Vector3(0.58f, 0.11f, 0.11f), blackIron, false);
            CreatePrimitive("BracketCollar", PrimitiveType.Sphere, root, new Vector3(inward * 0.58f, 0.08f, 0f), new Vector3(0.22f, 0.22f, 0.22f), gold, false);

            GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(LanternModelPath);
            GameObject lantern = modelAsset == null ? null : PrefabUtility.InstantiatePrefab(modelAsset) as GameObject;
            if (lantern != null)
            {
                lantern.name = "OrnateLanternModel";
                lantern.transform.SetParent(root, false);
                // Axis conversion is baked by the importer, leaving the lantern's
                // hanging axis correctly aligned with Unity's vertical Y axis.
                lantern.transform.localRotation = Quaternion.identity;
                lantern.transform.localScale = Vector3.one;
                AssignLanternMaterials(lantern, lanternMetal, lanternGlass);

                Bounds initialBounds = CalculateWorldBounds(lantern);
                if (initialBounds.size.y > 0.001f)
                {
                    lantern.transform.localScale *= 1.05f / initialBounds.size.y;
                }

                Bounds scaledBounds = CalculateWorldBounds(lantern);
                Vector3 targetCenter = root.TransformPoint(new Vector3(inward * 0.62f, -0.30f, 0f));
                lantern.transform.position += targetCenter - scaledBounds.center;
            }

            GameObject lightObject = new GameObject("WarmLight");
            lightObject.transform.SetParent(root, false);
            lightObject.transform.localPosition = new Vector3(inward * 0.62f, -0.30f, 0f);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.53f, 0.25f);
            light.intensity = 82f;
            light.range = 4.8f;
            light.shadows = LightShadows.None;
            ManorLightFlicker flicker = lightObject.AddComponent<ManorLightFlicker>();
            SerializedObject flickerSettings = new SerializedObject(flicker);
            flickerSettings.FindProperty("variation").floatValue = 24f;
            flickerSettings.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignLanternMaterials(GameObject root, Material metal, Material glass)
        {
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                Material[] materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    string materialName = materials[i] == null ? string.Empty : materials[i].name.ToLowerInvariant();
                    bool isGlass = materialName.Contains("glass") || renderer.name.ToLowerInvariant().Contains("glass");
                    materials[i] = isGlass ? glass : metal;
                }

                renderer.sharedMaterials = materials;
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.receiveShadows = true;
            }
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
            ring.startWidth = 0.065f;
            ring.endWidth = 0.065f;
            ring.numCornerVertices = 3;
            ring.numCapVertices = 3;
            ring.sharedMaterial = material;
            for (int i = 0; i < ring.positionCount; i++)
            {
                float angle = i * Mathf.PI * 2f / ring.positionCount;
                ring.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
            }
        }

        private static Transform CreateOrbitTrack(
            string trackName,
            string ringName,
            Transform parent,
            Quaternion rotation,
            float radius,
            Material material)
        {
            Transform track = new GameObject(trackName).transform;
            track.SetParent(parent, false);
            track.localRotation = rotation;
            CreateOrbitRing(ringName, track, Quaternion.identity, radius, material);
            return track;
        }

        private static GameObject CreateOrbitingBody(
            string name,
            Transform orbit,
            float radius,
            float startingAngle,
            float diameter,
            Material material)
        {
            float radians = startingAngle * Mathf.Deg2Rad;
            Vector3 position = new Vector3(Mathf.Cos(radians) * radius, 0f, Mathf.Sin(radians) * radius);
            return CreatePrimitive(name, PrimitiveType.Sphere, orbit, position, Vector3.one * diameter, material, false);
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

        private static Material EnsureTexturedMaterial(
            string name,
            Color tint,
            float metallic,
            float smoothness,
            Vector2 tiling,
            string baseMapPath,
            string normalMapPath,
            string occlusionMapPath)
        {
            Material material = EnsureMaterial(name, tint, metallic, smoothness);
            Texture2D baseMap = AssetDatabase.LoadAssetAtPath<Texture2D>(baseMapPath);
            Texture2D normalMap = AssetDatabase.LoadAssetAtPath<Texture2D>(normalMapPath);
            Texture2D occlusionMap = AssetDatabase.LoadAssetAtPath<Texture2D>(occlusionMapPath);

            material.SetTexture("_BaseMap", baseMap);
            material.SetTextureScale("_BaseMap", tiling);
            material.SetTexture("_BumpMap", normalMap);
            material.SetTextureScale("_BumpMap", tiling);
            material.SetFloat("_BumpScale", 0.72f);
            material.SetTexture("_OcclusionMap", occlusionMap);
            material.SetTextureScale("_OcclusionMap", tiling);
            material.SetFloat("_OcclusionStrength", 0.82f);
            material.EnableKeyword("_NORMALMAP");
            material.EnableKeyword("_OCCLUSIONMAP");
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material EnsureFurnitureMaterial(
            string name,
            Color tint,
            float metallic,
            float smoothness,
            string baseMapPath,
            string normalMapPath)
        {
            Material material = EnsureMaterial(name, tint, metallic, smoothness);
            Texture2D baseMap = AssetDatabase.LoadAssetAtPath<Texture2D>(baseMapPath);
            Texture2D normalMap = AssetDatabase.LoadAssetAtPath<Texture2D>(normalMapPath);

            material.SetTexture("_BaseMap", baseMap);
            material.SetTextureScale("_BaseMap", Vector2.one);
            material.SetTexture("_BumpMap", normalMap);
            material.SetTextureScale("_BumpMap", Vector2.one);
            material.SetFloat("_BumpScale", 0.82f);
            material.EnableKeyword("_NORMALMAP");
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material EnsureLanternMetalMaterial()
        {
            return EnsureFurnitureMaterial(
                "GothicLanternMetal",
                new Color(0.72f, 0.58f, 0.40f),
                0.78f,
                0.34f,
                $"{LanternFolder}/textures/lantern_chandelier_01_diff_1k.jpg",
                $"{LanternFolder}/textures/lantern_chandelier_01_nor_gl_1k.jpg");
        }

        private static Material EnsureLanternGlassMaterial()
        {
            Material material = EnsureMaterial(
                "GothicLanternGlass",
                new Color(0.72f, 0.34f, 0.12f, 0.48f),
                0.05f,
                0.72f,
                new Color(0.68f, 0.18f, 0.035f));
            Texture2D baseMap = AssetDatabase.LoadAssetAtPath<Texture2D>($"{LanternFolder}/textures/lantern_chandelier_01_glass_diff_1k.png");
            material.SetTexture("_BaseMap", baseMap);
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)RenderQueue.Transparent;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void ConfigurePbrTextureImports()
        {
            ConfigureTextureImport($"{WornPlasterFolder}/worn_plaster_wall_diff_2k.jpg", TextureImporterType.Default, true, 4);
            ConfigureTextureImport($"{WornPlasterFolder}/worn_plaster_wall_nor_gl_2k.jpg", TextureImporterType.NormalMap, false, 4);
            ConfigureTextureImport($"{WornPlasterFolder}/worn_plaster_wall_ao_2k.jpg", TextureImporterType.Default, false, 2);
            ConfigureTextureImport($"{WornWoodFloorFolder}/wood_floor_worn_diff_2k.jpg", TextureImporterType.Default, true, 8);
            ConfigureTextureImport($"{WornWoodFloorFolder}/wood_floor_worn_nor_gl_2k.jpg", TextureImporterType.NormalMap, false, 8);
            ConfigureTextureImport($"{WornWoodFloorFolder}/wood_floor_worn_ao_2k.jpg", TextureImporterType.Default, false, 4);

            ConfigureTextureImport($"{GothicTableFolder}/textures/gothic_coffee_table_diff_1k.jpg", TextureImporterType.Default, true, 4);
            ConfigureTextureImport($"{GothicTableFolder}/textures/gothic_coffee_table_nor_gl_1k.exr", TextureImporterType.NormalMap, false, 4);
            ConfigureTextureImport($"{GothicTableFolder}/textures/gothic_coffee_table_rough_1k.exr", TextureImporterType.Default, false, 2);

            ConfigureTextureImport($"{WoodenChairFolder}/textures/WoodenChair_01_diff_1k.jpg", TextureImporterType.Default, true, 4);
            ConfigureTextureImport($"{WoodenChairFolder}/textures/WoodenChair_01_nor_gl_1k.exr", TextureImporterType.NormalMap, false, 4);
            ConfigureTextureImport($"{WoodenChairFolder}/textures/WoodenChair_01_roughness_1k.jpg", TextureImporterType.Default, false, 2);
            ConfigureTextureImport($"{WoodenChairFolder}/textures/WoodenChair_01_metallic_1k.exr", TextureImporterType.Default, false, 2);

            ConfigureTextureImport($"{WoodenSofaFolder}/textures/painted_wooden_sofa_diff_1k.jpg", TextureImporterType.Default, true, 4);
            ConfigureTextureImport($"{WoodenSofaFolder}/textures/painted_wooden_sofa_nor_gl_1k.exr", TextureImporterType.NormalMap, false, 4);
            ConfigureTextureImport($"{WoodenSofaFolder}/textures/painted_wooden_sofa_rough_1k.exr", TextureImporterType.Default, false, 2);

            ConfigureTextureImport($"{WoodenCabinetFolder}/textures/painted_wooden_cabinet_diff_1k.jpg", TextureImporterType.Default, true, 4);
            ConfigureTextureImport($"{WoodenCabinetFolder}/textures/painted_wooden_cabinet_nor_gl_1k.exr", TextureImporterType.NormalMap, false, 4);
            ConfigureTextureImport($"{WoodenCabinetFolder}/textures/painted_wooden_cabinet_rough_1k.exr", TextureImporterType.Default, false, 2);
            ConfigureTextureImport($"{WoodenCabinetFolder}/textures/painted_wooden_cabinet_metal_1k.exr", TextureImporterType.Default, false, 2);

            ConfigureLanternImports();

            ConfigureFurnitureModelImport($"{GothicTableFolder}/gothic_coffee_table_1k.fbx");
            ConfigureFurnitureModelImport($"{WoodenChairFolder}/WoodenChair_01_1k.fbx");
            ConfigureFurnitureModelImport($"{WoodenSofaFolder}/painted_wooden_sofa_1k.fbx");
            ConfigureFurnitureModelImport($"{WoodenCabinetFolder}/painted_wooden_cabinet_1k.fbx");
        }

        private static void ConfigureLanternImports()
        {
            ConfigureTextureImport($"{LanternFolder}/textures/lantern_chandelier_01_diff_1k.jpg", TextureImporterType.Default, true, 4);
            ConfigureTextureImport($"{LanternFolder}/textures/lantern_chandelier_01_nor_gl_1k.jpg", TextureImporterType.NormalMap, false, 4);
            ConfigureTextureImport($"{LanternFolder}/textures/lantern_chandelier_01_rough_1k.jpg", TextureImporterType.Default, false, 2);
            ConfigureTextureImport($"{LanternFolder}/textures/lantern_chandelier_01_glass_diff_1k.png", TextureImporterType.Default, true, 4);
            ConfigureTextureImport($"{LanternFolder}/textures/lantern_chandelier_01_glass_alpha_1k.png", TextureImporterType.Default, false, 2);
            ConfigureFurnitureModelImport(LanternModelPath);
        }

        private static void ConfigureFurnitureModelImport(string path)
        {
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
            {
                Debug.LogWarning($"Model importer was not found for {path}");
                return;
            }

            if (!importer.bakeAxisConversion)
            {
                // Poly Haven FBX files are Z-up. Baking the axis conversion keeps
                // their furniture upright when the scene builder applies yaw.
                importer.bakeAxisConversion = true;
                importer.SaveAndReimport();
            }
        }

        private static void ConfigureTextureImport(
            string path,
            TextureImporterType type,
            bool sRgb,
            int anisoLevel)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                Debug.LogWarning($"Texture importer was not found for {path}");
                return;
            }

            bool changed = false;
            if (importer.textureType != type)
            {
                importer.textureType = type;
                changed = true;
            }

            if (importer.sRGBTexture != sRgb)
            {
                importer.sRGBTexture = sRgb;
                changed = true;
            }

            if (importer.maxTextureSize != 2048)
            {
                importer.maxTextureSize = 2048;
                changed = true;
            }

            if (!importer.mipmapEnabled)
            {
                importer.mipmapEnabled = true;
                changed = true;
            }

            if (importer.wrapMode != TextureWrapMode.Repeat)
            {
                importer.wrapMode = TextureWrapMode.Repeat;
                changed = true;
            }

            if (importer.anisoLevel != anisoLevel)
            {
                importer.anisoLevel = anisoLevel;
                changed = true;
            }

            if (importer.textureCompression != TextureImporterCompression.Compressed)
            {
                importer.textureCompression = TextureImporterCompression.Compressed;
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }
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

        private static void PreserveSourceObjectsFromSystems(Scene scene)
        {
            GameObject systems = FindRoot(scene, "Systems");
            if (systems == null)
            {
                return;
            }

            foreach (string objectName in new[] { "Global Volume", "EventSystem" })
            {
                Transform child = systems.transform.Find(objectName);
                if (child != null)
                {
                    child.SetParent(null, true);
                }
            }
        }

        private static void MoveSourceObjectUnder(Scene scene, string objectName, Transform parent)
        {
            GameObject sourceObject = FindRoot(scene, objectName);
            if (sourceObject != null)
            {
                sourceObject.transform.SetParent(parent, true);
            }
        }

        private static Transform NewRoot(string name)
        {
            return new GameObject(name).transform;
        }

        private static Transform NewGroup(string name, Transform parent)
        {
            Transform group = new GameObject(name).transform;
            group.SetParent(parent, false);
            return group;
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
