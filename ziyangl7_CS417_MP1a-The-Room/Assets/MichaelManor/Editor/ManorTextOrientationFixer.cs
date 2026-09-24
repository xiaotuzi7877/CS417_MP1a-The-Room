using System.Collections.Generic;
using System.Linq;
using MichaelManor;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MichaelManorEditor
{
    /// Single source of truth for which way every world-space text in Michael's room faces.
    /// TextMeshPro is readable only when the camera looks along the text's +Z (its readable face
    /// points -Z); the project's TMP materials disable culling, so the other side shows mirrored.
    /// Idempotent: installers call Apply() before saving so a reinstall never reintroduces a flip.
    public static class ManorTextOrientationFixer
    {
        private const float Standoff = 0.012f;

        [MenuItem("Tools/Michael Manor/Fix Text Orientation")]
        public static void Run()
        {
            Scene scene = SceneManager.GetActiveScene();
            int changed = Apply();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"Text orientation fixed on {changed} objects.");
        }

        public static int Apply()
        {
            int changed = 0;

            // Wall/board-mounted texts: readable face toward where the player stands.
            changed += Face(Find("RitualProgressBoard", "ProgressText"), Vector3.forward);
            changed += Face(Find("RitualProgressBoard", "CurrentClueText"), Vector3.forward);
            changed += Face(Find("RitualProgressBoard", "PuzzleAndClueCounter"), Vector3.forward);
            foreach (string name in new[] { "Label_BAT", "Label_WOLF", "Label_MOON", "PortraitSequenceStatus" })
                changed += Face(Find("SilverFangReleasePuzzle", name), Vector3.back);          // player stands at -Z of the case
            changed += Face(Find("VR_Restart_Control", "Restart_Label"), Vector3.forward);      // player stands at +Z of the plate
            changed += Face(Find("BloodSigilLeverPuzzle", "LeverSequenceStatus"), Vector3.back); // player stands at -Z of the seal

            // Texts that sat behind their plaque, on the wall side: move to the hall face and turn round.
            changed += OntoFace(Find("BlacklightHiddenPlaque", "BlacklightInstruction"), Find("BlacklightHiddenPlaque", "PlaqueBacking"), Vector3.left);
            changed += OntoFace(Find("BlacklightHiddenPlaque", "BlacklightHiddenMessage"), Find("BlacklightHiddenPlaque", "PlaqueBacking"), Vector3.left);
            changed += OntoFace(Find("InvisibleWritingInspection", "InspectionInstruction"), Find("InvisibleWritingInspection", "ObliqueWritingPlaque"), Vector3.left);
            changed += OntoFace(Find("InvisibleWritingInspection", "AngleHiddenWriting"), Find("InvisibleWritingInspection", "ObliqueWritingPlaque"), Vector3.left);

            // Codex pages and map lie flat: face up, text top toward the chest's far side (-X),
            // so a player standing on the hall side (+X) reads them left to right.
            foreach (string name in new[] { "CodexTitle", "CodexClue", "MapLabel" })
                changed += LayFlat(Find("RevealedContents", name));

            changed += FixMirrorClue(Find("HandmirrorInspection", "ReversedMirrorClue"));
            changed += FixExploredLabels();
            changed += FitClueText();

            // Free-standing labels and labels on grabbable props turn toward the player at runtime.
            var billboards = new List<Transform>
            {
                Find("HandmirrorInspection", "HandmirrorLabel"), Find("MagnifyingGlassInspection", "MagnifierLabel"),
                Find("Content_Section06", "Label_WOLF"), Find("Content_Section06", "Label_MOON"),
                Find("Content_Section06", "Label_BLOOD"), Find("Lock_02_CelestialConsole", "CelestialLockLabel"),
                Find("Content_Section05", "OpenLabel"), Find("Content_Section05", "PressEyeLabel"),
            };
            billboards.AddRange(AllNamed("FalseRelicLabel"));
            foreach (Transform label in billboards.Where(t => t != null))
            {
                if (label.GetComponent<ManorFaceViewer>() != null) continue;
                label.gameObject.AddComponent<ManorFaceViewer>();
                EditorUtility.SetDirty(label.gameObject);
                changed++;
            }
            return changed;
        }

        /// Rotates so the readable face points along readableNormal (world, horizontal).
        private static int Face(Transform text, Vector3 readableNormal)
        {
            if (text == null) return 0;
            Quaternion target = Quaternion.LookRotation(-readableNormal, Vector3.up);
            if (Quaternion.Angle(text.rotation, target) < 0.5f) return 0;
            text.rotation = target;
            EditorUtility.SetDirty(text);
            return 1;
        }

        /// Places the text just in front of the backing's face on the readableNormal side.
        private static int OntoFace(Transform text, Transform backing, Vector3 readableNormal)
        {
            if (text == null || backing == null) return 0;
            Bounds b = backing.GetComponent<Renderer>().bounds;
            Vector3 p = text.position;
            float face = Vector3.Dot(b.center, readableNormal) + Vector3.Dot(b.extents, new Vector3(Mathf.Abs(readableNormal.x), Mathf.Abs(readableNormal.y), Mathf.Abs(readableNormal.z)));
            float along = Vector3.Dot(p, readableNormal);
            Vector3 target = p + readableNormal * (face + Standoff - along);
            int changed = Face(text, readableNormal);
            if ((text.position - target).sqrMagnitude > 1e-8f) { text.position = target; EditorUtility.SetDirty(text); changed = 1; }
            return changed;
        }

        private static int LayFlat(Transform text)
        {
            if (text == null) return 0;
            // readable face up (+Y): +Z points down; text "up" points to -X (away from the reader).
            Quaternion target = Quaternion.LookRotation(Vector3.down, Vector3.left);
            float surface = SurfaceBelow(text);
            Vector3 position = text.position;
            if (!float.IsNaN(surface)) position.y = surface + 0.004f;
            if (Quaternion.Angle(text.rotation, target) < 0.5f && (text.position - position).sqrMagnitude < 1e-8f) return 0;
            text.SetPositionAndRotation(position, target);
            EditorUtility.SetDirty(text);
            return 1;
        }

        private static float SurfaceBelow(Transform text)
        {
            float best = float.NaN;
            foreach (Renderer r in text.parent.GetComponentsInChildren<Renderer>(true))
            {
                if (r.GetComponent<TMP_Text>() != null) continue;
                Bounds b = r.bounds;
                Vector3 p = text.position;
                if (p.x < b.min.x || p.x > b.max.x || p.z < b.min.z || p.z > b.max.z || b.max.y > p.y + 0.05f) continue;
                if (float.IsNaN(best) || b.max.y > best) best = b.max.y;
            }
            return best;
        }

        /// True mirror writing: normal text rendered left-right flipped, facing the hall (+X), so
        /// the wall shows mirror script and the hand mirror shows it the right way round.
        private static int FixMirrorClue(Transform clue)
        {
            if (clue == null) return 0;
            TMP_Text text = clue.GetComponent<TMP_Text>();
            int changed = Face(clue, Vector3.right);
            if (text.text != "LEFT, THEN RIGHT") { text.text = "LEFT, THEN RIGHT"; EditorUtility.SetDirty(text); changed = 1; }
            Vector3 scale = clue.localScale;
            if (scale.x > 0f) { clue.localScale = new Vector3(-scale.x, scale.y, scale.z); EditorUtility.SetDirty(clue); changed = 1; }
            return changed;
        }

        /// Wall Gate panels stand out from the wall and face along Z, with the rune on one face. One
        /// EXPLORED on each face (a child copy on the back) reads correctly from either end of the
        /// hall. The floor Gate has no panel, so its label turns toward the player instead.
        private static int FixExploredLabels()
        {
            int changed = 0;
            foreach (ManorGatePortal gate in Object.FindObjectsByType<ManorGatePortal>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Transform label = gate.ExploredLabel != null ? gate.ExploredLabel.transform : null;
                Transform visual = gate.transform.Find("Visual");
                Transform rune = visual != null ? visual.Find("GateRune") : null;
                if (label == null || rune == null) continue;
                if (visual.Find("LooseFloorboard") != null)
                {
                    if (label.GetComponent<ManorFaceViewer>() == null) { label.gameObject.AddComponent<ManorFaceViewer>(); changed++; }
                    continue;
                }

                Renderer panel = visual.GetComponentsInChildren<Renderer>(true)
                    .Where(r => r.transform != rune && r.GetComponent<TMP_Text>() == null)
                    .OrderByDescending(r => r.bounds.size.sqrMagnitude).First();
                Vector3 front = rune.position - panel.bounds.center;
                front.y = 0f;
                front = Mathf.Abs(front.x) > Mathf.Abs(front.z) ? new Vector3(Mathf.Sign(front.x), 0f, 0f) : new Vector3(0f, 0f, Mathf.Sign(front.z));
                float half = Vector3.Dot(panel.bounds.extents, new Vector3(Mathf.Abs(front.x), 0f, Mathf.Abs(front.z)));
                float height = rune.position.y + 0.62f;
                Vector3 center = panel.bounds.center;

                changed += Place(label, new Vector3(center.x, height, center.z) + front * (half + Standoff), front);
                Transform back = label.Find("ExploredLabelBack");
                if (back == null)
                {
                    back = Object.Instantiate(label.gameObject, label).transform;
                    back.name = "ExploredLabelBack";
                    back.gameObject.SetActive(true);
                    changed++;
                }
                changed += Place(back, new Vector3(center.x, height, center.z) - front * (half + Standoff), -front);
            }
            return changed;
        }

        private static int Place(Transform text, Vector3 position, Vector3 readableNormal)
        {
            int changed = Face(text, readableNormal);
            if ((text.position - position).sqrMagnitude > 1e-8f) { text.position = position; EditorUtility.SetDirty(text); changed = 1; }
            return changed;
        }

        /// Clue plaque text was wider (1.5 m) than its 0.82 m backing; keep it on the board.
        private static int FitClueText()
        {
            int changed = 0;
            foreach (string plaque in new[] { "Clue_01_Watcher", "Clue_02_Heavens", "Clue_03_Exit" })
            {
                TMP_Text text = Find(plaque, "Text")?.GetComponent<TMP_Text>();
                if (text == null || (text.enableAutoSizing && text.rectTransform.sizeDelta.x <= 0.78f)) continue;
                text.enableAutoSizing = true;
                text.fontSizeMax = text.fontSize;
                text.fontSizeMin = text.fontSize * 0.3f;
                text.rectTransform.sizeDelta = new Vector2(0.76f, 0.16f);
                EditorUtility.SetDirty(text);
                changed++;
            }
            return changed;
        }

        private static IEnumerable<Transform> All() => SceneManager.GetActiveScene().GetRootGameObjects()
            .SelectMany(r => r.GetComponentsInChildren<Transform>(true));
        private static IEnumerable<Transform> AllNamed(string name) => All().Where(t => t.name == name);
        private static Transform Find(string parentName, string name) =>
            All().FirstOrDefault(t => t.name == name && t.GetComponentsInParent<Transform>(true).Any(p => p.name == parentName));
    }
}
