using System.Collections;
using TMPro;
using UnityEngine;

namespace MichaelManor
{
    public sealed class ManorMoonCryptTimer : MonoBehaviour
    {
        [SerializeField] private ManorGatePortal moonGate;
        [SerializeField] private ManorReturnRune returnRune;
        [SerializeField] private ManorGateTravelSystem travel;
        [SerializeField] private ManorMoonCryptPuzzle puzzle;
        [SerializeField] private FiveChamberQuestController quest;
        [SerializeField] private TMP_Text display;
        [SerializeField] private float duration = 150f;
        private float remaining;

        public bool IsRunning { get; private set; }
        public float Remaining => remaining;
        public float Duration => duration;
        public TMP_Text Display => display;

        public void Configure(ManorGatePortal gate, ManorReturnRune rune, ManorGateTravelSystem travelSystem,
            ManorMoonCryptPuzzle cryptPuzzle, FiveChamberQuestController questController, TMP_Text timerDisplay, float seconds)
        {
            Unsubscribe();
            moonGate = gate; returnRune = rune; travel = travelSystem; puzzle = cryptPuzzle;
            quest = questController; display = timerDisplay; duration = Mathf.Max(10f, seconds);
            remaining = duration;
            if (isActiveAndEnabled) Subscribe();
            ShowReady();
        }

        private void OnEnable() { remaining = duration; Subscribe(); ShowReady(); }
        private void OnDisable() => Unsubscribe();
        private void Subscribe()
        {
            if (moonGate != null) { moonGate.ActivationRequested -= StartForGate; moonGate.ActivationRequested += StartForGate; }
            if (returnRune != null) { returnRune.ReturnRequested -= StopForReturn; returnRune.ReturnRequested += StopForReturn; }
            if (quest != null) { quest.QuestReset -= ResetTimer; quest.QuestReset += ResetTimer; }
        }
        private void Unsubscribe()
        {
            if (moonGate != null) moonGate.ActivationRequested -= StartForGate;
            if (returnRune != null) returnRune.ReturnRequested -= StopForReturn;
            if (quest != null) quest.QuestReset -= ResetTimer;
        }

        private void Update()
        {
            if (!IsRunning) return;
            if (puzzle != null && puzzle.IsSolved) { StopSolved(); return; }
            remaining -= Time.deltaTime;
            if (remaining <= 0f) { Expire(); return; }
            ShowTime();
        }

        private void StartForGate(ManorGatePortal gate)
        {
            if (gate != moonGate) return;
            remaining = duration; IsRunning = true; ShowTime();
        }
        private void StopForReturn(ManorReturnRune rune)
        {
            if (rune != returnRune) return;
            IsRunning = false; ShowReady();
        }
        private void StopSolved()
        {
            IsRunning = false;
            if (display != null) { display.text = "MOONSTONE REVEALED"; display.color = new Color(0.45f, 1f, 0.62f); }
        }

        private void Expire()
        {
            IsRunning = false; remaining = 0f;
            puzzle?.ResetPuzzle();
            if (travel != null && moonGate != null) travel.TravelTo(moonGate.ReturnAnchor);
            if (display != null) { display.text = "THE MOON HAS SET\nRETURNING TO THE HALL"; display.color = new Color(1f, 0.18f, 0.16f); }
            StartCoroutine(ReadyAfterLoss());
        }

        private IEnumerator ReadyAfterLoss()
        {
            yield return new WaitForSeconds(2f);
            remaining = duration; ShowReady();
        }

        public void ExpireForTest() => Expire();
        public void StartForTest() { remaining = duration; IsRunning = true; ShowTime(); }
        public void ResetTimer()
        {
            StopAllCoroutines(); IsRunning = false; remaining = duration; ShowReady();
        }

        private void ShowTime()
        {
            if (display == null) return;
            int seconds = Mathf.CeilToInt(remaining);
            display.text = $"MOON ABOVE  {seconds / 60:00}:{seconds % 60:00}";
            display.color = seconds <= 30 ? new Color(1f, 0.28f, 0.18f) : new Color(0.62f, 0.88f, 1f);
        }
        private void ShowReady()
        {
            if (display != null) { display.text = "MOON TRIAL  02:30"; display.color = new Color(0.62f, 0.88f, 1f); }
        }
    }
}
