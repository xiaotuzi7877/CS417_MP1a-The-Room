using System;
using UnityEngine;
using UnityEngine.Events;

namespace MichaelManor
{
    /// <summary>
    /// Owns the shared state for the five gated locations. Travel and chamber-specific
    /// puzzles are deliberately supplied by later sections of the implementation plan.
    /// </summary>
    public sealed class FiveChamberQuestController : MonoBehaviour
    {
        public const int RequiredChamberCount = 5;

        [SerializeField] private ManorGatePortal[] gates = new ManorGatePortal[RequiredChamberCount];
        [SerializeField] private bool gatesUnlocked;
        [SerializeField] private bool[] completedChambers = new bool[RequiredChamberCount];
        [SerializeField] private UnityEvent onStateChanged = new UnityEvent();

        public event Action<bool> GateAvailabilityChanged;
        public event Action<int, int> ExplorationChanged;
        public event Action QuestReset;

        public bool GatesUnlocked => gatesUnlocked;
        public int ChamberCount => RequiredChamberCount;
        public int ExploredCount { get; private set; }
        public UnityEvent OnStateChanged => onStateChanged;

        public void Configure(ManorGatePortal[] configuredGates)
        {
            gates = configuredGates ?? new ManorGatePortal[RequiredChamberCount];
            EnsureStateArrays();
            ApplyStateToGates();
        }

        private void Awake()
        {
            EnsureStateArrays();
            RecalculateExploredCount();
            ApplyStateToGates();
        }

        public bool IsChamberComplete(int chamberIndex)
        {
            EnsureStateArrays();
            return IsValidChamberIndex(chamberIndex) && completedChambers[chamberIndex];
        }

        public void UnlockGates()
        {
            SetGatesUnlocked(true);
        }

        public void SetGatesUnlocked(bool unlocked)
        {
            if (gatesUnlocked == unlocked)
            {
                ApplyStateToGates();
                return;
            }

            gatesUnlocked = unlocked;
            ApplyStateToGates();
            GateAvailabilityChanged?.Invoke(gatesUnlocked);
            onStateChanged.Invoke();
        }

        public bool TryCompleteChamber(int chamberIndex)
        {
            EnsureStateArrays();
            if (!IsValidChamberIndex(chamberIndex) || completedChambers[chamberIndex])
            {
                return false;
            }

            completedChambers[chamberIndex] = true;
            RecalculateExploredCount();

            ManorGatePortal gate = GetGate(chamberIndex);
            if (gate != null)
            {
                gate.SetExplored(true);
            }

            ExplorationChanged?.Invoke(ExploredCount, RequiredChamberCount);
            onStateChanged.Invoke();
            return true;
        }

        [ContextMenu("Reset Five Chamber Quest")]
        public void ResetQuest()
        {
            EnsureStateArrays();
            for (int i = 0; i < completedChambers.Length; i++)
            {
                completedChambers[i] = false;
            }

            gatesUnlocked = false;
            ExploredCount = 0;
            ApplyStateToGates();
            GateAvailabilityChanged?.Invoke(false);
            ExplorationChanged?.Invoke(0, RequiredChamberCount);
            onStateChanged.Invoke();
            QuestReset?.Invoke();
        }

        private void EnsureStateArrays()
        {
            if (completedChambers == null || completedChambers.Length != RequiredChamberCount)
            {
                completedChambers = new bool[RequiredChamberCount];
            }

            if (gates == null || gates.Length != RequiredChamberCount)
            {
                ManorGatePortal[] resized = new ManorGatePortal[RequiredChamberCount];
                if (gates != null)
                {
                    Array.Copy(gates, resized, Mathf.Min(gates.Length, resized.Length));
                }

                gates = resized;
            }
        }

        private void RecalculateExploredCount()
        {
            ExploredCount = 0;
            for (int i = 0; i < completedChambers.Length; i++)
            {
                if (completedChambers[i])
                {
                    ExploredCount++;
                }
            }
        }

        private void ApplyStateToGates()
        {
            EnsureStateArrays();
            for (int i = 0; i < RequiredChamberCount; i++)
            {
                ManorGatePortal gate = GetGate(i);
                if (gate == null)
                {
                    continue;
                }

                gate.SetUnlocked(gatesUnlocked);
                gate.SetExplored(completedChambers[i]);
            }
        }

        private ManorGatePortal GetGate(int chamberIndex)
        {
            return gates != null && chamberIndex >= 0 && chamberIndex < gates.Length
                ? gates[chamberIndex]
                : null;
        }

        private static bool IsValidChamberIndex(int chamberIndex)
        {
            return chamberIndex >= 0 && chamberIndex < RequiredChamberCount;
        }
    }
}
