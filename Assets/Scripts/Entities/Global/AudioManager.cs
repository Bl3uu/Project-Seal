using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set;  }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        if (sfxSource == null)
        {
            sfxSource = GetComponent<AudioSource>();
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }

    public void PlaySFX(AudioClip clip, float volume = 1.0f, bool randomizePitch = true)
    {
        if (clip == null || sfxSource == null)
        {
            return;
        }
        
        if (randomizePitch)
        {
            sfxSource.pitch = Random.Range(0.95f, 1.05f);
        }
        else
        {
            sfxSource.pitch = 1.0f;
        }
    }

    public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volume = 1.0f)
    {
        if (clip == null)
        {
            return;
        }

        AudioSource.PlayClipAtPoint(clip, position, volume);
    }
}
