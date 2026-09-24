using System.IO;
using System.Linq;
using MichaelManor;
using UnityEditor;
using UnityEngine;

namespace MichaelManorEditor
{
    /// Play Mode QA helper: captures the Game view as a PNG frame sequence under tmp/Recordings.
    public static class ManorPlaythroughRecorder
    {
        private static string folder;
        private static int frame;
        private static double nextCapture;
        private static double interval;

        public static bool IsRecording => folder != null;
        public static string Folder => folder;

        public static string Begin(string name, float framesPerSecond = 8f)
        {
            if (!EditorApplication.isPlaying) { Debug.LogWarning("Enter Play Mode before recording."); return null; }
            End();
            folder = Path.Combine("tmp", "Recordings", name);
            if (Directory.Exists(folder)) Directory.Delete(folder, true);
            Directory.CreateDirectory(folder);
            frame = 0;
            interval = 1.0 / Mathf.Max(1f, framesPerSecond);
            nextCapture = 0;
            EditorApplication.update += Capture;
            EditorApplication.playModeStateChanged += StopOnExit;
            return folder;
        }

        public static int End()
        {
            EditorApplication.update -= Capture;
            EditorApplication.playModeStateChanged -= StopOnExit;
            folder = null;
            return frame;
        }

        /// Places the tracked head at eye (keeping its height when eye.y is NaN) and yaws it toward target.
        public static bool Look(Vector3 eye, Vector3 target)
        {
            ManorGateTravelSystem travel = Object.FindFirstObjectByType<ManorGateTravelSystem>();
            if (travel == null || travel.XrRig == null || travel.TrackedHead == null) return false;
            Transform rig = travel.XrRig, head = travel.TrackedHead;
            Vector3 flat = target - eye;
            flat.y = 0f;
            if (flat.sqrMagnitude > 0.0001f)
            {
                float yaw = Quaternion.LookRotation(flat).eulerAngles.y;
                rig.RotateAround(head.position, Vector3.up, Mathf.DeltaAngle(head.eulerAngles.y, yaw));
            }
            if (float.IsNaN(eye.y)) eye.y = head.position.y;
            rig.position += eye - head.position;
            return true;
        }

        [MenuItem("Tools/Michael Manor/Record Moon Crypt Playthrough (Play Mode)")]
        public static async void RecordMoonCrypt()
        {
            if (Begin("section06") == null) return;
            try
            {
                var ritual = Object.FindFirstObjectByType<ManorThreeStagePuzzle>();
                var buttons = Object.FindObjectsByType<ManorMoonSequenceButton>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                    .OrderBy(b => b.SequenceIndex).ToArray();
                var moonGate = Object.FindObjectsByType<ManorGatePortal>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                    .First(g => g.IsMoonCryptGate);
                var rune = GameObject.Find("Chamber_05_MoonCrypt").transform.Find("TravelShell/ReturnRune").GetComponent<ManorReturnRune>();
                float n = float.NaN;

                ritual.ResetPuzzle(); await Wait(0.5f);
                Look(new Vector3(0f, n, -12.5f), new Vector3(1.55f, 2.8f, -15.2f)); await Wait(2.5f);
                Look(new Vector3(-6.2f, n, -9.3f), new Vector3(-8.05f, 1.8f, -9.3f)); await Wait(1f);
                ritual.SolveCurrentStageForPresentation(); await Wait(3.5f);
                moonGate.TryRequestActivation(); await Wait(1f);
                Look(new Vector3(200f, n, -1.6f), new Vector3(200f, 1.3f, 3f)); await Wait(2.5f);
                buttons[1].Press(); await Wait(1.5f);                 // wrong: MOON first
                buttons[0].Press(); await Wait(1f); buttons[2].Press(); await Wait(1.5f); // wrong: WOLF, BLOOD
                buttons[0].Press(); await Wait(1f); buttons[1].Press(); await Wait(1f); buttons[2].Press(); await Wait(2.5f);
                Look(new Vector3(200f, n, 1.0f), new Vector3(200f, 1.1f, 3.5f)); await Wait(2f);
                Look(new Vector3(199.2f, n, -1.8f), new Vector3(197.75f, 1f, -0.25f)); await Wait(1f);
                ritual.SolveCurrentStageForPresentation(); await Wait(1.5f);
                Look(new Vector3(201.0f, n, -1.8f), new Vector3(202.35f, 1.1f, 1.0f)); await Wait(3f);
                Look(new Vector3(201.0f, n, -3.8f), new Vector3(202.3f, 0.1f, -2.6f)); await Wait(1.5f);
                rune.TryReturn(); await Wait(0.5f);
                Look(new Vector3(0f, n, -12.5f), new Vector3(1.55f, 2.8f, -15.2f)); await Wait(3f);
            }
            catch (System.Exception e) { Debug.LogError("Moon Crypt recording failed: " + e.Message); }
            Debug.Log($"RECORDING DONE section06 frames={End()}");
        }

        private static System.Threading.Tasks.Task Wait(float seconds) =>
            System.Threading.Tasks.Task.Delay((int)(seconds * 1000));

        private static void Capture()
        {
            if (folder == null || !EditorApplication.isPlaying) return;
            double now = EditorApplication.timeSinceStartup;
            if (now < nextCapture) return;
            nextCapture = now + interval;
            ScreenCapture.CaptureScreenshot(Path.Combine(folder, $"frame_{frame++:D4}.png"));
        }

        private static void StopOnExit(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.ExitingPlayMode) End();
        }
    }
}
