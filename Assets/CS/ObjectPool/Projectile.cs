using UnityEngine;

public abstract class Projectile : MonoBehaviour, IPoolable
{
    [Header("Common")]
    public float fDamage = 1f;
    public float fLifeTime = 3f;

    [Header("Tick Mode")]
    [SerializeField] protected bool bIsPhysics = false;

    protected float fCurLifeTime = 0f;
    protected Vector3 startPos;
    protected Vector3 targetPos;
    protected Vector3 AimDir;
    protected int iOwnTeam = 0;
    protected ImpactEffect impactEffect;

    public Projectile OriginPrefab { get; private set; }
    private System.Action<Projectile> returnToPool;
    public void SetReturnToPoolAction(System.Action<Projectile> action)
    {
        returnToPool = action;
    }

    public void SetOriginPrefab(Projectile _prefab)
    {
        OriginPrefab = _prefab;
    }

    protected virtual void Awake()
    {
        impactEffect = GetComponent<ImpactEffect>();
    }
    protected virtual void PlayEffect(Vector3 _vPos, Vector3 _vDir)
    {
        if (impactEffect != null)
        {
            impactEffect.Play(_vPos, _vDir);
        }
    }
    public virtual void Launch(Vector3 start, Vector3 target, int ownTeam)
    {
        startPos = start;
        targetPos = target;
        AimDir = (targetPos - startPos).normalized;
        iOwnTeam = ownTeam;
        fCurLifeTime = 0f;
        transform.position = startPos;
        transform.forward = AimDir;
        OnLaunched();
    }

    protected virtual void OnLaunched() {}

    protected virtual void Update()
    {
        if (bIsPhysics)
            return;

        fCurLifeTime += Time.deltaTime;
        if (fCurLifeTime > fLifeTime)
        {
            ReturnToPool();
            return;
        }

        Tick(Time.deltaTime);
    }

    protected virtual void FixedUpdate()
    {
        if (!bIsPhysics)
            return;
        fCurLifeTime += Time.fixedDeltaTime;
        if (fCurLifeTime > fLifeTime)
        {
            ReturnToPool();
            return;
        }
        PhysicsTick(Time.fixedDeltaTime);
    }

    protected virtual void Tick(float _fTimeDelta) { }
    protected virtual void PhysicsTick(float _fTimeDelta) { }
    protected void ReturnToPool()
    {
        returnToPool?.Invoke(this);
    }

    #region ObjectPooling
    public void OnSpawn()
    {
    }
    public void OnDespawn()
    {
    }

    #endregion


    protected virtual void OnTriggerEnter(Collider other)
    {
        // Handle collision logic here (e.g., apply damage)
        ReturnToPool();
    }

}
