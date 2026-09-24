using System.Linq;
using System.Threading.Tasks;
using MichaelManor;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace MichaelManorEditor
{
    /// Connected Scenes acceptance: locked before the win, then an item in each hand survives the
    /// walk through the portal and arrives in the matching hand of the next scene.
    public static class ManorScenePortalTest
    {
        [MenuItem("Tools/Michael Manor/Test Minh Room Portal (Play Mode)")]
        public static async void Run()
        {
            if (!EditorApplication.isPlaying) { Debug.LogError("Enter Play Mode (MichaelManorHall) before testing the portal."); return; }
            string failure = null;
            try { failure = await Execute(); }
            catch (System.Exception e) { failure = "exception: " + e.Message; }
            if (failure == null) Debug.Log("MINH PORTAL TEST PASS: locked before win, opened after win, left and right held items instantiated into matching hands in the next scene.");
            else Debug.LogError("MINH PORTAL TEST FAIL: " + failure);
        }

        private static async Task<string> Execute()
        {
            var ritual = Object.FindFirstObjectByType<ManorThreeStagePuzzle>();
            var portal = Object.FindFirstObjectByType<ManorScenePortal>();
            if (ritual == null || portal == null) return "ritual or portal missing (run Install Minh Room Portal)";
            string startScene = SceneManager.GetActiveScene().path;

            ManorPlaythroughRecorder.Begin("minh_portal", 8f);
            ritual.ResetPuzzle();
            await Wait(1f);
            ManorPlaythroughRecorder.Look(new Vector3(1.8f, float.NaN, 9f), new Vector3(0f, 2.3f, 15.5f));   // beside the exit pedestal
            await Wait(1.5f);                                               // sealed door, dark portal
            if (portal.IsOpen || portal.TryEnter()) return "portal usable before the ritual was complete";

            ritual.SolveAllForPresentation();
            float end = Time.realtimeSinceStartup + 30f;
            while (!portal.IsOpen && Time.realtimeSinceStartup < end) await Wait(0.2f);
            if (!portal.IsOpen) return "portal did not open after the win";
            await Wait(2.5f);                                               // door open, portal glowing

            // Without a headset XRI leaves both controllers disabled; simulate tracked controllers
            // here and again in the next scene as soon as it loads.
            ActivateControllers();
            SceneManager.sceneLoaded += ActivateControllersOnLoad;
            await Wait(0.3f);
            XRBaseInputInteractor left = Hand(InteractorHandedness.Left), right = Hand(InteractorHandedness.Right);
            if (left == null || right == null) return "hand interactors not found";
            var leftItem = GameObject.Find("GrabbableHandmirror")?.GetComponent<XRGrabInteractable>();
            var rightItem = GameObject.Find("RustyKey_RedHerring")?.GetComponent<XRGrabInteractable>();
            if (leftItem == null || rightItem == null) return "test items missing";
            string leftName = leftItem.GetComponent<ManorCarryableItem>().CarryPrefab.name;
            string rightName = rightItem.GetComponent<ManorCarryableItem>().CarryPrefab.name;
            left.interactionManager.SelectEnter((IXRSelectInteractor)left, (IXRSelectInteractable)leftItem);
            right.interactionManager.SelectEnter((IXRSelectInteractor)right, (IXRSelectInteractable)rightItem);
            await Wait(0.3f);
            if (!left.IsSelecting(leftItem) || !right.IsSelecting(rightItem)) return "could not place test items in hands";

            // Walk the head up to, then into, the doorway.
            for (float z = 11.4f; z < 14.4f; z += 0.25f)
            {
                ManorPlaythroughRecorder.Look(new Vector3(Mathf.Lerp(1.2f, 0f, (z - 11.4f) / 3f), float.NaN, z), new Vector3(0f, 2.3f, 17f));
                await Wait(0.12f);
            }
            ManorPlaythroughRecorder.Look(new Vector3(0f, float.NaN, 15.3f), new Vector3(0f, 2f, 17f));
            end = Time.realtimeSinceStartup + 15f;
            while (SceneManager.GetActiveScene().path == startScene && Time.realtimeSinceStartup < end) await Wait(0.2f);
            if (SceneManager.GetActiveScene().path != portal_NextScene) return "walking into the portal did not load " + portal_NextScene;
            await Wait(1.5f);

            GameObject leftCopy = GameObject.Find(leftName), rightCopy = GameObject.Find(rightName);
            if (leftCopy == null || rightCopy == null) return "carried items were not instantiated in the new scene";
            if (leftCopy.scene != SceneManager.GetActiveScene()) return "carried item is not part of the new scene";
            var newLeft = Hand(InteractorHandedness.Left);
            var newRight = Hand(InteractorHandedness.Right);
            bool leftHeld = newLeft != null && newLeft.IsSelecting(leftCopy.GetComponent<XRGrabInteractable>());
            bool rightHeld = newRight != null && newRight.IsSelecting(rightCopy.GetComponent<XRGrabInteractable>());
            float leftDistance = newLeft != null ? Vector3.Distance(leftCopy.transform.position, newLeft.transform.position) : 99f;
            Debug.Log($"Portal arrival: leftHeld={leftHeld} rightHeld={rightHeld} leftDistance={leftDistance:F2}");
            await Wait(2f);                                                 // arrival view with items in hand
            Debug.Log($"RECORDING DONE minh_portal frames={ManorPlaythroughRecorder.End()}");
            if (!leftHeld || !rightHeld) return "carried items were not placed in the matching hands";
            return null;
        }

        private static void ActivateControllersOnLoad(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= ActivateControllersOnLoad;
            ActivateControllers();
        }

        private static void ActivateControllers()
        {
            foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if ((t.name == "Left Controller" || t.name == "Right Controller") && t.gameObject.scene.isLoaded)
                    t.gameObject.SetActive(true);
        }

        private static string portal_NextScene = "Assets/Scenes/SampleScene.unity";

        private static XRBaseInputInteractor Hand(InteractorHandedness handedness) =>
            Object.FindObjectsByType<XRBaseInputInteractor>(FindObjectsSortMode.None)
                .Where(i => i.handedness == handedness && i.isActiveAndEnabled)
                .OrderByDescending(i => i is NearFarInteractor).FirstOrDefault();

        private static Task Wait(float seconds) => Task.Delay((int)(seconds * 1000));
    }
}
