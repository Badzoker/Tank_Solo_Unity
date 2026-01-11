using UnityEngine;
using UnityEngine.Windows;

public class Tank_Player : MonoBehaviour
{
    public float speed = 1f;

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
    void Update()
    {
        Vector2 move = input.MyPlayerInput.Move.ReadValue<Vector2>();

        #region 실제 움직임

        Moving_Forward(move.y);
        Moving_Turn(move.x * 15f);

        #endregion


    }


    void Moving_Forward(float _fValue) // 이건 바퀴만 움직이는중 이제 탱크 전체를 움직이도록 하면될듯
    {
        if (Mathf.Abs(_fValue) < 0.01f) return;

        transform.Translate(
            Vector3.forward * _fValue * speed * Time.deltaTime,
            Space.Self
        );
    }

    void Moving_Turn(float _fValue)
    {
        if (Mathf.Abs(_fValue) < 0.01f) return;

        transform.Rotate(
            Vector3.up * _fValue * speed * Time.deltaTime,
            Space.Self
        );
    }

}
