using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private MovementControl _controller;

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        _controller.MoveAxis(new Vector2(horizontal, vertical));
        
        if (Input.GetKeyDown(KeyCode.Space))
            _controller.Use();

        if (Input.GetKey(KeyCode.LeftShift))
            _controller.UseDash();
    }
}
