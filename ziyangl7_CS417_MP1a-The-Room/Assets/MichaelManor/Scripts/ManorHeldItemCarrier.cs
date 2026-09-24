using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace MichaelManor
{
    /// Carries whatever the player holds across a single-mode scene load: before leaving it
    /// records the prefab of each held item per hand, and after the next scene loads it
    /// instantiates those prefabs and puts them back into the matching hand.
    public static class ManorHeldItemCarrier
    {
        private struct Carried
        {
            public GameObject prefab;
            public InteractorHandedness hand;
        }

        private static readonly List<Carried> pending = new List<Carried>();

        public static IReadOnlyList<GameObject> LastSpawned => lastSpawned;
        private static readonly List<GameObject> lastSpawned = new List<GameObject>();

        /// Records held carryable items and arms the spawn for the next loaded scene.
        public static int CaptureHeldItems()
        {
            pending.Clear();
            var seen = new HashSet<ManorCarryableItem>();
            foreach (XRBaseInputInteractor interactor in Object.FindObjectsByType<XRBaseInputInteractor>(FindObjectsSortMode.None))
            {
                if (!interactor.hasSelection || interactor.handedness == InteractorHandedness.None) continue;
                foreach (IXRSelectInteractable held in interactor.interactablesSelected)
                {
                    ManorCarryableItem item = held.transform.GetComponentInParent<ManorCarryableItem>();
                    if (item == null || item.CarryPrefab == null || !seen.Add(item)) continue;
                    pending.Add(new Carried { prefab = item.CarryPrefab, hand = interactor.handedness });
                }
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (pending.Count > 0) SceneManager.sceneLoaded += OnSceneLoaded;
            return pending.Count;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            var runner = new GameObject("MichaelCarriedItems").AddComponent<SpawnRunner>();
            SceneManager.MoveGameObjectToScene(runner.gameObject, scene);
            runner.Begin(new List<Carried>(pending));
            pending.Clear();
        }

        private sealed class SpawnRunner : MonoBehaviour
        {
            private List<Carried> items;

            public void Begin(List<Carried> carried)
            {
                items = carried;
                StartCoroutine(Spawn());
            }

            private IEnumerator Spawn()
            {
                // Let the new rig register its interactors and apply its first tracked pose. XRI keeps
                // a controller disabled until it is tracked, so give the hands a moment to appear.
                yield return null;
                yield return null;
                float deadline = Time.realtimeSinceStartup + 1.5f;
                while (items.Exists(c => FindHand(c.hand) == null) && Time.realtimeSinceStartup < deadline)
                    yield return null;
                lastSpawned.Clear();
                foreach (Carried carried in items)
                {
                    // Without a tracked hand (desktop), the item still arrives, in front of the player.
                    XRBaseInputInteractor hand = FindHand(carried.hand);
                    Transform anchor = hand != null && hand.attachTransform != null ? hand.attachTransform
                        : hand != null ? hand.transform : null;
                    Vector3 position = anchor != null ? anchor.position : Camera.main != null
                        ? Camera.main.transform.position + Camera.main.transform.forward * 0.5f : Vector3.up;
                    GameObject spawned = Instantiate(carried.prefab, position, anchor != null ? anchor.rotation : Quaternion.identity);
                    spawned.name = carried.prefab.name;
                    lastSpawned.Add(spawned);

                    var grab = spawned.GetComponentInChildren<XRGrabInteractable>();
                    if (hand == null || grab == null || hand.interactionManager == null) continue;
                    yield return null;   // the new interactable registers in OnEnable
                    if (hand.interactionManager.IsRegistered((IXRSelectInteractable)grab))
                        hand.interactionManager.SelectEnter((IXRSelectInteractor)hand, (IXRSelectInteractable)grab);
                }
                Destroy(gameObject);
            }

            // Prefer the hand's Near-Far interactor (every group rig has one), then any other.
            private static XRBaseInputInteractor FindHand(InteractorHandedness handedness)
            {
                XRBaseInputInteractor fallback = null;
                foreach (XRBaseInputInteractor interactor in FindObjectsByType<XRBaseInputInteractor>(FindObjectsSortMode.None))
                {
                    if (interactor.handedness != handedness || !interactor.isActiveAndEnabled) continue;
                    if (interactor is NearFarInteractor) return interactor;
                    if (fallback == null) fallback = interactor;
                }
                return fallback;
            }
        }
    }
}
