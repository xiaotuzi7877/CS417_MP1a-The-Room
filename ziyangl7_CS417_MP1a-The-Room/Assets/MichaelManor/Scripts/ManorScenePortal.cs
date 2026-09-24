using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace MichaelManor
{
    /// The exit Gate to the next room. It stays dark until the ritual is complete, then glows;
    /// walking the player's head into it (or pressing N on a keyboard) loads the next scene and
    /// carries any held items along.
    public sealed class ManorScenePortal : MonoBehaviour
    {
        [SerializeField] private ManorThreeStagePuzzle ritual;
        [SerializeField] private WinCelebrationController celebration;
        // The next room is whichever scene follows this one in Build Settings (Start, Ken, Michael,
        // Minh). Unity keeps that list correct when a scene is renamed or moved, so the teammate's
        // room can be reorganised without touching this portal. The path is only a fallback for
        // when this room is last in the list.
        [SerializeField] private string nextScenePath = "Assets/Scenes/SampleScene.unity";
        [SerializeField] private Vector3 triggerSize = new Vector3(3.2f, 3.4f, 1.6f);
        [SerializeField] private Transform swirl;
        [SerializeField] private Renderer[] glowRenderers;
        [SerializeField] private Light glowLight;
        [SerializeField] private TMP_Text label;
        [SerializeField] private Color glowColor = new Color(0.42f, 0.12f, 0.9f);

        private float glow;
        private bool loading;

        // Opens once the exit door has finished lifting and the win celebration has started.
        public bool IsOpen => ritual != null && ritual.IsComplete && (celebration == null || celebration.HasWon);
        public bool IsLoading => loading;
        public string NextScenePath => nextScenePath;

        /// Build index to load, or -1 to fall back to nextScenePath.
        public int NextBuildIndex
        {
            get
            {
                int next = SceneManager.GetActiveScene().buildIndex + 1;
                return next > 0 && next < SceneManager.sceneCountInBuildSettings ? next : -1;
            }
        }

        public string NextSceneDescription => NextBuildIndex >= 0
            ? SceneUtility.GetScenePathByBuildIndex(NextBuildIndex)
            : nextScenePath;
        public event Action<ManorScenePortal> Leaving;

        public void Configure(ManorThreeStagePuzzle ritualController, WinCelebrationController winCelebration,
            string scenePath, Transform swirlDisc, Renderer[] renderers, Light light, TMP_Text portalLabel)
        {
            ritual = ritualController;
            celebration = winCelebration;
            nextScenePath = scenePath;
            swirl = swirlDisc;
            glowRenderers = renderers;
            glowLight = light;
            label = portalLabel;
        }

        private void OnEnable()
        {
            loading = false;
            glow = IsOpen ? 1f : 0f;
            ApplyGlow();
        }

        private void Update()
        {
            float target = IsOpen ? 1f : 0f;
            glow = Mathf.MoveTowards(glow, target, Time.deltaTime / 1.5f);
            ApplyGlow();
            if (swirl != null && glow > 0f) swirl.Rotate(0f, 40f * Time.deltaTime, 0f, Space.Self);   // spin about the disc axis
            if (!IsOpen || loading || glow < 0.95f) return;

            Camera head = Camera.main;
            bool entered = head != null && IsInside(head.transform.position);
            Keyboard keyboard = Keyboard.current;
            if (entered || (keyboard != null && keyboard.nKey.wasPressedThisFrame)) TryEnter();
        }

        public bool IsInside(Vector3 worldPoint)
        {
            Vector3 local = transform.InverseTransformPoint(worldPoint);
            return Mathf.Abs(local.x) <= triggerSize.x * 0.5f && local.y >= 0f && local.y <= triggerSize.y &&
                   Mathf.Abs(local.z) <= triggerSize.z * 0.5f;
        }

        public bool TryEnter()
        {
            if (!IsOpen || loading) return false;
            if (NextBuildIndex < 0 && !Application.CanStreamedLevelBeLoaded(nextScenePath))
            {
                if (label != null) label.text = "THE NEXT ROOM IS NOT IN THE BUILD";
                Debug.LogError("ManorScenePortal: no scene after this room in Build Settings and cannot load " + nextScenePath, this);
                return false;
            }

            loading = true;
            int carried = ManorHeldItemCarrier.CaptureHeldItems();
            if (label != null) label.text = "ENTERING MINH'S ROOM...";
            Debug.Log($"Michael Manor portal: loading {NextSceneDescription} carrying {carried} held item(s).");
            Leaving?.Invoke(this);
            StartCoroutine(Load());
            return true;
        }

        private IEnumerator Load()
        {
            yield return null;
            int index = NextBuildIndex;
            AsyncOperation operation = index >= 0
                ? SceneManager.LoadSceneAsync(index, LoadSceneMode.Single)
                : SceneManager.LoadSceneAsync(nextScenePath, LoadSceneMode.Single);
            if (operation == null) { loading = false; yield break; }
        }

        private void ApplyGlow()
        {
            if (glowRenderers != null)
            {
                var block = new MaterialPropertyBlock();
                Color color = Color.Lerp(new Color(0.06f, 0.05f, 0.08f), glowColor, glow);
                foreach (Renderer target in glowRenderers)
                {
                    if (target == null) continue;
                    target.GetPropertyBlock(block);
                    block.SetColor("_BaseColor", color);
                    block.SetColor("_EmissionColor", glowColor * (glow * 1.1f));   // stays purple instead of blooming to white
                    target.SetPropertyBlock(block);
                }
            }
            if (glowLight != null) glowLight.intensity = glow * 2.5f;
            if (label != null && !loading) label.gameObject.SetActive(glow > 0.5f);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(0.6f, 0.4f, 1f, 0.35f);
            Gizmos.DrawCube(new Vector3(0f, triggerSize.y * 0.5f, 0f), triggerSize);
        }
    }
}
