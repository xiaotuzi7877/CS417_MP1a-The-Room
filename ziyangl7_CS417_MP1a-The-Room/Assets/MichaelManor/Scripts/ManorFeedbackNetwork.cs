using UnityEngine;

namespace MichaelManor
{
    /// <summary>
    /// Distributes particle and spatial-audio feedback around the hall for rubric demonstrations.
    /// Four distinct controller actions each activate a separate bank of four emitters.
    /// </summary>
    public sealed class ManorFeedbackNetwork : MonoBehaviour
    {
        [SerializeField] private ParticleSystem[] spawnParticles;
        [SerializeField] private AudioSource[] spawnAudio;
        [SerializeField] private ParticleSystem[] lightParticles;
        [SerializeField] private AudioSource[] lightAudio;
        [SerializeField] private ParticleSystem[] breakOutParticles;
        [SerializeField] private AudioSource[] breakOutAudio;
        [SerializeField] private ParticleSystem[] returnParticles;
        [SerializeField] private AudioSource[] returnAudio;

        public void Configure(
            ParticleSystem[] spawnParticleBank,
            AudioSource[] spawnAudioBank,
            ParticleSystem[] lightParticleBank,
            AudioSource[] lightAudioBank,
            ParticleSystem[] breakOutParticleBank,
            AudioSource[] breakOutAudioBank,
            ParticleSystem[] returnParticleBank,
            AudioSource[] returnAudioBank)
        {
            spawnParticles = spawnParticleBank;
            spawnAudio = spawnAudioBank;
            lightParticles = lightParticleBank;
            lightAudio = lightAudioBank;
            breakOutParticles = breakOutParticleBank;
            breakOutAudio = breakOutAudioBank;
            returnParticles = returnParticleBank;
            returnAudio = returnAudioBank;
        }

        public void PlaySpawnFeedback()
        {
            PlayBank(spawnParticles, spawnAudio);
        }

        public void PlayLightFeedback()
        {
            PlayBank(lightParticles, lightAudio);
        }

        public void PlayBreakOutFeedback()
        {
            PlayBank(breakOutParticles, breakOutAudio);
        }

        public void PlayReturnFeedback()
        {
            PlayBank(returnParticles, returnAudio);
        }

        private static void PlayBank(ParticleSystem[] particles, AudioSource[] audioSources)
        {
            if (particles != null)
            {
                foreach (ParticleSystem particle in particles)
                {
                    if (particle != null)
                    {
                        particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                        particle.Play(true);
                    }
                }
            }

            if (audioSources != null)
            {
                foreach (AudioSource audioSource in audioSources)
                {
                    if (audioSource != null)
                    {
                        audioSource.Play();
                    }
                }
            }
        }
    }
}
