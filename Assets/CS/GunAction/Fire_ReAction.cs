using System.Collections;
using UnityEngine;

public class Fire_ReAction : MonoBehaviour
{
    public Transform body;           // 기우뚱할 대상(차체 or BodyTiltPivot)
    public Transform turretRef;      // 기준이 될 포탑(보통 TurretYaw)
    public float kickAngle = 3.5f;
    public float kickTime = 0.05f;
    public float returnTime = 0.18f;

    Coroutine reactionCo;
    TankBarrel barrel;
    private void Awake()
    {
        barrel = GetComponent<TankBarrel>();
    }

    void OnEnable()
    {
        if (barrel != null)
        {
            Bind(barrel);
        }
    }

    private void OnDisable()
    {
        if (barrel != null)
        {
            unBind(barrel);
        }
    }


    public void Bind(TankBarrel tankBarrel)
    {
        tankBarrel.OnFired += (dir) => PlayReAction();
    }

    public void unBind(TankBarrel tankBarrel)
    {
        tankBarrel.OnFired -= (dir) => PlayReAction();
    }

    public void PlayReAction()
    {
        if (!isActiveAndEnabled) return;
        if (reactionCo != null) StopCoroutine(reactionCo);
        reactionCo = StartCoroutine(Kick());
    }

    IEnumerator Kick()
    {
        if (!body || !turretRef) yield break;

        Quaternion startRot = body.localRotation;

        // 포탑의 right 축(=포탑 forward 기준 pitch 회전축)을 사용
        Vector3 axisWorld = turretRef.right;

        // 그 월드 축을 body의 로컬 축으로 변환
        Vector3 axisLocal = body.parent
            ? body.parent.InverseTransformDirection(axisWorld)
            : axisWorld;

        Quaternion kick = Quaternion.AngleAxis(kickAngle, axisLocal.normalized);

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
