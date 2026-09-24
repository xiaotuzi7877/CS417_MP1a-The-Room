using UnityEngine;

namespace MichaelManor
{
    public sealed class ManorHandMirror : MonoBehaviour
    {
        [SerializeField] private Camera mirrorCamera;
        [SerializeField] private Renderer mirrorSurface;
        [SerializeField] private RenderTexture renderTexture;
        public Camera MirrorCamera => mirrorCamera;
        public Renderer MirrorSurface => mirrorSurface;
        public RenderTexture RenderTexture => renderTexture;
        public void Configure(Camera source, Renderer surface, RenderTexture texture)
        { mirrorCamera=source;mirrorSurface=surface;renderTexture=texture;if(mirrorCamera!=null)mirrorCamera.targetTexture=renderTexture; }
    }
}
