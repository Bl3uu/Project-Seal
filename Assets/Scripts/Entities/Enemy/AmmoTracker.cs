using System;
using UnityEngine;

public class AmmoTracker : MonoBehaviour
{
    [SerializeField] private float reloadDuration = 3.5f;

    public bool HasBullet { get; private set; } = true;
    public bool IsReloading { get; private set; } = false;

    public event Action OnReloadStarted;
    public event Action OnReloadingFinished;

    private float reloadTimer;

    private void Update()
    {
        if (!IsReloading)
        {
            return;
        }

        reloadTimer -= Time.deltaTime;
        if (reloadTimer <= 0f)
        {
            IsReloading = false;
            HasBullet = true;

            if (OnReloadingFinished != null)
            {
                OnReloadingFinished();
            }
        }
    }

    public void ConsumeBullet()
    {
        HasBullet = false;
        StartReload();
    }

    public void StartReload()
    {
        if (IsReloading)
        {
            return;
        }

        IsReloading = true;
        reloadTimer = reloadDuration;
        if (OnReloadStarted != null)
        {
            OnReloadStarted();
        }
    }
}
