using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MichaelManor;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace MichaelManorEditor
{
    /// Section 8 integration QA. Drives the full five-chamber route through the real XR
    /// sockets and interactables, then checks resets, travel, false relics, drops, and the win.
    /// Prints one summary line: FULL RITUAL QA PASS/FAIL.
    public static class ManorFullRitualQATest
    {
        private static int errors;
        private static readonly List<string> failures = new List<string>();
        private static readonly List<string> notes = new List<string>();

        private static ManorThreeStagePuzzle ritual;
        private static FiveChamberQuestController quest;
        private static ManorQuestScoreboard board;
        private static ManorMoonCryptPuzzle crypt;
        private static WinCelebrationController celebration;
        private static ManorGateTravelSystem travel;
        private static ManorGatePortal[] gates;
        private static ManorReturnRune[] runes;
        private static ManorChamberReveal[] reveals;
        private static ManorMoonSequenceButton[] buttons;
        private static XRSocketInteractor[] sockets;
        private static ManorKeyArtifact fang, moon, blood;
        private static ManorKeyArtifact[] herrings;
        private static bool congratsHidden;

        [MenuItem("Tools/Michael Manor/Run Full Ritual QA (Play Mode)")]
        public static async void Run()
        {
            if (!EditorApplication.isPlaying) { Debug.LogWarning("Enter Play Mode first, then run the full ritual QA."); return; }
            errors = 0; failures.Clear(); notes.Clear(); congratsHidden = false;
            Application.logMessageReceived += Count;
            try { await Execute(); }
            catch (System.Exception e)
            {
                string where = e.StackTrace?.Split('\n').FirstOrDefault(l => l.Contains("ManorFullRitualQATest"))?.Trim();
                failures.Add($"exception: {e.Message} at {where}");
                Debug.LogWarning("FULL RITUAL QA exception detail: " + e);
            }
            finally { Application.logMessageReceived -= Count; }
            string result = failures.Count == 0 && errors == 0 ? "PASS" : "FAIL";
            Debug.Log($"FULL RITUAL QA {result} | console errors during test: {errors} | checks: {string.Join("; ", notes)} | failures: {(failures.Count == 0 ? "none" : string.Join("; ", failures))}");
        }

        private static async Task Execute()
        {
            ritual = Object.FindFirstObjectByType<ManorThreeStagePuzzle>();
            quest = Object.FindFirstObjectByType<FiveChamberQuestController>();
            board = Object.FindFirstObjectByType<ManorQuestScoreboard>();
            crypt = Object.FindFirstObjectByType<ManorMoonCryptPuzzle>();
            celebration = Object.FindFirstObjectByType<WinCelebrationController>();
            travel = Object.FindFirstObjectByType<ManorGateTravelSystem>();
            gates = Object.FindObjectsByType<ManorGatePortal>(FindObjectsSortMode.None).OrderBy(g => g.ChamberIndex).ToArray();
            runes = Object.FindObjectsByType<ManorReturnRune>(FindObjectsInactive.Include, FindObjectsSortMode.None).OrderBy(r => r.ChamberIndex).ToArray();
            reveals = Object.FindObjectsByType<ManorChamberReveal>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .OrderBy(r => ChamberIndexOf(r.transform)).ToArray();
            buttons = Object.FindObjectsByType<ManorMoonSequenceButton>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .OrderBy(b => b.SequenceIndex).ToArray();
            var artifacts = Object.FindObjectsByType<ManorKeyArtifact>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            fang = artifacts.FirstOrDefault(a => a.ArtifactId == "SilverFang");
            moon = artifacts.FirstOrDefault(a => a.ArtifactId == "Moonstone");
            blood = artifacts.FirstOrDefault(a => a.ArtifactId == "BloodSigil");
            herrings = artifacts.Where(a => a.ArtifactId.Contains("RedHerring")).ToArray();
            if (!Check(ritual && quest && board && crypt && celebration && travel && gates.Length == 5 && runes.Length == 5 &&
                       reveals.Length == 4 && buttons.Length == 3 && fang && moon && blood, "scene objects missing")) return;
            sockets = new[] { "SilverFangWatcherSocket", "MoonstoneOrrerySocket", "BloodSigilDoorSocket" }
                .Select(n => GameObject.Find(n)?.GetComponent<XRSocketInteractor>()).ToArray();
            if (!Check(sockets.All(s => s != null), "ritual sockets missing")) return;
            Application.logMessageReceived += WatchCongrats;

            StaticChecks();

            ritual.ResetPuzzle();
            await Wait(2f);
            Vector3 fangHome = fang.transform.position;
            await ExpectBoard(0, 0, "SILVER FANG", "start");

            // Route 2-4: grab Silver Fang and insert it through the real Watcher socket.
            await Insert(fang, 0, "Silver Fang");
            await Until(() => gates.All(g => g.IsUnlocked), 5f);
            Check(ritual.CurrentStage == 1 && gates.All(g => g.IsUnlocked), "Watcher Lock did not open the chest and unlock five Gates");
            await ExpectBoard(1, 0, "CODEX", "after Watcher Lock");
            await RejectAt(herrings, 1, "false relics at Celestial Lock");

            // QA: repeated Gate travel and every Return Rune, twice each.
            for (int pass = 0; pass < 2; pass++)
                for (int i = 0; i < 5; i++)
                {
                    bool entered = gates[i].TryRequestActivation();
                    float arrive = Vector3.Distance(Flat(travel.TrackedHead.position), Flat(gates[i].Destination.position));
                    if (i == 4) runes[i].gameObject.SetActive(true);   // Moon rune normally appears after its Lock
                    bool returned = runes[i].TryReturn();
                    float back = Vector3.Distance(Flat(travel.TrackedHead.position), Flat(gates[i].ReturnAnchor.position));
                    if (i == 4) runes[i].gameObject.SetActive(false);
                    Check(entered && returned && arrive < 0.05f && back < 0.05f, $"Gate/Return {i + 1} pass {pass + 1} misaligned");
                }
            Check(quest.ExploredCount == 0, "travel alone counted chambers");
            notes.Add("10 Gate trips aligned");

            // Route 5-8: four false chambers, deliberately out of order, each counted once.
            int[] order = { 2, 0, 3, 1 };
            for (int k = 0; k < order.Length; k++)
            {
                int i = order[k];
                gates[i].TryRequestActivation();
                await Wait(0.2f);
                Check(reveals[i].TryActivate(), $"chamber {i + 1} interaction failed");
                await Until(() => quest.ExploredCount == k + 1, 5f);
                Check(!reveals[i].TryActivate() && quest.ExploredCount == k + 1, $"chamber {i + 1} counted twice");
                runes[i].TryReturn();
            }
            await ExpectBoard(1, 4, "WOLF", "after four false chambers (Moon Crypt already visited)");

            // Route 9-12: Moon Crypt, wrong orders repeatedly, then Wolf -> Moon -> Blood.
            gates[4].TryRequestActivation();
            await Wait(0.3f);
            int[][] wrong = { new[] { 1 }, new[] { 2 }, new[] { 0, 2 }, new[] { 0, 0 }, new[] { 0, 1, 1 } };
            foreach (int[] attempt in wrong)
            {
                foreach (int b in attempt) { buttons[b].Press(); await Wait(0.05f); }
                await Wait(0.7f);
                Check(crypt.SequencePosition == 0 && !crypt.IsSolved && !moon.gameObject.activeInHierarchy,
                    $"wrong order {string.Join("", attempt)} was not rejected");
            }
            notes.Add("5 wrong orders rejected");
            buttons[0].Press(); await Wait(0.3f); buttons[1].Press(); await Wait(0.3f); buttons[2].Press();
            await Until(() => moon.gameObject.activeInHierarchy, 4f);
            await Wait(0.3f);
            Check(crypt.IsSolved && moon.gameObject.activeInHierarchy && quest.ExploredCount == 5, "Moon Crypt did not release the Moonstone");
            await ExpectBoard(1, 5, "CELESTIAL", "after Moon Crypt sequence");
            await RejectAt(new[] { moon }, 2, "Moonstone at the Exit Lock (out of order)");

            // Route 13-15: Moonstone into the Celestial Lock, Blood Sigil revealed, return to hall.
            await Insert(moon, 1, "Moonstone");
            await Wait(1.5f);
            Check(ritual.CurrentStage == 2 && blood.gameObject.activeInHierarchy, "Celestial Lock did not reveal the Blood Sigil");
            await ExpectBoard(2, 5, "BLOOD SIGIL", "after Celestial Lock");
            await RejectAt(herrings, 2, "false relics at Exit Lock");
            Check(runes[4].gameObject.activeSelf && runes[4].TryReturn(), "Moon Crypt Return Rune failed");

            // Route 16-17: Blood Sigil into the Exit Lock; door, lights, celebration, text fade.
            Vector3 doorClosed = DoorPosition();
            await Insert(blood, 2, "Blood Sigil");
            await Until(() => ritual.IsComplete, 6f);
            await Until(() => celebration.HasWon, 10f);       // the win follows the seal and door animation
            Check(ritual.IsComplete && celebration.HasWon, "Exit Lock did not trigger the win");
            Check(Vector3.Distance(doorClosed, DoorPosition()) > 0.5f, "exit door did not open");
            await ExpectBoard(3, 5, "COMPLETE", "after Exit Lock");
            await Until(() => congratsHidden, 8f);
            Check(congratsHidden, "Congratulations text did not fade after five seconds");

            // QA: reset from every major stage.
            await ResetFrom(3, fangHome);
            await ResetAt(0, fangHome, async () =>
            {
                Vector3 carried = new Vector3(3f, 1.2f, -4f);             // player carried the Fang off and dropped it
                fang.transform.position = carried;
                fang.GetComponent<Rigidbody>().position = carried;
                await Wait(1.5f);
            });
            await ResetAt(1, fangHome, async () =>
            {
                await Insert(fang, 0, "Silver Fang");
                await Until(() => gates.All(g => g.IsUnlocked), 5f);
                gates[1].TryRequestActivation(); reveals[1].TryActivate(); await Wait(1.5f); runes[1].TryReturn();
                gates[4].TryRequestActivation(); buttons[0].Press(); buttons[1].Press(); buttons[2].Press();
                await Until(() => moon.gameObject.activeInHierarchy, 4f);
            });
            await ResetAt(2, fangHome, async () =>
            {
                await Insert(fang, 0, "Silver Fang");
                await Until(() => gates.All(g => g.IsUnlocked), 5f);
                buttons[0].Press(); buttons[1].Press(); buttons[2].Press();
                await Until(() => moon.gameObject.activeInHierarchy, 4f);
                await Wait(0.3f);
                await Insert(moon, 1, "Moonstone");
            });
            notes.Add("reset from stages 0-3");

            // QA: drop every grabbable onto the hall floor.
            await DropTest();
            ritual.ResetPuzzle();
            await Wait(1.5f);
            Check(ritual.CurrentStage == 0, "final reset did not hold at stage 0");
            Application.logMessageReceived -= WatchCongrats;
        }

        private static void StaticChecks()
        {
            float[] masses = { fang.GetComponent<Rigidbody>().mass, moon.GetComponent<Rigidbody>().mass, blood.GetComponent<Rigidbody>().mass };
            float ratio = masses.Max() / masses.Min();
            Check(Mathf.Abs(ratio - 20f) < 0.001f, $"Key Prop mass ratio is {ratio:F3}, not 20:1");
            Check(herrings.Length >= 3 && herrings.All(h => h.GetComponent<Rigidbody>() && h.GetComponent<XRGrabInteractable>()),
                "fewer than three grabbable red herrings");
            var badBodies = Object.FindObjectsByType<Rigidbody>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(b => !b.isKinematic && b.GetComponent<XRGrabInteractable>() == null && !b.name.Contains("SpawnBall"))
                .Select(b => b.name).ToArray();
            Check(badBodies.Length == 0, "static objects with dynamic Rigidbody: " + string.Join(",", badBodies));
            notes.Add($"mass ratio {ratio:F1}:1, {herrings.Length} red herrings, no stray dynamic bodies");
        }

        private static async Task Insert(ManorKeyArtifact artifact, int socketIndex, string label)
        {
            int before = ritual.CurrentStage;
            Place(artifact, sockets[socketIndex]);
            await Until(() => ritual.CurrentStage > before, 8f);
            Check(ritual.CurrentStage == before + 1, $"{label} was not accepted by its Lock socket");
            await Wait(0.3f);
        }

        private static async Task RejectAt(ManorKeyArtifact[] items, int socketIndex, string label)
        {
            int before = ritual.CurrentStage;
            foreach (ManorKeyArtifact item in items)
            {
                Vector3 home = item.transform.position;
                bool wasActive = item.gameObject.activeSelf;
                item.gameObject.SetActive(true);
                Place(item, sockets[socketIndex]);
                await Wait(2.5f);
                Check(ritual.CurrentStage == before, $"{label}: {item.name} advanced the ritual");
                Check(!sockets[socketIndex].hasSelection &&
                      Vector3.Distance(item.transform.position, sockets[socketIndex].transform.position) > 0.5f,
                    $"{label}: {item.name} stayed stuck in the Lock after rejection");
                Rigidbody body = item.GetComponent<Rigidbody>();
                item.transform.position = home;
                if (body != null) { body.position = home; if (!body.isKinematic) body.linearVelocity = Vector3.zero; }
                item.gameObject.SetActive(wasActive);
                await Wait(0.2f);
            }
            notes.Add(label + " rejected");
        }

        private static void Place(ManorKeyArtifact artifact, XRSocketInteractor socket)
        {
            Transform anchor = socket.attachTransform != null ? socket.attachTransform : socket.transform;
            artifact.gameObject.SetActive(true);
            Rigidbody body = artifact.GetComponent<Rigidbody>();
            artifact.transform.SetPositionAndRotation(anchor.position, anchor.rotation);
            if (body != null)
            {
                body.position = anchor.position;
                body.rotation = anchor.rotation;
                if (!body.isKinematic) { body.linearVelocity = Vector3.zero; body.angularVelocity = Vector3.zero; }
            }
        }

        private static async Task ResetAt(int stage, Vector3 fangHome, System.Func<Task> reach)
        {
            await reach();
            Check(ritual.CurrentStage == stage, $"could not reach stage {stage} before reset test");
            await ResetFrom(stage, fangHome);
        }

        private static async Task ResetFrom(int stage, Vector3 fangHome)
        {
            ritual.ResetPuzzle();
            await Wait(2f);
            string when = $"reset from stage {stage}";
            Check(ritual.CurrentStage == 0, $"{when}: stage advanced by itself");
            Check(Vector3.Distance(fang.transform.position, fangHome) < 0.1f, $"{when}: Silver Fang not back on its table");
            Check(!moon.gameObject.activeInHierarchy && !blood.gameObject.activeInHierarchy, $"{when}: later keys still visible");
            Check(gates.All(g => !g.IsUnlocked && !g.IsExplored && !g.ExploredLabel.gameObject.activeInHierarchy), $"{when}: Gates not locked/cleared");
            Check(quest.ExploredCount == 0 && !crypt.IsSolved && !celebration.HasWon, $"{when}: chamber/crypt/win state survived");
            await ExpectBoard(0, 0, "SILVER FANG", when);
        }

        private static async Task DropTest()
        {
            ritual.ResetPuzzle();
            await Wait(1f);
            var grabbables = Object.FindObjectsByType<XRGrabInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(g => g.GetComponent<Rigidbody>() != null).ToArray();
            var saved = grabbables.Select(g => (grab: g, active: g.gameObject.activeSelf, pos: g.transform.position,
                rot: g.transform.rotation, kinematic: g.GetComponent<Rigidbody>().isKinematic)).ToArray();
            // Test one prop at a time over the verified-clear centre aisle. Testing every prop
            // simultaneously made later props land on furniture or on one another, which measured
            // the room layout rather than whether each object has working solid-body physics.
            foreach (var item in saved)
            {
                Rigidbody body = item.grab.GetComponent<Rigidbody>();
                item.grab.gameObject.SetActive(true);
                Vector3 drop = new Vector3(0f, 1.5f, -10f);
                item.grab.transform.SetPositionAndRotation(drop, Quaternion.identity);
                body.position = drop;
                body.rotation = Quaternion.identity;
                body.isKinematic = false;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                await Wait(2f);

                Collider[] solidColliders = item.grab.GetComponentsInChildren<Collider>(true)
                    .Where(c => c.enabled && !c.isTrigger).ToArray();
                float lowestPoint = solidColliders.Length > 0
                    ? solidColliders.Min(c => c.bounds.min.y)
                    : item.grab.transform.position.y;
                Check(solidColliders.Length > 0 && lowestPoint > -0.08f && lowestPoint < 0.18f &&
                      body.linearVelocity.magnitude < 0.2f,
                    $"{item.grab.name} did not settle on the floor (lowest={lowestPoint:F2}, speed={body.linearVelocity.magnitude:F2})");

                item.grab.transform.SetPositionAndRotation(item.pos, item.rot);
                body.position = item.pos;
                body.rotation = item.rot;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.isKinematic = item.kinematic;
                item.grab.gameObject.SetActive(item.active);
                await Wait(0.1f);
            }
            notes.Add($"{grabbables.Length} grabbables individually dropped and settled");
        }

        private static async Task ExpectBoard(int stage, int explored, string objectiveWord, string when)
        {
            await Wait(0.2f);   // the board refreshes in LateUpdate
            string progress = board.ProgressValue ?? "", objective = board.InstructionValue ?? "";
            Check(progress.Contains($"RITUAL PROGRESS    {stage} / 3") && progress.Contains($"CHAMBERS EXPLORED  {explored} / 5") &&
                  progress.Contains($"KEYS REMAINING     {3 - stage}"), $"{when}: board shows '{progress.Replace('\n', ' ')}'");
            Check(objective.Contains(objectiveWord), $"{when}: objective '{objective.Replace('\n', ' ')}' lacks {objectiveWord}");
        }

        private static Vector3 DoorPosition()
        {
            var so = new SerializedObject(ritual);
            var door = so.FindProperty("exitDoor").objectReferenceValue as Transform;
            return door != null ? door.position : Vector3.zero;
        }

        private static int ChamberIndexOf(Transform item)
        {
            while (item != null && !item.name.StartsWith("Chamber_")) item = item.parent;
            return item != null ? int.Parse(item.name.Substring(8, 2)) - 1 : -1;
        }

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);
        private static bool Check(bool ok, string message) { if (!ok) failures.Add(message); return ok; }
        private static void Count(string c, string s, LogType t) { if (t == LogType.Error || t == LogType.Exception) errors++; }
        private static void WatchCongrats(string c, string s, LogType t) { if (c.Contains("Congratulations text hidden")) congratsHidden = true; }
        private static Task Wait(float seconds) => Task.Delay((int)(seconds * 1000));
        private static async Task Until(System.Func<bool> condition, float timeout)
        {
            float end = Time.realtimeSinceStartup + timeout;
            while (!condition() && Time.realtimeSinceStartup < end) await Task.Delay(50);
        }
    }
}
