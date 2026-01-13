using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTankControl : MonoBehaviour, ITankControl
{
    public Vector2 Move { get; private set; }
    public Vector3 AimDir { get; private set; }
    public bool isAim { get; private set; }
    public bool isFire { get; private set; }

    private Player input;

    private Transform MainCamera;

    private void Awake()
    {
        input = new Player();

        if(!MainCamera && Camera.main)
        {
            MainCamera = Camera.main.transform;
        }
    }

    void OnEnable() => input.MyPlayerInput.Enable();
    void OnDisable() => input.MyPlayerInput.Disable();

    private void Update()
    {
        Move = input.MyPlayerInput.Move.ReadValue<Vector2>();

        isFire = input.MyPlayerInput.Fire.WasPressedThisFrame();

        if (Mouse.current != null && Mouse.current.rightButton.isPressed)
        {
            isAim = false;
            return;
        }

        Vector3 mouseDir = MainCamera.forward;
        mouseDir.y = 0;

        if(mouseDir.sqrMagnitude < 0.0001f)
        {
            isAim = false;
            return;
        }

        AimDir = mouseDir.normalized;
        isAim = true;
    }

}
