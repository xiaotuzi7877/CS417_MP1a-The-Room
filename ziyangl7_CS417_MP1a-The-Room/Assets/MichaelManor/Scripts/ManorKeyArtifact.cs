using UnityEngine;

namespace MichaelManor
{
    /// <summary>
    /// Identifies a grabbable object as a key artifact for the manor puzzle.
    /// </summary>
    public sealed class ManorKeyArtifact : MonoBehaviour
    {
        [SerializeField] private string artifactId = "SilverFang";

        public string ArtifactId => artifactId;

        public void Configure(string id)
        {
            artifactId = id;
        }
    }
}
