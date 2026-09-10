using UnityEngine;

public class DestroyAfterSound : MonoBehaviour
{
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource != null && audioSource.clip != null)
        {
            Destroy(gameObject, audioSource.clip.length + 0.1f);
        }
    }
}