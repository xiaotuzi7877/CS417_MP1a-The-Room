using TMPro;
using UnityEngine;

namespace MichaelManor
{
    public sealed class ManorMoonShardCollection : MonoBehaviour
    {
        [SerializeField] private ManorMoonShard[] shards;
        [SerializeField] private TMP_Text display;
        [SerializeField] private ManorThreeStagePuzzle ritual;
        public int CollectedCount { get; private set; }
        public int TotalCount => shards != null ? shards.Length : 0;
        public TMP_Text Display => display;

        public void Configure(ManorMoonShard[] configuredShards, TMP_Text counter, ManorThreeStagePuzzle ritualController)
        {
            Unsubscribe(); shards = configuredShards; display = counter; ritual = ritualController;
            if (isActiveAndEnabled) Subscribe(); ResetCollection();
        }
        private void Awake() => ResetCollection();
        private void OnEnable() => Subscribe();
        private void OnDisable() => Unsubscribe();
        private void Subscribe() { if (ritual != null) { ritual.PuzzleReset -= ResetCollection; ritual.PuzzleReset += ResetCollection; } }
        private void Unsubscribe() { if (ritual != null) ritual.PuzzleReset -= ResetCollection; }

        public bool TryCollect(ManorMoonShard shard)
        {
            if (shard == null || shard.IsCollected) return false;
            shard.SetCollected(true); CollectedCount++; UpdateDisplay(); return true;
        }

        [ContextMenu("Reset Moon Shards")]
        public void ResetCollection()
        {
            CollectedCount = 0;
            if (shards != null) foreach (ManorMoonShard shard in shards) if (shard != null) shard.SetCollected(false);
            UpdateDisplay();
        }
        private void UpdateDisplay()
        {
            if (display != null) display.text = $"MOON SHARDS  {CollectedCount} / {TotalCount}";
        }
    }
}
