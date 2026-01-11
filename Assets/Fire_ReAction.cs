using System.Collections;
using UnityEngine;

public class Fire_ReAction : MonoBehaviour
{
    public Transform body;          // 기울일 대상(차체/루트)
    public Transform muzzle;         // 포신 끝(방향 기준)
    public float kickAngle = 3.5f;   // 기울기 각도(도)
    public float kickTime = 0.05f;   // 튕기는 시간
    public float returnTime = 0.18f; // 복귀 시간

    Coroutine ReAction;
    Player input;
    private void Awake()
    {
        input = new Player();
    }
    private void OnEnable()
    {
        input.MyPlayerInput.Enable();
    }

    private void OnDisable()
    {
        input.MyPlayerInput.Disable();
    }

    private void Update()
    {
        if(input.MyPlayerInput.Fire.WasPressedThisFrame())
        {
            PlayReAction();
        }
    }

    public void PlayReAction()
    {
        if (ReAction != null)
            StopCoroutine(ReAction);
        ReAction = StartCoroutine(Kick());
    }

    IEnumerator Kick()
    {
        if (!body) yield break;

        Quaternion startRot = body.localRotation;

        // Unity에서 +X 회전은 보통 "고개 숙임" 느낌이 나서,
        // 반동(앞이 들림)은 -각도가 자연스러운 경우가 많음
        Quaternion kick = Quaternion.AngleAxis(-kickAngle, Vector3.right);

        // 1) 빠르게 킥
        float t = 0f;
        while (t < kickTime)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / kickTime);
            body.localRotation = Quaternion.Slerp(startRot, startRot * kick, a);
            yield return null;
        }

        // 2) 천천히 복귀
        t = 0f;
        Quaternion kicked = body.localRotation;
        while (t < returnTime)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / returnTime);
            float e = 1f - Mathf.Pow(1f - a, 3f);
            body.localRotation = Quaternion.Slerp(kicked, startRot, e);
            yield return null;
        }

        body.localRotation = startRot;
    }
}
