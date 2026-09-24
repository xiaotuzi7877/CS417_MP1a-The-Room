using TMPro;
using UnityEngine;

namespace MichaelManor
{
    public sealed class ManorAngleRevealMarker : MonoBehaviour
    {
        [SerializeField] private TMP_Text hiddenWriting;
        public TMP_Text HiddenWriting => hiddenWriting;
        public void Configure(TMP_Text text) => hiddenWriting = text;
        public float VisibilityFrom(Vector3 viewerPosition)
        {
            Vector3 view = (viewerPosition - transform.position).normalized;
            float oblique = 1f - Mathf.Abs(Vector3.Dot(transform.forward, view));
            return Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.18f, 0.58f, oblique));
        }
    }
}
