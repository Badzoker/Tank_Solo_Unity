using System.Collections;
using UnityEngine;

public class Fire_ReAction : MonoBehaviour
{
    public Transform body;          // 반동 줄 대상(포신/포탑/바디 등)
    public float kickAngle = 3.5f;  // 반동 각도(도)
    public float kickTime = 0.05f;  // 튕겨나가는 시간
    public float returnTime = 0.18f;// 복귀 시간

    Coroutine reactionCo;

    public void PlayReAction()
    {
        if (!isActiveAndEnabled) return;

        if (reactionCo != null)
            StopCoroutine(reactionCo);

        reactionCo = StartCoroutine(Kick());
    }

    IEnumerator Kick()
    {
        if (!body) yield break;

        Quaternion startRot = body.localRotation;
        Quaternion kick = Quaternion.AngleAxis(-kickAngle, Vector3.right);

        float t = 0f;
        while (t < kickTime)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / kickTime);
            body.localRotation = Quaternion.Slerp(startRot, startRot * kick, a);
            yield return null;
        }

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
        reactionCo = null;
    }
}
