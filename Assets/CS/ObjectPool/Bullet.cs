using UnityEngine;

public class Bullet :Projectile
{
    public float fSpeed = 10f;

    protected override void Tick(float _fTimeDelta)
    {
        transform.position += AimDir * fSpeed * _fTimeDelta;
    }
}
