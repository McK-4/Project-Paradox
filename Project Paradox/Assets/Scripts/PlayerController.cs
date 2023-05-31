using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    //rigidbody for movement
    Rigidbody rb;
    
    GrapplingHook gh;
    //controlls
    public Vector2 MouseInput { get; private set; }
    public Vector2 MovementInput { get; private set; }
    public bool IsJumpPressed { get; private set; } = false;
    public bool WaitForJumpRelease { get; private set; } = false;
    public bool grapplePressed { get; private set; } = false;   
    public bool grapplePullPressed { get; private set; } = false;

    //Movement
    [SerializeField] float movementSpeed = 3.75f;
    [SerializeField] float airControllAce = 3.75f;
    [SerializeField] float sprintMultiplier = 1.5f;
    [SerializeField] float jumpPower = 350;
    [SerializeField] float rayCastDownDist = 1.1f;
    [SerializeField] LayerMask groundLayer;

    //camera
    [SerializeField] Transform PlayerCamera;
    [SerializeField] float upAndDownMaxs = 90f;
    public float sensitivityLeftAndRight = 10f;
    public float sensitivityUpAndDown = 50f;

    public float speedMultiplier { get; set; } = 1;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gh = GetComponent<GrapplingHook>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        updateLook();
        updateMove();
        updateGrapple();
    }
    void updateMove()
    {
        Vector3 movement = rb.velocity;
        if (IsGrounded())
        {
            //resetting the ability to jump
            if (WaitForJumpRelease && !IsJumpPressed)
            {
                WaitForJumpRelease = false;
            }

            //snapy movement on the ground. forward to get characters forward and go that direction right to get strafing.
            movement = ((transform.forward * MovementInput.y) + (transform.right * MovementInput.x)) * (movementSpeed * speedMultiplier);

            //for jumping
            if(IsJumpPressed && !WaitForJumpRelease)
            {
                rb.AddForce(new Vector3(0,jumpPower,0));
                WaitForJumpRelease = true;
            }
        }
        else
        {
            //air controll

            //air controll in the forward direction
            movement += transform.forward * MovementInput.y * airControllAce * speedMultiplier * Time.deltaTime;

            //air controll in the right direction
            movement += transform.right * MovementInput.x * airControllAce * speedMultiplier * Time.deltaTime;
        }
        movement.y = rb.velocity.y;
        rb.velocity = movement;

        
    }
    void updateLook()
    {
        //checking if mouse is active
        if (MouseInput != Vector2.zero)
        {
            //initilizing to the current rotation
            Vector3 newCameraRot = new Vector3(PlayerCamera.transform.rotation.eulerAngles.x, PlayerCamera.transform.rotation.eulerAngles.y, PlayerCamera.transform.rotation.eulerAngles.z);
            Vector3 newPlayerRot = new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
            if (newCameraRot.x > 180)
            {
                //gives us the underside of the half circle instead of having it go up to 360. 
                //this works by subtracting 360 by the to large number then setting it to be negetive
                //ex. -(360 - 344) -> -(16) -> -16  
                newCameraRot.x = -(360 - newCameraRot.x);
            }
            //add our up and down to camera
            newCameraRot.x = Mathf.Clamp(newCameraRot.x + (-MouseInput.y * sensitivityUpAndDown * Time.deltaTime), -upAndDownMaxs, upAndDownMaxs);

            //add out left and right to the player (turning the player to look where we want)
            newPlayerRot.y = newPlayerRot.y + (MouseInput.x * sensitivityLeftAndRight * Time.deltaTime);

            //setting our rotations to be right
            PlayerCamera.transform.rotation = Quaternion.Euler(newCameraRot);
            transform.rotation = Quaternion.Euler(newPlayerRot);
        }
    }
    void updateGrapple()
    {
        if(grapplePressed && !gh.isGrappling)
        {
            gh.StartGrapple();
        }else if(!grapplePressed && gh.isGrappling)
        {
            gh.EndGrapple();
        }
        if (grapplePullPressed && gh.isGrappling)
        {
            gh.Pull(rb, 10f);
        }
    }
    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, rayCastDownDist, groundLayer);
    }
    public void SetMovementInput(InputAction.CallbackContext context)
    {
        MovementInput = context.ReadValue<Vector2>().normalized;
    }

    public void SetMouseInput(InputAction.CallbackContext context)
    {
        MouseInput = context.ReadValue<Vector2>();

    }
    public void SetJumpInput(InputAction.CallbackContext context)
    {
        IsJumpPressed = context.ReadValue<float>() != 0;
        
    }

    public void GrappleInput(InputAction.CallbackContext context)
    {
        grapplePressed = context.ReadValue<float>() != 0;
    }
    public void GrapplePullInput(InputAction.CallbackContext context)
    {
        grapplePullPressed = context.ReadValue<float>() != 0;
        Debug.Log("Pull Pressed");
    }
}
