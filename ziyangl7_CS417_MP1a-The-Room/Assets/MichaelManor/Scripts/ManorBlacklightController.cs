using TMPro;
using UnityEngine;

namespace MichaelManor
{
    public sealed class ManorBlacklightController : MonoBehaviour
    {
        [SerializeField] private Transform beamOrigin;
        [SerializeField] private Light ultravioletLight;
        [SerializeField] private TMP_Text hiddenMessage;
        [SerializeField] private float range = 7f;
        public Transform BeamOrigin => beamOrigin;
        public TMP_Text HiddenMessage => hiddenMessage;
        public void Configure(Transform origin, Light light, TMP_Text message, float beamRange)
        { beamOrigin=origin;ultravioletLight=light;hiddenMessage=message;range=beamRange;ApplyGlobals(); }
        private void LateUpdate() => ApplyGlobals();
        private void ApplyGlobals()
        {
            if (beamOrigin == null) return;
            Shader.SetGlobalVector("_BlacklightPosition", beamOrigin.position);
            Shader.SetGlobalVector("_BlacklightDirection", beamOrigin.forward);
            Shader.SetGlobalFloat("_BlacklightRange", range);
        }
        public float VisibilityAt(Vector3 worldPoint)
        {
            if (beamOrigin == null) return 0f;
            Vector3 delta=worldPoint-beamOrigin.position; float cone=Vector3.Dot(delta.normalized,beamOrigin.forward);
            return Mathf.SmoothStep(0f,1f,Mathf.InverseLerp(0.90f,1f,cone))*Mathf.Clamp01(1f-delta.magnitude/range);
        }
    }
}
