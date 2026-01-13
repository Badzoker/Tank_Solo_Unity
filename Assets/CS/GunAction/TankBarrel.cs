using UnityEngine;

public class TankBarrel : MonoBehaviour
{
    public Fire_ReAction recoil;

    ITankControl control;

    void Awake()
    {
        control = GetComponent<ITankControl>();
    }

    void Update()
    {
        if (control == null) return;

        if (control.isFire)   // SpaceBar 누르면 true (WasPressedThisFrame이면 1프레임)
        {
            if (recoil) recoil.PlayReAction();
        }
    }
}
