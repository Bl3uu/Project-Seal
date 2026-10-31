using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class AttackTelegraph : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer indicatorRenderer;
    [SerializeField] private Transform weaponPivot;

    [Header("Raycast Settings")]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float rayLineWidth = 0.5f;

    private Coroutine telegraphAnimationRoutine;

    private void Awake()
    {
        if (indicatorRenderer == null)
        {
            indicatorRenderer = GetComponent<SpriteRenderer>();
        }
        
        HideTelegraph();
    }

    public void ShowTelegraph(Vector2 direction, AttackData attackData)
    {
        Vector2 enemyPosition;
        float offsetRotation;

        if (indicatorRenderer == null)
        {
            return;
        }

        if (attackData.telegraphSprites == null || attackData.telegraphSprites.Length == 0)
        {
            return;
        }

        gameObject.SetActive(true);

        if (telegraphAnimationRoutine != null)
        {
            StopCoroutine(telegraphAnimationRoutine);
        }

        telegraphAnimationRoutine = StartCoroutine(PlayTelegraphFramesRoutine(attackData));

        if (attackData.isRangedRay)
        {
            offsetRotation = -90f;
        }
        else
        {
            offsetRotation = -135f;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle + offsetRotation);

        if (weaponPivot != null)
        {
            enemyPosition = weaponPivot.position;
        }
        else
        {
            enemyPosition = (Vector2)transform.position;
        }

        if (attackData.isRangedRay)
        {
            float rayLength = attackData.attackDistance;
            RaycastHit2D hit = Physics2D.Raycast(enemyPosition, direction.normalized, attackData.attackDistance, obstacleMask);
            
            if (hit.collider != null)
            {
                Debug.Log($"Ray hit: {hit.collider.name} at distance {hit.distance}");
                rayLength = hit.distance;
            }
            else
            {
                Debug.Log("Ray hit nothing, using full attack distance.");
            }

            transform.localScale = new Vector3(rayLineWidth, rayLength, 1f);
            transform.position = enemyPosition + (direction.normalized * (rayLength * 0.5f));
        }
        else
        {
            transform.localScale = Vector3.one * 1.2f;
            float iconOffset = 0.8f;
            transform.position = enemyPosition + (direction.normalized * iconOffset);
        }
    }

    private IEnumerator PlayTelegraphFramesRoutine(AttackData attackData)
    {
        Sprite[] sprites = attackData.telegraphSprites;
        float duration = attackData.telegraphDuration;

        if (sprites == null || sprites.Length == 0 || duration <= 0f)
        {
            yield break;
        }

        float timePerSprite = duration / sprites.Length;

        foreach (Sprite frameSprite in sprites)
        {
            if (indicatorRenderer != null && frameSprite != null)
            {
                indicatorRenderer.sprite = frameSprite;
            }

            yield return new WaitForSeconds(timePerSprite);
        }
    }

    public void HideTelegraph()
    {
        if (telegraphAnimationRoutine != null)
        {
            StopCoroutine(telegraphAnimationRoutine);
            telegraphAnimationRoutine = null;
        }
        gameObject.SetActive(false);
    }
}
