using System.Collections;
using UnityEngine;

public class HitFeedback : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float flashDuration = 0.5f;
    [SerializeField] private Material flashMaterial;


    [Header("Audio")]
    [SerializeField] private EntityAudioDriver audioDriver;
    [SerializeField] private AudioClip defaultHitSFX;

    private Material originalMaterial;
    private Coroutine flashCoroutine;

    public void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioDriver = GetComponent<EntityAudioDriver>();
        originalMaterial = spriteRenderer.material;
    }

    private IEnumerator FlashRoutine()
    {
        spriteRenderer.material = flashMaterial;

        yield return new WaitForSeconds(flashDuration);

        spriteRenderer.material = originalMaterial;
    }

    public void PlayHitEffects(AudioClip customImpactSFX = null)
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = StartCoroutine(FlashRoutine());

        AudioClip clipToPlay;

        if (customImpactSFX != null) 
        {
            clipToPlay = customImpactSFX;
        }
        else
        {
            clipToPlay = defaultHitSFX;
        }

        if (audioDriver != null && clipToPlay != null)
        {
            audioDriver.PlaySound(clipToPlay);
        }
    }
}
