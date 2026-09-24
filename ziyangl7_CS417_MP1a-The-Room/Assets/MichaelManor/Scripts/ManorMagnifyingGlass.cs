using UnityEngine;

namespace MichaelManor
{
    public sealed class ManorMagnifyingGlass : MonoBehaviour
    {
        [SerializeField] private Camera lensCamera;
        [SerializeField] private Renderer lensSurface;
        [SerializeField] private RenderTexture renderTexture;
        public Camera LensCamera => lensCamera;
        public Renderer LensSurface => lensSurface;
        public RenderTexture RenderTexture => renderTexture;
        public void Configure(Camera camera, Renderer surface, RenderTexture texture)
        { lensCamera=camera;lensSurface=surface;renderTexture=texture;if(lensCamera!=null)lensCamera.targetTexture=renderTexture; }
    }
}
