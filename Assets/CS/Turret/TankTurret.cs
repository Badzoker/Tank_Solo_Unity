using UnityEngine;
using UnityEngine.InputSystem;

public class TankTurret : MonoBehaviour
{
    public Transform turretYaw;   // TurretLogic
    public float yawSpeedDeg = 120f;
    private ITankControl controller;
    void Awake()
    {
        if (!turretYaw) turretYaw = transform;
        if(controller == null)
        {
            controller = GetComponentInParent<ITankControl>();
            if (controller == null)
            {
                Debug.LogError("ITankControl 컴포넌트가 없습니다!");
            }
        }
    }

    void LateUpdate()
    {

        if (controller == null || !controller.isAim)
            return;

        Vector3 TurretDir = controller.AimDir;
        TurretDir.y = 0f;
        if (TurretDir.sqrMagnitude < 0.0001f)
            return;
        TurretDir.Normalize();

        Transform parent = turretYaw.parent;

        Vector3 dirW = parent ? parent.InverseTransformDirection(TurretDir) : TurretDir;
        float targetYaw = Mathf.Atan2(dirW.x, dirW.z) * Mathf.Rad2Deg;
        float currentYaw = NormalizeAngle(turretYaw.localEulerAngles.y);
        float nextYaw = Mathf.MoveTowardsAngle(
                        currentYaw,
                        targetYaw,
                        yawSpeedDeg * Time.deltaTime);

        Vector3 Result = turretYaw.localEulerAngles;
        turretYaw.localEulerAngles = new Vector3(Result.x, nextYaw, Result.z);
    }

    static float NormalizeAngle(float a)
    {
        a %= 360f;
        if (a > 180f) a -= 360f;
        return a;
    }
}
