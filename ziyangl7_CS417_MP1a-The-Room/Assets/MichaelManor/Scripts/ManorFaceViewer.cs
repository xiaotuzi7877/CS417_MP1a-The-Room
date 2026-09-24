using UnityEngine;

namespace MichaelManor
{
    /// Turns a free-standing label about the vertical axis so its readable side always faces the
    /// player's head. TextMeshPro draws both faces, so without this a floating label (or one on a
    /// prop the player picks up and turns) reads mirrored from half of all directions.
    [DisallowMultipleComponent]
    public sealed class ManorFaceViewer : MonoBehaviour
    {
        private void LateUpdate()
        {
            Camera viewer = Camera.main;
            if (viewer == null) return;
            // TMP text reads correctly when the camera looks along the text's +Z.
            Vector3 away = transform.position - viewer.transform.position;
            away.y = 0f;
            if (away.sqrMagnitude < 0.0001f) return;
            transform.rotation = Quaternion.LookRotation(away, Vector3.up);
        }
    }
}
