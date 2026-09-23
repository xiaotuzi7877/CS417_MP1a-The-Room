using System;
using System.Linq;
using MichaelManor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MichaelManorEditor
{
    public static class ManorFiveChamberFoundationInstaller
    {
        private const string ScenePath = "Assets/Scenes/MichaelManorHall.unity";

        private static readonly string[] ChamberNames =
        {
            "Chamber_01_Cellar",
            "Chamber_02_BoneCloset",
            "Chamber_03_CoffinVault",
            "Chamber_04_Portrait",
            "Chamber_05_MoonCrypt"
        };

        [MenuItem("Tools/Michael Manor/Install Five Chamber Foundation")]
        public static void InstallFoundation()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Transform puzzleRoot = FindOrCreateSceneRoot(scene, "Puzzle");
            Transform questRoot = FindOrCreateChild(puzzleRoot, "FiveChamberQuest");
            Transform gatesRoot = FindOrCreateChild(questRoot, "Gates");
            Transform stateRoot = FindOrCreateChild(questRoot, "ChamberState");
            FindOrCreateChild(questRoot, "SharedFeedback");
            Transform locationsRoot = FindOrCreateSceneRoot(scene, "GatedLocations");

            FiveChamberQuestController controller =
                GetOrAddComponent<FiveChamberQuestController>(questRoot.gameObject);
            ManorGatePortal[] gates = new ManorGatePortal[FiveChamberQuestController.RequiredChamberCount];

            for (int i = 0; i < ChamberNames.Length; i++)
            {
                Transform gateObject = FindOrCreateChild(gatesRoot, $"Gate_{i + 1:00}_{GetShortName(i)}");
                ManorGatePortal gate = GetOrAddComponent<ManorGatePortal>(gateObject.gameObject);
                gate.Configure(i, i == ChamberNames.Length - 1, null, null, Array.Empty<Renderer>());
                gates[i] = gate;

                Transform chamberState = FindOrCreateChild(stateRoot, $"State_{i + 1:00}_{GetShortName(i)}");
                ManorChamberInteraction interaction =
                    GetOrAddComponent<ManorChamberInteraction>(chamberState.gameObject);
                interaction.Configure(controller, i);

                FindOrCreateChild(locationsRoot, ChamberNames[i]);
            }

            controller.Configure(gates);
            controller.ResetQuest();

            EditorUtility.SetDirty(controller);
            foreach (ManorGatePortal gate in gates)
            {
                EditorUtility.SetDirty(gate);
            }

            foreach (ManorChamberInteraction interaction in
                     stateRoot.GetComponentsInChildren<ManorChamberInteraction>(true))
            {
                EditorUtility.SetDirty(interaction);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Installed the reusable five-chamber quest foundation without changing room visuals or travel.");
        }

        [MenuItem("Tools/Michael Manor/Test Five Chamber Foundation In Play Mode")]
        public static void TestFoundationInPlayMode()
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("Enter Play Mode before testing the five-chamber foundation.");
                return;
            }

            FiveChamberQuestController controller =
                UnityEngine.Object.FindFirstObjectByType<FiveChamberQuestController>(FindObjectsInactive.Include);
            ManorChamberInteraction[] interactions =
                UnityEngine.Object.FindObjectsByType<ManorChamberInteraction>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);
            ManorGatePortal[] gates =
                UnityEngine.Object.FindObjectsByType<ManorGatePortal>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);

            if (controller == null || interactions.Length != FiveChamberQuestController.RequiredChamberCount ||
                gates.Length != FiveChamberQuestController.RequiredChamberCount)
            {
                Debug.LogError(
                    $"Five-chamber foundation is incomplete: controller={controller != null}, " +
                    $"interactions={interactions.Length}, gates={gates.Length}.");
                return;
            }

            Array.Sort(interactions, (left, right) => left.ChamberIndex.CompareTo(right.ChamberIndex));
            Array.Sort(gates, (left, right) => left.ChamberIndex.CompareTo(right.ChamberIndex));

            controller.ResetQuest();
            foreach (ManorChamberInteraction interaction in interactions)
            {
                interaction.ResetInteraction();
            }

            bool startsLocked = !controller.GatesUnlocked && controller.ExploredCount == 0 &&
                                gates.All(gate => !gate.IsUnlocked);
            bool lockedGateRejected = !gates[0].TryRequestActivation();
            bool firstCompletionAccepted = interactions[0].TryCompleteInteraction();
            bool duplicateCompletionRejected = !interactions[0].TryCompleteInteraction();
            bool countedOnce = controller.ExploredCount == 1 && controller.IsChamberComplete(0);

            controller.UnlockGates();
            bool gatesUnlocked = controller.GatesUnlocked && gates.All(gate => gate.IsUnlocked);
            bool activationAccepted = gates[0].TryRequestActivation();

            foreach (ManorChamberInteraction interaction in interactions)
            {
                interaction.ResetInteraction();
            }
            controller.ResetQuest();

            if (!startsLocked || !lockedGateRejected || !firstCompletionAccepted ||
                !duplicateCompletionRejected || !countedOnce || !gatesUnlocked || !activationAccepted ||
                controller.ExploredCount != 0 || controller.GatesUnlocked)
            {
                Debug.LogError(
                    "Five-chamber foundation test failed. " +
                    $"startsLocked={startsLocked}, lockedRejected={lockedGateRejected}, " +
                    $"firstAccepted={firstCompletionAccepted}, duplicateRejected={duplicateCompletionRejected}, " +
                    $"countedOnce={countedOnce}, gatesUnlocked={gatesUnlocked}, " +
                    $"activationAccepted={activationAccepted}.");
                return;
            }

            Debug.Log(
                "Five-chamber foundation test passed: locked 0/5 start, one-shot interaction, " +
                "shared Gate unlock, explicit activation request, and clean reset.");
        }

        private static string GetShortName(int index)
        {
            return ChamberNames[index].Replace($"Chamber_{index + 1:00}_", string.Empty);
        }

        private static Transform FindOrCreateSceneRoot(Scene scene, string name)
        {
            GameObject existing = scene.GetRootGameObjects().FirstOrDefault(root => root.name == name);
            if (existing != null)
            {
                return existing.transform;
            }

            GameObject created = new GameObject(name);
            SceneManager.MoveGameObjectToScene(created, scene);
            return created.transform;
        }

        private static Transform FindOrCreateChild(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
            {
                return existing;
            }

            GameObject created = new GameObject(name);
            created.transform.SetParent(parent, false);
            return created.transform;
        }

        private static T GetOrAddComponent<T>(GameObject target) where T : Component
        {
            T component = target.GetComponent<T>();
            return component != null ? component : target.AddComponent<T>();
        }
    }
}
