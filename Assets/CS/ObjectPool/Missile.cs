using UnityEngine;

public class Missile : Projectile
{
    Rigidbody rb;
    public float speed = 10f;

    protected override void Awake()
    {
        base.Awake();
        bIsPhysics = true;
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    protected override void OnLaunched()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    protected override void PhysicsTick(float fdt)
    {
        rb.linearVelocity = AimDir * speed; // 예시 (유도면 AimDir 갱신)
    }
}
