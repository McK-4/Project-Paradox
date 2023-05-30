using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerController : MonoBehaviour
{
    Rigidbody rb;
    public Vector2 MouseInput { get; private set; }
    public Vector2 MovementInput { get; private set; }

    //Movement
    [SerializeField] bool moving = false;
    private Vector2 tempVel;
    private float movementSpeedMax = 7.5f;
    private float movementSpeed = 3.75f;
    private float movementSpeedStart = 0.1f;
    private float maxVelocityX = 7.6f;
    private float maxVelocityY = 7.6f;
    private float acceleration = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        moving = false;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 moveDirection;
        if (MovementInput != Vector2.zero)
        {
            moving = true;
        }
        else
        {
            moving = false;
        }

        if (moving)
        {
            movementSpeed += acceleration;

            moveDirection = gameObject.transform.forward * MovementInput.y + gameObject.transform.right * MovementInput.x;

            tempVel = moveDirection * movementSpeed;
        }
        else if (!moving)
        {
            movementSpeed -= acceleration * 20;
            //Debug.Log()
        }

        if(movementSpeed <= movementSpeedStart && !moving)
        {
            movementSpeed = movementSpeedStart;
        }
        if (movementSpeed >= movementSpeedMax)
        {
            movementSpeed = movementSpeedMax;
        }

        rb.velocity = tempVel;
        rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -maxVelocityX, maxVelocityX), Mathf.Clamp(rb.velocity.y, -maxVelocityY, maxVelocityY));
    }


    public void SetMovementInput(InputAction.CallbackContext context)
    {
        MovementInput = context.ReadValue<Vector2>();
    }

    public void SetMouseInput(InputAction.CallbackContext context)
    {
        MouseInput = context.ReadValue<Vector2>().normalized;
        
    }
}
