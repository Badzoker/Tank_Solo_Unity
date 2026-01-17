using System.Collections;
using UnityEngine;

public class AIFire_ReAction : MonoBehaviour
{
    [Header("Bind")]
    [SerializeField] private TankBarrel barrel; // Enemy의 발사 로직(이벤트 발생자)

    [Header("Barrels (Kick 대상)")]
    [SerializeField] private Transform leftBarrel;
    [SerializeField] private Transform rightBarrel;

    [Header("Kick Settings")]
    [Tooltip("로컬 기준으로 뒤로 밀리는 거리(+면 forward 반대, 즉 -forward로 밀림)")]
    [SerializeField] private float kickDistance = 0.12f;
    [SerializeField] private float kickTime = 0.04f;
    [SerializeField] private float returnTime = 0.12f;

    private bool useLeftNext = true;

    // 각 배럴 코루틴 관리 (연속 발사 시 꼬임 방지)
    private Coroutine leftCo;
    private Coroutine rightCo;
    private Vector3 leftBaseLocalPos;
    private Vector3 rightBaseLocalPos;

    private void Awake()
    {
        // 인스펙터 할당 우선, 없으면 부모에서 자동 탐색
        if (barrel == null) barrel = GetComponent<TankBarrel>();
        CacheBase();

        if (leftBarrel != null) leftBaseLocalPos = leftBarrel.localPosition;
        if (rightBarrel != null) rightBaseLocalPos = rightBarrel.localPosition;
    }
    private void CacheBase()
    {
        if (leftBarrel != null) leftBaseLocalPos = leftBarrel.localPosition;
        if (rightBarrel != null) rightBaseLocalPos = rightBarrel.localPosition;
    }

    private void OnEnable()
    {
        CacheBase();

        if (barrel != null) barrel.OnFired += HandleFired;
    }

    private void OnDisable()
    {
        if (barrel != null) barrel.OnFired -= HandleFired;
    }

    private void HandleFired(Vector3 fireDir)
    {
        // 어느 배럴을 쓸지 결정 (번갈아)
        Transform t = useLeftNext ? leftBarrel : rightBarrel;
        useLeftNext = !useLeftNext;

        if (t == null) return;

        // 해당 배럴의 베이스 위치(로컬) 가져오기
        Vector3 basePos = (t == leftBarrel) ? leftBaseLocalPos : rightBaseLocalPos;

        // 이미 코루틴 돌고 있으면 끊고 베이스로 복귀 후 다시 시작 (연속 발사 대응)
        if (t == leftBarrel)
        {
            if (leftCo != null) StopCoroutine(leftCo);
            leftBarrel.localPosition = leftBaseLocalPos;
            leftCo = StartCoroutine(KickRoutine(leftBarrel, leftBaseLocalPos));
            Debug.Log($"[AI] {name} → 왼쪽!", this);
        }
        else
        {
            if (rightCo != null) StopCoroutine(rightCo);
            rightBarrel.localPosition = rightBaseLocalPos;
            rightCo = StartCoroutine(KickRoutine(rightBarrel, rightBaseLocalPos));
            Debug.Log($"[AI] {name} → 오른쪽!", this);
        }
    }

    private IEnumerator KickRoutine(Transform target, Vector3 baseLocalPos)
    {
        Vector3 kickLocalOffset = Vector3.back * kickDistance; // 로컬 -Z로 밀기(대부분의 총구가 +Z라면 뒤로는 -Z)

        Vector3 kickPos = baseLocalPos + kickLocalOffset;

        // Kick (빠르게)
        float t = 0f;
        while (t < kickTime)
        {
            t += Time.deltaTime;
            float a = (kickTime <= 0f) ? 1f : Mathf.Clamp01(t / kickTime);
            target.localPosition = Vector3.Lerp(baseLocalPos, kickPos, a);
            yield return null;
        }
        target.localPosition = kickPos;

        // Return (천천히)
        t = 0f;
        while (t < returnTime)
        {
            t += Time.deltaTime;
            float a = (returnTime <= 0f) ? 1f : Mathf.Clamp01(t / returnTime);
            target.localPosition = Vector3.Lerp(kickPos, baseLocalPos, a);
            yield return null;
        }
        target.localPosition = baseLocalPos;
    }
}
