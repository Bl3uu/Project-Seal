using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EntityAudioDriver : MonoBehaviour
{
    [Header("References")]
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 0.3f;
    }

    public void PlaySound(AudioClip clip, float volume = 1.0f)
    {
        if (clip == null || audioSource == null)
        {
            return;
        }

        audioSource.pitch = Random.Range(0.95f, 1.05f);
        audioSource.PlayOneShot(clip, volume);
    }
}
