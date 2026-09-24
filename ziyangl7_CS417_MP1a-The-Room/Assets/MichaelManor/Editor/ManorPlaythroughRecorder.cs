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

        [MenuItem("Tools/Michael Manor/Record Quest Progress Playthrough (Play Mode)")]
        public static async void RecordQuestProgress()
        {
            if (Begin("section07") == null) return;
            try
            {
                var ritual = Object.FindFirstObjectByType<ManorThreeStagePuzzle>();
                var gates = Object.FindObjectsByType<ManorGatePortal>(FindObjectsSortMode.None).OrderBy(g => g.ChamberIndex).ToArray();
                var reveals = Object.FindObjectsByType<ManorChamberReveal>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                var runes = Object.FindObjectsByType<ManorReturnRune>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                var buttons = Object.FindObjectsByType<ManorMoonSequenceButton>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                    .OrderBy(b => b.SequenceIndex).ToArray();
                float n = float.NaN;
                Vector3 start = new Vector3(0f, n, -12.5f), board = new Vector3(1.55f, 2.8f, -15.2f);
                Vector3 hallView = new Vector3(0f, n, -9.5f), northGates = new Vector3(0f, 1.8f, 4f);

                ritual.ResetPuzzle(); await Wait(0.8f);
                Look(start, board); await Wait(3f);                                   // readable board at start pose
                Look(hallView, northGates); await Wait(2f);                           // dark, locked runes
                Look(new Vector3(-6.2f, n, -9.3f), new Vector3(-8.05f, 1.8f, -9.3f)); await Wait(0.8f);
                ritual.SolveCurrentStageForPresentation(); await Wait(3.5f);
                Look(start, board); await Wait(2.5f);                                 // 1/3 and Codex objective
                Look(hallView, northGates); await Wait(2.5f);                         // purple + blue-white runes
                for (int i = 0; i < 4; i++)
                {
                    gates[i].TryRequestActivation(); await Wait(1f);
                    reveals.First(r => ChamberIndexOf(r.transform) == i).TryActivate(); await Wait(2.2f);
                    runes.First(r => r.ChamberIndex == i).TryReturn(); await Wait(0.3f);
                }
                Look(hallView, northGates); await Wait(2.5f);                         // four green + EXPLORED
                Look(new Vector3(-6.4f, n, -3.2f), new Vector3(-7.85f, 1.9f, -1f)); await Wait(2f);
                Look(start, board); await Wait(2.5f);                                 // 4 / 5
                gates[4].TryRequestActivation(); await Wait(1.5f);
                buttons[0].Press(); await Wait(0.8f); buttons[1].Press(); await Wait(0.8f); buttons[2].Press(); await Wait(2.5f);
                ritual.SolveCurrentStageForPresentation(); await Wait(2.5f);
                runes.First(r => r.ChamberIndex == 4).TryReturn(); await Wait(0.3f);
                Look(start, board); await Wait(3f);                                   // 2/3, 5/5, Blood Sigil objective
                Look(new Vector3(5.8f, n, -9f), new Vector3(7.85f, 2.1f, -5.5f)); await Wait(2f);
                Look(new Vector3(0f, n, 4f), new Vector3(0f, 2f, 10.6f)); await Wait(0.5f);
                ritual.SolveCurrentStageForPresentation(); await Wait(4f);
                Look(start, board); await Wait(3f);                                   // 3/3 complete
            }
            catch (System.Exception e) { Debug.LogError("Quest progress recording failed: " + e.Message); }
            Debug.Log($"RECORDING DONE section07 frames={End()}");
        }

        [MenuItem("Tools/Michael Manor/Record Full Ritual Route (Play Mode)")]
        public static async void RecordFullRoute()
        {
            if (Begin("section08", 6f) == null) return;
            try
            {
                var ritual = Object.FindFirstObjectByType<ManorThreeStagePuzzle>();
                var gates = Object.FindObjectsByType<ManorGatePortal>(FindObjectsSortMode.None).OrderBy(g => g.ChamberIndex).ToArray();
                var reveals = Object.FindObjectsByType<ManorChamberReveal>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                var runes = Object.FindObjectsByType<ManorReturnRune>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                var buttons = Object.FindObjectsByType<ManorMoonSequenceButton>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                    .OrderBy(b => b.SequenceIndex).ToArray();
                var artifacts = Object.FindObjectsByType<ManorKeyArtifact>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                ManorKeyArtifact Key(string id) => artifacts.First(a => a.ArtifactId == id);
                var travel = Object.FindFirstObjectByType<ManorGateTravelSystem>();
                float n = float.NaN;
                Vector3 start = new Vector3(0f, n, -12.5f), board = new Vector3(1.55f, 2.8f, -15.2f);

                ritual.ResetPuzzle(); await Wait(1f);
                Look(start, board); await Wait(3f);                                                    // 1 task board
                Look(new Vector3(0f, n, -9.5f), new Vector3(0f, 1.8f, 4f)); await Wait(2f);             //   dark Gate runes
                Vector3 fangAt = Key("SilverFang").transform.position;                                   // 2 sealed Silver Fang
                Look(new Vector3(fangAt.x, n, fangAt.z - 2.6f), fangAt); await Wait(2f);
                foreach (string rune in new[] { "PortraitRune_WOLF", "PortraitRune_BAT", "PortraitRune_WOLF", "PortraitRune_MOON" })
                {                                                                                          //   wrong start, then BAT WOLF MOON
                    GameObject.Find(rune)?.GetComponent<ManorKeyReleaseButton>()?.PressForTest(); await Wait(0.9f);
                }
                await Wait(2f);
                Look(new Vector3(-6.2f, n, -9.3f), new Vector3(-8.05f, 1.8f, -9.3f)); await Wait(0.8f);
                await Carry(Key("SilverFang"), "SilverFangWatcherSocket", 1.5f);                         // 3 into the Watcher Lock
                await Wait(2f);
                Look(new Vector3(-3.4f, n, -3f), new Vector3(-4.9f, 1.2f, -3f)); await Wait(3f);        // 4 chest opens on the Codex
                Look(new Vector3(0f, n, -9.5f), new Vector3(0f, 1.8f, 4f)); await Wait(2.5f);           //   purple + blue-white runes
                for (int i = 0; i < 4; i++)                                                              // 5-8 four false chambers
                {
                    gates[i].TryRequestActivation(); await Wait(1.2f);
                    reveals.First(r => ChamberIndexOf(r.transform) == i).TryActivate(); await Wait(2.5f);
                    runes.First(r => r.ChamberIndex == i).TryReturn(); await Wait(0.3f);
                }
                Look(new Vector3(0f, n, -9.5f), new Vector3(0f, 1.8f, 4f)); await Wait(2.5f);           //   green + EXPLORED, 4 / 5
                Look(start, board); await Wait(2.5f);
                gates[4].TryRequestActivation(); await Wait(1f);                                         // 9 Moon Crypt
                Look(new Vector3(200f, n, -1.6f), new Vector3(200f, 1.3f, 3f)); await Wait(2.5f);
                buttons[1].Press(); await Wait(1.5f);                                                    // 10 wrong order, red flash
                buttons[0].Press(); await Wait(0.9f); buttons[1].Press(); await Wait(0.9f); buttons[2].Press(); // 11 correct order
                await Wait(2.5f);                                                                         // 12 slab slides, Moonstone
                Look(new Vector3(199.2f, n, -1.8f), new Vector3(197.75f, 1f, -0.25f)); await Wait(0.8f);
                await Carry(Key("Moonstone"), "MoonstoneOrrerySocket", 1.5f);                            // 13 Celestial Lock
                await Wait(1.5f);
                Look(new Vector3(201.0f, n, -1.8f), new Vector3(202.35f, 1.1f, 1.0f)); await Wait(2.5f); // 14 slab rises on the lever seal
                foreach (string lever in new[] { "Lever_LEFT", "Lever_RIGHT" })
                {
                    GameObject.Find(lever)?.GetComponent<ManorKeyReleaseButton>()?.PressForTest(); await Wait(0.9f);
                }
                await Wait(2.5f);                                                                         //   Blood Sigil released
                Look(new Vector3(201.0f, n, -3.8f), new Vector3(202.3f, 0.1f, -2.6f)); await Wait(1.2f);
                var blood = Key("BloodSigil");
                runes.First(r => r.ChamberIndex == 4).TryReturn(); await Wait(0.3f);                     // 15 back to the hall
                Look(start, board); await Wait(2.5f);
                Look(new Vector3(0f, n, 5.5f), new Vector3(0f, 2f, 10.6f)); await Wait(0.8f);
                await Carry(blood, "BloodSigilDoorSocket", 1.5f);                                        // 16 Exit Lock
                await Wait(6f);                                                                           // 17 door, lights, text
                Look(new Vector3(0f, n, 2f), new Vector3(0f, 2f, 10.6f)); await Wait(4f);
                Look(start, board); await Wait(3f);
            }
            catch (System.Exception e) { Debug.LogError("Full route recording failed: " + e.Message); }
            Debug.Log($"RECORDING DONE section08 frames={End()}");
        }

        /// Moves a key along a short visible path into the socket anchor so the real socket selects it.
        private static async System.Threading.Tasks.Task Carry(ManorKeyArtifact key, string socketName, float seconds)
        {
            var socket = GameObject.Find(socketName).GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
            Transform anchor = socket.attachTransform != null ? socket.attachTransform : socket.transform;
            Rigidbody body = key.GetComponent<Rigidbody>();
            bool wasKinematic = body.isKinematic;
            body.isKinematic = true;
            Vector3 from = key.transform.position;
            Vector3 lift = Vector3.Lerp(from, anchor.position, 0.5f) + Vector3.up * 0.4f;
            int steps = Mathf.Max(1, Mathf.RoundToInt(seconds * 30f));
            for (int i = 1; i <= steps; i++)
            {
                float t = Mathf.SmoothStep(0f, 1f, i / (float)steps);
                Vector3 a = Vector3.Lerp(from, lift, t), b = Vector3.Lerp(lift, anchor.position, t);
                key.transform.position = Vector3.Lerp(a, b, t);
                body.position = key.transform.position;
                await Wait(seconds / steps);
            }
            key.transform.rotation = anchor.rotation;
            body.rotation = anchor.rotation;
            body.isKinematic = wasKinematic;
        }

        private static int ChamberIndexOf(Transform item)
        {
            while (item != null && !item.name.StartsWith("Chamber_")) item = item.parent;
            return item != null ? int.Parse(item.name.Substring(8, 2)) - 1 : -1;
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
