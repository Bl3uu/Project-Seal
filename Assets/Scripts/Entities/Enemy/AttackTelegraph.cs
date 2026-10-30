using System;
using Unity.Mathematics;
using UnityEngine;

public class AttackTelegraph : MonoBehaviour
{
    [SerializeField] private SpriteRenderer indicatorRenderer;
    [SerializeField] private float telegraphWidth = 1f;

    private void Awake()
    {
        if (indicatorRenderer == null)
        {
            indicatorRenderer = GetComponent<SpriteRenderer>();
        }
        
        HideTelegraph();
    }

    public void ShowTelegraph(Vector2 direction, float attackDistance)
    {
        if (indicatorRenderer == null)
        {
            return;
        }

        gameObject.SetActive(true);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        transform.localScale = new Vector3(attackDistance, telegraphWidth, 1f);
    }

    public void HideTelegraph()
    {
        gameObject.SetActive(false);
    }
}
