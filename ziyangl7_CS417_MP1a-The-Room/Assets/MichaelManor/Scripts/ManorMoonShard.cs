using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MichaelManor
{
    [RequireComponent(typeof(XRSimpleInteractable), typeof(Collider))]
    public sealed class ManorMoonShard : MonoBehaviour
    {
        [SerializeField] private ManorMoonShardCollection collection;
        [SerializeField] private int shardIndex;
        private XRSimpleInteractable interactable;
        private Vector3 baseScale;
        public bool IsCollected { get; private set; }
        public int ShardIndex => shardIndex;

        public void Configure(ManorMoonShardCollection owner, int index) { collection = owner; shardIndex = index; }
        private void Awake() { interactable = GetComponent<XRSimpleInteractable>(); baseScale = transform.localScale; }
        private void OnEnable()
        {
            if (interactable == null) interactable = GetComponent<XRSimpleInteractable>();
            interactable.selectEntered.AddListener(HandleSelected);
        }
        private void OnDisable() { if (interactable != null) interactable.selectEntered.RemoveListener(HandleSelected); }
        private void Update()
        {
            transform.Rotate(Vector3.up, 42f * Time.deltaTime, Space.World);
            transform.localScale = baseScale * (1f + Mathf.Sin(Time.time * 3f + shardIndex) * 0.08f);
        }
        private void HandleSelected(SelectEnterEventArgs args) => collection?.TryCollect(this);
        public bool CollectForTest() => collection != null && collection.TryCollect(this);
        public void SetCollected(bool collected)
        {
            IsCollected = collected;
            if (collected) gameObject.SetActive(false);
            else { gameObject.SetActive(true); if (baseScale != Vector3.zero) transform.localScale = baseScale; }
        }
    }
}
