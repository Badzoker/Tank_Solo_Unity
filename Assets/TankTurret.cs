using UnityEngine;
using UnityEngine.InputSystem;

public class TankTurret : MonoBehaviour
{
    public Transform turretYaw;   // TurretLogic
    public Transform cam;         // Main Camera
    public float yawSpeedDeg = 120f;
    Player input;
    void Awake()
    {
        if (!cam && Camera.main) cam = Camera.main.transform;
        if (!turretYaw) turretYaw = transform;

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

    void LateUpdate()
    {

        if (Mouse.current != null && Mouse.current.rightButton.isPressed)
            return;

        Vector3 dirW = cam.forward;
        dirW.y = 0f;
        if (dirW.sqrMagnitude < 0.0001f) return;
        dirW.Normalize();

        Transform parent = turretYaw.parent;
        Vector3 dirL = parent ? parent.InverseTransformDirection(dirW) : dirW;

        float targetYaw = Mathf.Atan2(dirL.x, dirL.z) * Mathf.Rad2Deg;
        float curYaw = NormalizeAngle(turretYaw.localEulerAngles.y);

        float nextYaw = Mathf.MoveTowardsAngle(curYaw, targetYaw, yawSpeedDeg * Time.deltaTime);

        Vector3 e = turretYaw.localEulerAngles;
        turretYaw.localEulerAngles = new Vector3(e.x, nextYaw, e.z);
    }

    static float NormalizeAngle(float a)
    {
        a %= 360f;
        if (a > 180f) a -= 360f;
        return a;
    }
}
