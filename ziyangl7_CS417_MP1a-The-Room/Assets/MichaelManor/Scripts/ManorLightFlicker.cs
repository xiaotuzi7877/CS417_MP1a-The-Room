using UnityEngine;

namespace MichaelManor
{
    /// <summary>
    /// Adds subtle, deterministic candlelight variation without changing scene state.
    /// </summary>
    [RequireComponent(typeof(Light))]
    public sealed class ManorLightFlicker : MonoBehaviour
    {
        [SerializeField] private float variation = 90f;
        [SerializeField] private float speed = 2.4f;

        private Light targetLight;
        private float baseIntensity;
        private float noiseOffset;

        private void Awake()
        {
            targetLight = GetComponent<Light>();
            baseIntensity = targetLight.intensity;
            noiseOffset = transform.position.sqrMagnitude * 0.173f;
        }

        private void Update()
        {
            float noise = Mathf.PerlinNoise(noiseOffset, Time.time * speed) - 0.5f;
            targetLight.intensity = Mathf.Max(0f, baseIntensity + noise * variation);
        }
    }
}
