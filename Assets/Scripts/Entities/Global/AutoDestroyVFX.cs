using UnityEngine;

public class AutoDestroyVFX : MonoBehaviour
{
    [SerializeField] private float fallbackLifetime = 0.3f;

    private void Start()
    {
        float lifetime = this.GetAnimationLengthOrDefault(fallbackLifetime);
        Destroy(gameObject, lifetime);
    }
}
