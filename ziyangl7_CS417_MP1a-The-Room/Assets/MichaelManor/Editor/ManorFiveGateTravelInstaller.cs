using System;
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
    public static class ManorFiveGateTravelInstaller
    {
        private const string ScenePath = "Assets/Scenes/MichaelManorHall.unity";
        private const float WallInnerSurfaceX = 8.57f;
        private const float WallPanelThickness = 0.14f;
        private const float FloorSurfaceY = 0.07f;

        private static readonly string[] ShortNames =
        {
            "Cellar", "BoneCloset", "CoffinVault", "Portrait", "MoonCrypt"
        };

        private static readonly Vector3[] GatePositions =
        {
            new Vector3(-4.8f, FloorSurfaceY + 0.06f, -6.0f),
            new Vector3(-WallInnerSurfaceX + WallPanelThickness * 0.5f, 1.55f, -1.0f),
            new Vector3(WallInnerSurfaceX - WallPanelThickness * 0.5f, 1.55f, 2.0f),
            new Vector3(-WallInnerSurfaceX + WallPanelThickness * 0.5f, 1.85f, 5.5f),
            new Vector3(WallInnerSurfaceX - WallPanelThickness * 0.5f, 2.10f, -5.5f)
        };

        private static readonly Vector3[] ChamberPositions =
        {
            new Vector3(80f, 0f, 0f),
            new Vector3(110f, 0f, 0f),
            new Vector3(140f, 0f, 0f),
            new Vector3(170f, 0f, 0f),
            new Vector3(200f, 0f, 0f)
        };

        [MenuItem("Tools/Michael Manor/Install Five Gate Travel")]
        public static void Install()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Transform questRoot = FindSceneObject(scene, "FiveChamberQuest").transform;
            Transform gatesRoot = questRoot.Find("Gates");
            Transform locationsRoot = FindSceneObject(scene, "GatedLocations").transform;
            FiveChamberQuestController controller = questRoot.GetComponent<FiveChamberQuestController>();
            Transform xrRig = FindSceneObject(scene, "XR Origin (XR Rig)").transform;
            Camera headCamera = xrRig.GetComponentInChildren<Camera>(true);

            Transform travelRoot = FindOrCreateChild(questRoot, "TravelSystem");
            ManorGateTravelSystem travelSystem = GetOrAddComponent<ManorGateTravelSystem>(travelRoot.gameObject);
            Transform hallReturn = FindOrCreateChild(travelRoot, "HallReturnAnchor");
            hallReturn.position = new Vector3(0f, 1.70f, -12.0f);
            hallReturn.rotation = Quaternion.Euler(0f, 0f, 0f);

            Material stone = LoadMaterial("ManorStone");
            Material darkWood = LoadMaterial("DarkWood");
            Material purple = LoadMaterial("SpectralGlow");
            Material moon = LoadMaterial("CelestialMoon");
            ManorGatePortal[] gates = new ManorGatePortal[FiveChamberQuestController.RequiredChamberCount];

            for (int i = 0; i < ShortNames.Length; i++)
            {
                Transform gateTransform = gatesRoot.Find($"Gate_{i + 1:00}_{ShortNames[i]}");
                ManorGatePortal gate = gateTransform.GetComponent<ManorGatePortal>();
                gates[i] = gate;
                DestroyChildIfPresent(gateTransform, "Visual");
                Transform visual = FindOrCreateChild(gateTransform, "Visual");
                visual.position = GatePositions[i];

                bool floorGate = i == 0;
                visual.rotation = Quaternion.identity;

                CreateGateVisual(i, visual, darkWood, i == 4 ? moon : purple, floorGate);
                Renderer[] runeRenderers = visual.GetComponentsInChildren<Renderer>(true)
                    .Where(renderer => renderer.gameObject.name.Contains("Rune")).ToArray();

                XRSimpleInteractable interactable = GetOrAddComponent<XRSimpleInteractable>(visual.gameObject);
                ManorGateInteractor gateInteractor = GetOrAddComponent<ManorGateInteractor>(visual.gameObject);
                gateInteractor.Configure(gate);
                EditorUtility.SetDirty(interactable);
                EditorUtility.SetDirty(gateInteractor);

                Transform chamber = locationsRoot.Find($"Chamber_{i + 1:00}_{ShortNames[i]}");
                chamber.position = ChamberPositions[i];
                DestroyChildIfPresent(chamber, "TravelShell");
                Transform shell = FindOrCreateChild(chamber, "TravelShell");
                BuildRoomShell(shell, stone, darkWood);

                Transform spawn = FindOrCreateChild(chamber, "SpawnPoint");
                spawn.localPosition = new Vector3(0f, 1.70f, -2.2f);
                spawn.localRotation = Quaternion.identity;

                Transform returnRune = CreatePart("ReturnRune", PrimitiveType.Cylinder, shell,
                    new Vector3(0f, 0.08f, 3.0f), new Vector3(0.58f, 0.08f, 0.58f), moon);
                GetOrAddComponent<XRSimpleInteractable>(returnRune.gameObject);
                ManorReturnRune returnComponent = GetOrAddComponent<ManorReturnRune>(returnRune.gameObject);
                returnComponent.Configure(travelSystem, hallReturn, i);
                CreateWorldText("ReturnLabel", returnRune, new Vector3(0f, 0.15f, 0f),
                    Quaternion.Euler(90f, 0f, 0f), "RETURN", 0.32f, Color.white);

                gate.SetTravelAnchors(spawn, hallReturn);
                gate.Configure(i, i == 4, spawn, hallReturn, runeRenderers);
                EditorUtility.SetDirty(gate);
                EditorUtility.SetDirty(returnComponent);
            }

            controller.Configure(gates);
            controller.ResetQuest();
            travelSystem.Configure(xrRig, headCamera.transform, gates);
            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(travelSystem);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Installed five explicit hall Gates, isolated shells, Spawn Points, Return Runes, and XR-safe same-scene travel.");
        }

        [MenuItem("Tools/Michael Manor/Align Five Gate Panels To Surfaces")]
        public static void AlignFiveGatePanelsToSurfaces()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ManorGatePortal[] gates = UnityEngine.Object.FindObjectsByType<ManorGatePortal>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None)
                .OrderBy(gate => gate.ChamberIndex).ToArray();
            if (gates.Length != FiveChamberQuestController.RequiredChamberCount)
            {
                Debug.LogError($"Gate alignment stopped: expected 5 Gates, found {gates.Length}.");
                return;
            }

            for (int i = 0; i < gates.Length; i++)
            {
                Transform visual = gates[i].transform.Find("Visual");
                Transform panel = visual != null ? visual.Find(PanelName(i)) : null;
                Transform rune = visual != null ? visual.Find("GateRune") : null;
                if (visual == null || panel == null || rune == null)
                {
                    Debug.LogError($"Gate alignment stopped: Gate {i + 1} is missing Visual, panel, or GateRune.");
                    return;
                }

                visual.SetPositionAndRotation(GatePositions[i], Quaternion.identity);
                if (i == 0)
                {
                    panel.localPosition = Vector3.zero;
                    panel.localScale = new Vector3(1.8f, 0.12f, 1.15f);
                    rune.localPosition = new Vector3(0f, 0.10f, 0f);
                    rune.localRotation = Quaternion.identity;
                }
                else
                {
                    panel.localPosition = Vector3.zero;
                    panel.localScale = new Vector3(WallPanelThickness, 1.65f, 1.25f);
                    rune.localPosition = new Vector3(GatePositions[i].x < 0f ? 0.13f : -0.13f, 0f, 0f);
                    rune.localRotation = Quaternion.Euler(0f, 0f, 90f);
                }

                EditorUtility.SetDirty(visual);
                EditorUtility.SetDirty(panel);
                EditorUtility.SetDirty(rune);
            }

            ManorTextOrientationFixer.Apply();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("VR PLAYTEST SECTION 5 FIXED: all five Gate panels are flush to their wall or floor surface.");
        }

        [MenuItem("Tools/Michael Manor/Validate Five Gate Panel Alignment")]
        public static void ValidateFiveGatePanelAlignment()
        {
            ManorGatePortal[] gates = UnityEngine.Object.FindObjectsByType<ManorGatePortal>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None)
                .OrderBy(gate => gate.ChamberIndex).ToArray();
            bool valid = gates.Length == FiveChamberQuestController.RequiredChamberCount;
            for (int i = 0; valid && i < gates.Length; i++)
            {
                Transform visual = gates[i].transform.Find("Visual");
                Transform panel = visual != null ? visual.Find(PanelName(i)) : null;
                Transform rune = visual != null ? visual.Find("GateRune") : null;
                Renderer renderer = panel != null ? panel.GetComponent<Renderer>() : null;
                valid &= visual != null && panel != null && rune != null && renderer != null &&
                         panel.GetComponent<Collider>() != null && visual.GetComponent<XRSimpleInteractable>() != null;
                if (!valid) break;

                Bounds bounds = renderer.bounds;
                if (i == 0)
                {
                    valid &= Mathf.Abs(bounds.min.y - FloorSurfaceY) < 0.015f && bounds.size.y < 0.15f;
                }
                else
                {
                    bool leftWall = GatePositions[i].x < 0f;
                    float wallContactFace = leftWall ? bounds.min.x : bounds.max.x;
                    bool runeFacesHall = leftWall ? rune.position.x > bounds.center.x : rune.position.x < bounds.center.x;
                    valid &= Mathf.Abs(Mathf.Abs(wallContactFace) - WallInnerSurfaceX) < 0.015f &&
                             bounds.size.x < 0.18f && bounds.size.z > 1.20f && runeFacesHall;
                }
            }

            if (valid) Debug.Log("FIVE GATE PANEL ALIGNMENT PASS: floor Gate is seated and four wall panels are flush, correctly oriented, and interactive.");
            else Debug.LogError("Five Gate panel alignment validation failed.");
        }

        [MenuItem("Tools/Michael Manor/Test Five Gate Travel In Play Mode")]
        public static void TestInPlayMode()
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("Enter Play Mode before testing five-Gate travel.");
                return;
            }

            FiveChamberQuestController controller = UnityEngine.Object.FindFirstObjectByType<FiveChamberQuestController>();
            ManorGateTravelSystem travel = UnityEngine.Object.FindFirstObjectByType<ManorGateTravelSystem>();
            ManorGatePortal[] gates = UnityEngine.Object.FindObjectsByType<ManorGatePortal>(FindObjectsSortMode.None)
                .OrderBy(gate => gate.ChamberIndex).ToArray();
            // The Moon Crypt rune stays hidden until the Celestial Lock (Section 6), so include inactive runes.
            ManorReturnRune[] returns = UnityEngine.Object.FindObjectsByType<ManorReturnRune>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None)
                .OrderBy(rune => rune.ChamberIndex).ToArray();

            if (controller == null || travel == null || gates.Length != 5 || returns.Length != 5)
            {
                Debug.LogError($"Five-Gate travel setup incomplete: gates={gates.Length}, returns={returns.Length}.");
                return;
            }

            Vector3 originalPosition = travel.XrRig.position;
            Quaternion originalRotation = travel.XrRig.rotation;
            controller.ResetQuest();
            bool lockedRejected = !gates[0].TryRequestActivation();
            controller.UnlockGates();
            bool allPassed = lockedRejected;

            for (int i = 0; i < gates.Length; i++)
            {
                bool entered = gates[i].TryRequestActivation();
                float arrivalDistance = Vector3.Distance(travel.TrackedHead.position, gates[i].Destination.position);
                float arrivalYaw = Mathf.Abs(Mathf.DeltaAngle(
                    travel.TrackedHead.eulerAngles.y, gates[i].Destination.eulerAngles.y));
                bool returned = returns[i].TryReturn();
                float returnDistance = Vector3.Distance(travel.TrackedHead.position, gates[i].ReturnAnchor.position);
                float returnYaw = Mathf.Abs(Mathf.DeltaAngle(
                    travel.TrackedHead.eulerAngles.y, gates[i].ReturnAnchor.eulerAngles.y));
                allPassed &= entered && returned && arrivalDistance < 0.03f && returnDistance < 0.03f &&
                             arrivalYaw < 1f && returnYaw < 1f;
            }

            travel.XrRig.SetPositionAndRotation(originalPosition, originalRotation);
            controller.ResetQuest();
            if (!allPassed)
            {
                Debug.LogError("Five-Gate travel test failed explicit-lock, arrival, return, or repeated alignment checks.");
                return;
            }

            Debug.Log("Five-Gate travel test passed: locked rejection, 5 unique arrivals, 5 explicit returns, and stable head alignment.");
        }

        private static void CreateGateVisual(int index, Transform parent, Material frame, Material rune, bool floorGate)
        {
            if (floorGate)
            {
                CreatePart("LooseFloorboard", PrimitiveType.Cube, parent, Vector3.zero,
                    new Vector3(1.8f, 0.12f, 1.15f), frame);
                CreatePart("GateRune", PrimitiveType.Cylinder, parent, new Vector3(0f, 0.10f, 0f),
                    new Vector3(0.42f, 0.04f, 0.42f), rune);
                return;
            }

            CreatePart(index == 1 ? "CabinetPortal" : index == 2 ? "FuneraryPortal" :
                index == 3 ? "PortraitPortal" : "MoonPortal", PrimitiveType.Cube, parent,
                Vector3.zero, new Vector3(WallPanelThickness, 1.65f, 1.25f), frame);
            Transform disc = CreatePart("GateRune", PrimitiveType.Cylinder, parent,
                new Vector3(parent.position.x < 0f ? 0.13f : -0.13f, 0f, 0f),
                new Vector3(0.38f, 0.055f, 0.38f), rune);
            disc.localRotation = Quaternion.Euler(0f, 0f, 90f);
        }

        private static void BuildRoomShell(Transform root, Material stone, Material trim)
        {
            CreatePart("Floor", PrimitiveType.Cube, root, new Vector3(0f, -0.10f, 0f), new Vector3(8f, 0.20f, 10f), stone);
            CreatePart("Wall_Back", PrimitiveType.Cube, root, new Vector3(0f, 2.5f, 5f), new Vector3(8f, 5f, 0.20f), stone);
            CreatePart("Wall_Front", PrimitiveType.Cube, root, new Vector3(0f, 2.5f, -5f), new Vector3(8f, 5f, 0.20f), stone);
            CreatePart("Wall_Left", PrimitiveType.Cube, root, new Vector3(-4f, 2.5f, 0f), new Vector3(0.20f, 5f, 10f), stone);
            CreatePart("Wall_Right", PrimitiveType.Cube, root, new Vector3(4f, 2.5f, 0f), new Vector3(0.20f, 5f, 10f), stone);
            CreatePart("Ceiling", PrimitiveType.Cube, root, new Vector3(0f, 5f, 0f), new Vector3(8f, 0.20f, 10f), trim);
        }

        private static Transform CreatePart(string name, PrimitiveType type, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            if (material != null) part.GetComponent<Renderer>().sharedMaterial = material;
            return part.transform;
        }

        private static void CreateWorldText(string name, Transform parent, Vector3 position, Quaternion rotation, string value, float size, Color color)
        {
            GameObject target = new GameObject(name);
            target.transform.SetParent(parent, false);
            target.transform.localPosition = position;
            target.transform.localRotation = rotation;
            TextMeshPro text = target.AddComponent<TextMeshPro>();
            text.text = value;
            text.fontSize = size;
            text.alignment = TextAlignmentOptions.Center;
            text.color = color;
            text.rectTransform.sizeDelta = new Vector2(1.2f, 0.35f);
        }

        private static Material LoadMaterial(string name) => AssetDatabase.LoadAssetAtPath<Material>($"Assets/MichaelManor/Materials/{name}.mat");

        private static string PanelName(int index) => index == 0 ? "LooseFloorboard" :
            index == 1 ? "CabinetPortal" : index == 2 ? "FuneraryPortal" :
            index == 3 ? "PortraitPortal" : "MoonPortal";

        private static GameObject FindSceneObject(Scene scene, string name)
        {
            return scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .First(transform => transform.name == name).gameObject;
        }

        private static Transform FindOrCreateChild(Transform parent, string name)
        {
            Transform found = parent.Find(name);
            if (found != null) return found;
            GameObject created = new GameObject(name);
            created.transform.SetParent(parent, false);
            return created.transform;
        }

        private static void DestroyChildIfPresent(Transform parent, string name)
        {
            Transform found = parent.Find(name);
            if (found != null) UnityEngine.Object.DestroyImmediate(found.gameObject);
        }

        private static T GetOrAddComponent<T>(GameObject target) where T : Component
        {
            T found = target.GetComponent<T>();
            return found != null ? found : target.AddComponent<T>();
        }
    }
}
