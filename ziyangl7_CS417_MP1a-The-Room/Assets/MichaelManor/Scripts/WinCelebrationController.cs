using UnityEngine;

namespace MichaelManor
{
    /// <summary>
    /// Reusable final-game celebration. Minh's scene connector can call TriggerWin().
    /// </summary>
    public sealed class WinCelebrationController : MonoBehaviour
    {
        [SerializeField] private GameObject celebrationRoot;
        [SerializeField] private ParticleSystem[] particles;
        [SerializeField] private AudioSource victoryAudio;

        private bool hasWon;

        public bool HasWon => hasWon;

        public void Configure(
            GameObject root,
            ParticleSystem[] celebrationParticles,
            AudioSource audioSource = null)
        {
            celebrationRoot = root;
            particles = celebrationParticles;
            victoryAudio = audioSource;
        }

        private void Awake()
        {
            if (celebrationRoot != null)
            {
                celebrationRoot.SetActive(false);
            }
        }

        [ContextMenu("Trigger Win Celebration")]
        public void TriggerWin()
        {
            if (hasWon)
            {
                return;
            }

            hasWon = true;

            if (celebrationRoot != null)
            {
                celebrationRoot.SetActive(true);
            }

            if (particles != null)
            {
                foreach (ParticleSystem particle in particles)
                {
                    if (particle != null)
                    {
                        particle.Play(true);
                    }
                }
            }

            if (victoryAudio != null)
            {
                victoryAudio.Play();
            }

            Debug.Log("Win Celebration triggered.");
        }

        [ContextMenu("Reset Win Celebration")]
        public void ResetCelebration()
        {
            hasWon = false;

            if (particles != null)
            {
                foreach (ParticleSystem particle in particles)
                {
                    if (particle != null)
                    {
                        particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    }
                }
            }

            if (celebrationRoot != null)
            {
                celebrationRoot.SetActive(false);
            }
        }
    }
}
