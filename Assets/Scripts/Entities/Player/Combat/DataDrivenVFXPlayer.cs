using System.Collections;
using UnityEngine;

[RequireComponent (typeof(SpriteRenderer))]
[RequireComponent (typeof(MeleeHitbox))]
public class DataDrivenVFXPlayer : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private MeleeHitbox meleeHitbox;
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
    }

    public void InitializeAndPlay(AttackData attackData, DamageData damageData)
    {
        if (attackData == null)
        {
            return;
        }

        if (meleeHitbox != null)
        {
            meleeHitbox.PrepareHitbox(damageData);
            meleeHitbox.DisableHitbox();
        }

        StartCoroutine(PlayerSequenceRoutine(attackData));
    }

    private IEnumerator PlayerSequenceRoutine(AttackData attackData)
    {
        // Startup / Wind-up
        if (attackData.startupSprites != null && attackData.startupSprites.Length > 0)
        {
            yield return PlaySpriteArray(attackData.startupSprites, attackData.startupDuration);
        }
        else
        {
            yield return new WaitForSeconds(attackData.startupDuration);
        }

        // Active Hit
        if (meleeHitbox != null)
        {
            meleeHitbox.EnableHitbox();
        }

        if (attackData.activeSprites != null && attackData.activeSprites.Length > 0)
        {
            yield return PlaySpriteArray(attackData.activeSprites, attackData.activeHitDuration);
        }
        else
        {
            yield return new WaitForSeconds(attackData.activeHitDuration);
        }

        if (meleeHitbox != null)
        {
            meleeHitbox.DisableHitbox();
        }

        // Recovery
        if (attackData.recoverySprites != null && attackData.recoverySprites.Length > 0)
        {
            yield return PlaySpriteArray(attackData.recoverySprites, attackData.recoveryDuration);
        }
        else
        {
            yield return new WaitForSeconds(attackData.recoveryDuration);
        }

        Destroy(gameObject);
    }

    private IEnumerator PlaySpriteArray(Sprite[] sprites, float phaseDuration)
    {
        if (sprites == null || sprites.Length == 0 || phaseDuration <= 0f)
        {
            yield break;
        }

        float timePerSprite = phaseDuration / sprites.Length;

        foreach (Sprite frameSprite in sprites)
        {
            if (spriteRenderer != null && frameSprite != null)
            {
                spriteRenderer.sprite = frameSprite;
            }
            yield return new WaitForSeconds(timePerSprite);
        }
    }
}