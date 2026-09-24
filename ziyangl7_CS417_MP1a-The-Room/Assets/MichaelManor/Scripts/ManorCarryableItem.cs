using UnityEngine;

namespace MichaelManor
{
    /// Marks a grabbable object that can travel to the next scene. The scene object itself is
    /// left behind; ManorHeldItemCarrier instantiates this prefab in the player's hand instead.
    [DisallowMultipleComponent]
    public sealed class ManorCarryableItem : MonoBehaviour
    {
        [SerializeField] private GameObject carryPrefab;

        public GameObject CarryPrefab => carryPrefab;

        public void Configure(GameObject prefab)
        {
            carryPrefab = prefab;
        }
    }
}
