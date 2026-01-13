using UnityEngine;

public class TankMotor : MonoBehaviour
{
    public float Movespeed = 1f;
    public float Turnspeed = 1f;

    private ITankControl controller;

    private void Awake()
    {
        controller = GetComponent<ITankControl>();
        if (controller == null)
        {
            Debug.LogError("ITankControl 컴포넌트가 없습니다!");
        }
    }

    void Update()
    {
        if (controller == null)
            return;

        Vector2 move = controller.Move;

        #region 실제 움직임

        Moving_Forward(move.y);
        Moving_Turn(move.x * 15f);

        #endregion
    }


    void Moving_Forward(float _fValue) // 이건 바퀴만 움직이는중 이제 탱크 전체를 움직이도록 하면될듯
    {
        if (Mathf.Abs(_fValue) < 0.01f) return;

        transform.Translate(
            Vector3.forward * _fValue * Movespeed * Time.deltaTime,
            Space.Self
        );
    }

    void Moving_Turn(float _fValue)
    {
        if (Mathf.Abs(_fValue) < 0.01f) return;

        transform.Rotate(
            Vector3.up * _fValue * Turnspeed * Time.deltaTime,
            Space.Self
        );
    }
}
