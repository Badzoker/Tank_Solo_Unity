using System;
using System.Threading;
using UnityEngine;

public class TankBarrel : MonoBehaviour
{
    ITankControl control;
    public event Action<Vector3> OnFired;
    [SerializeField] private Transform firePoint;

    [Header("Test")]
    [SerializeField] private Projectile bulletPrefab;
    [SerializeField] private Projectile missilePrefab;
    [SerializeField] private int team;

    private bool useMissile;

    void Awake()
    {
        control = GetComponent<ITankControl>();
    }

    private void LateUpdate()
    {
        if (control == null) return;

        if (control.isFire)
        {
            FireNow();
        }
    }

    public void Fire(Projectile prefab, Vector3 targetPos, int team)
    {
        var p = ProjectilePool.Instance.Spawn(prefab, firePoint.position, firePoint.rotation);
        p.Launch(firePoint.position, targetPos, team);
    }

    private void FireNow()
    {
        Vector3 fireDir = control.AimDir;
        if (fireDir.sqrMagnitude < 0.0001f)
            fireDir = transform.forward;

        // 테스트용 타겟 포지션(원래는 조준점/타겟 Transform)
        Vector3 targetPos = firePoint.position + fireDir.normalized * 1000f;

        //Projectile prefab = useMissile ? missilePrefab : bulletPrefab;
        Projectile prefab = missilePrefab;
        Fire(prefab, targetPos, team);

        //useMissile = !useMissile; // 좌/우 번갈아 같은 테스트

        Vector3 dir = (targetPos - firePoint.position).normalized;
        OnFired?.Invoke(dir);
    }
}
