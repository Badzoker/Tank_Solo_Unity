using UnityEngine;

public class ImpactEffect : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject impactPrefab;

    [Header("Lifetime")]
    [SerializeField] private float lifeTime = 2f;

    [Header("Placement")]
    [SerializeField] private float surfaceOffset = 0.01f; // z-fighting 방지용
    [SerializeField] private bool parentToHitObject = false;

    [Header("Randomize (Optional)")]
    [SerializeField] private bool randomYaw = true;       // 노말 기준으로 빙글빙글
    [SerializeField] private Vector2 randomScale = new Vector2(1f, 1f); // (min,max)

    /// <summary>
    /// 충돌 지점/노말로 임팩트 이펙트를 재생합니다.
    /// </summary>
    public void Play(Vector3 point, Vector3 normal, Transform hitTransform = null)
    {
        if (impactPrefab == null) return;

        // 표면에서 살짝 띄워서 겹침/깜빡임 방지
        Vector3 pos = point + normal * surfaceOffset;

        // 노말을 up으로 맞춰서 표면에 "붙는" 회전
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, normal);

        // 노말 축 기준 랜덤 회전(같은 이펙트 반복 티 덜 남)
        if (randomYaw)
        {
            rot = Quaternion.AngleAxis(Random.Range(0f, 360f), normal) * rot;
        }

        Transform parent = (parentToHitObject && hitTransform != null) ? hitTransform : null;
        GameObject fx = Instantiate(impactPrefab, pos, rot, parent);

        // 랜덤 스케일 (원하면 끄면 됨)
        if (randomScale.x != 1f || randomScale.y != 1f)
        {
            float s = Random.Range(randomScale.x, randomScale.y);
            fx.transform.localScale *= s;
        }

        // 자동 삭제
        if (lifeTime > 0f)
            Destroy(fx, lifeTime);
    }

    /// <summary>
    /// RaycastHit로 바로 재생하는 오버로드
    /// </summary>
    public void Play(RaycastHit hit)
    {
        Play(hit.point, hit.normal, hit.transform);
    }
}
