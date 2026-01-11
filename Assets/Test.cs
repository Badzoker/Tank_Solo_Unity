using System;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class Test : MonoBehaviour
{
    public Renderer rend;
    public float speed = 1f;
    Vector2 uv;
    public bool isLeft = false;
    

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


        # region 궤도 움직임 모양
        float forward = move.y; // W/S
        float turn = move.x;    // A/D

        float trackSpeed;

        // A는 왼쪽 +, 오른쪽 -
        // D는 왼쪽 -, 오른쪽 +
        if (isLeft)
        {
            trackSpeed = forward + turn;
        }
        else
        {
            trackSpeed = forward - turn;
        }

        uv.y -= trackSpeed * speed * Time.deltaTime;
        
        rend.material.SetTextureOffset("_BaseMap", uv); // 뒤로 가는 궤도 모션
        #endregion

    }
}
