using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance { get; private set; }

    [System.Serializable]
    public class Prewarm
    {
        public Projectile prefab;
        public int count = 20;
    }

    [Header("Prewarm (Optional)")]
    [SerializeField] private List<Prewarm> prewarms = new();

    [Header("Hierarchy")]
    [SerializeField] private Transform poolRoot;

    // prefab -> stack
    private readonly Dictionary<Projectile, Stack<Projectile>> pools = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (poolRoot == null)
        {
            var go = new GameObject("ProjectilePoolRoot");
            poolRoot = go.transform;
            poolRoot.SetParent(transform);
        }

        // optional prewarm
        foreach (var p in prewarms)
        {
            if (p.prefab == null || p.count <= 0) continue;
            Warm(p.prefab, p.count);
        }
    }

    public void Warm(Projectile prefab, int count)
    {
        if (!pools.TryGetValue(prefab, out var stack))
        {
            stack = new Stack<Projectile>(count);
            pools[prefab] = stack;
        }

        for (int i = 0; i < count; i++)
        {
            var inst = CreateInstance(prefab);
            DespawnInternal(prefab, inst);
        }
    }

    public Projectile Spawn(Projectile prefab, Vector3 pos, Quaternion rot)
    {
        if (prefab == null) return null;

        if (!pools.TryGetValue(prefab, out var stack))
        {
            stack = new Stack<Projectile>();
            pools[prefab] = stack;
        }

        Projectile inst = (stack.Count > 0) ? stack.Pop() : CreateInstance(prefab);

        inst.transform.SetPositionAndRotation(pos, rot);
        inst.gameObject.SetActive(true);
        inst.OnSpawn();

        return inst;
    }

    public void Despawn(Projectile inst)
    {
        if (inst == null) return;

        // 어떤 prefab 풀로 돌아가야 하는지 알아야 함.
        // 가장 간단한 방법: inst가 자신의 원본 prefab 참조를 기억하게 만들기
        // -> 아래 3)에서 Projectile에 originPrefab 필드 추가
        var prefab = inst.OriginPrefab;
        if (prefab == null)
        {
            // 안전장치: 그냥 비활성화
            inst.gameObject.SetActive(false);
            inst.transform.SetParent(poolRoot);
            return;
        }

        DespawnInternal(prefab, inst);
    }

    private void DespawnInternal(Projectile prefab, Projectile inst)
    {
        inst.OnDespawn();
        inst.gameObject.SetActive(false);
        inst.transform.SetParent(poolRoot);

        pools[prefab].Push(inst);
    }

    private Projectile CreateInstance(Projectile prefab)
    {
        var inst = Instantiate(prefab, poolRoot);
        inst.SetReturnToPoolAction(Despawn);
        inst.SetOriginPrefab(prefab); // 아래 3)에서 구현
        inst.gameObject.SetActive(false);
        return inst;
    }
}
