using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Vector2 rawMoveInput = Vector2.zero;


    public void OnMoveInput(InputAction.CallbackContext context) => rawMoveInput = context.ReadValue<Vector2>();


    private void Update()
    {
        transform.position += new Vector3(rawMoveInput.x, 0.0f, rawMoveInput.y) * Time.deltaTime;
    }
}
