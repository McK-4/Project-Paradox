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
    public bool sprintPressed { get; private set; } = false;
    //public bool waitForSprintRelease { get; private set; } = false;
    public bool crouchPressed { get; private set; } = false;
    public bool waitForCrouchRelease { get; private set; } = false;
    public bool grapplePressed { get; private set; } = false;
    public bool grapplePullPressed { get; private set; } = false;

    //Movement
    Vector3 movement;
    //[SerializeField] float movementSpeed = 3.75f;
    [SerializeField] float movementSpeed = 0.5f;
    [SerializeField] float movementSpeedMax = 3.75f;
    [SerializeField] float movementSpeedStart = 0.1f;
    [SerializeField] float acceleration = 0.1f;
    [SerializeField] bool moving = false;
    private float maxVelocityX = 7.60f;
    private float maxVelocityY = 7.60f;
    private float maxVelocityZ = 7.60f;
    //Sprint
    public float speedMultiplier { get; set; } = 1;
    //Jumping
    [SerializeField] float airControllAce = 3.75f;
    //[SerializeField] float sprintMultiplier = 1.5f;
    [SerializeField] float jumpPower = 350;
    [SerializeField] float rayCastDownDist = 1.1f;
    [SerializeField] LayerMask groundLayer;
    //Crouching
    [SerializeField] float crouchSpeed = 1.2f;
    [SerializeField] float localCrouchYScale = 0.5f;
    [SerializeField] float targetcrouchYScale = 0.5f;
    private float startYScale;


    //camera
    [SerializeField] Camera cam;
    [SerializeField] Transform PlayerCamera;
    [SerializeField] float upAndDownMaxs = 90f;
    public float sensitivityLeftAndRight = 10f;
    public float sensitivityUpAndDown = 50f;
    private float camFOV;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gh = GetComponent<GrapplingHook>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        camFOV = cam.fieldOfView;
        startYScale = transform.localScale.y;
    }

    // Update is called once per frame
    void Update()
    {
        updateLook();
        updateMove();
        updateGrapple();

        //This can be removed, just for testing:
        //transform.rotation = Quaternion.Euler(0, 0, 0);

        if(transform.position.y < -40)
        {
            transform.position = new Vector3(transform.position.x, 20, transform.position.z);
        }
    }
    void updateMove()
    {
        movement = rb.velocity;

        localCrouchYScale = transform.localScale.y;

        if (!sprintPressed)//&& waitForSprintRelease)
        {
            //speedMultiplier = 1;
            if (speedMultiplier < 1)
            {
                speedMultiplier = 1;// .02f;
                camFOV = 60;// .1f;
            }

            else if (speedMultiplier > 1.02)
            {
                speedMultiplier -= 3f * Time.deltaTime;
                camFOV -= 3.4f * Time.deltaTime;
                cam.fieldOfView = camFOV;
                Debug.Log("Cam FOV: " + cam.fieldOfView);
                Debug.Log(speedMultiplier);
            }
        }

        if (!IsGrounded() && transform.localScale.y != startYScale)
        {
            localCrouchYScale += crouchSpeed * Time.deltaTime;
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            /*
            if(speedMultiplier < 1)
            {
                speedMultiplier *= 2;
            }
            */
            speedMultiplier = 1;
            //rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        if (IsGrounded())
        {
            //resetting the ability to jump and sprint
            if (WaitForJumpRelease && !IsJumpPressed)
            {
                WaitForJumpRelease = false;
            }
            /*
            if (!sprintPressed )//&& waitForSprintRelease)
            {
                //speedMultiplier = 1;
                if (speedMultiplier <= 1)
                {
                    speedMultiplier = 1.02f;
                }

                else if (speedMultiplier > 1.02)
                {
                    speedMultiplier -= 2f * Time.deltaTime;
                    Debug.Log(speedMultiplier);
                }
                waitForSprintRelease = false;
            }

            /*
            if(MovementInput != Vector2.zero)
            {
                moving = true;
            }
            else
            {
                moving = false;
            }

            if (movementSpeed <= movementSpeedStart && !moving)
            {
                movementSpeed = movementSpeedStart;
            }

            if (movementSpeed >= movementSpeedMax)
            {
                movementSpeed = movementSpeedMax;
            }

            if (moving)
            {
                movementSpeed += acceleration;
            }
            else
            {
                movementSpeed -= (acceleration * 100);
            }
            */

            //snapy movement on the ground. forward to get characters forward and go that direction right to get strafing. (Needs to be adjusted for smoother movement)
            movement = ((transform.forward * MovementInput.y) + (transform.right * MovementInput.x)) * (movementSpeed * speedMultiplier);

            //Jumping
            if(IsJumpPressed && !WaitForJumpRelease)
            {
                rb.AddForce(new Vector3(0,jumpPower,0));
                WaitForJumpRelease = true;
            }

            //Sprinting
            if(sprintPressed )//&& !waitForSprintRelease)
            {
                if (speedMultiplier >= 4)
                {
                    speedMultiplier = 4;
                }

                else if(speedMultiplier < 4)
                {
                    speedMultiplier += 1.5f * Time.deltaTime;
                    camFOV += 1.7f * Time.deltaTime;
                    cam.fieldOfView = camFOV;
                    Debug.Log("Cam FOV: " + cam.fieldOfView);
                    Debug.Log(speedMultiplier);
                }
            }

            //Crouching
            if (crouchPressed && localCrouchYScale > targetcrouchYScale)
            {
                localCrouchYScale -= crouchSpeed * Time.deltaTime;
                //rb.AddForce(Vector3.down *5f, ForceMode.Impulse);
                speedMultiplier /= 2;
            }
            else if (!crouchPressed && localCrouchYScale < startYScale) 
            {
                localCrouchYScale += crouchSpeed * Time.deltaTime;
                speedMultiplier = 1;
            }
            transform.localScale = new Vector3(transform.localScale.x, localCrouchYScale, transform.localScale.z);
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
        //rb.velocity = new Vector3(Mathf.Clamp(rb.velocity.x, -maxVelocityX, maxVelocityX), Mathf.Clamp(rb.velocity.y, -maxVelocityY, maxVelocityY), Mathf.Clamp(rb.velocity.z, -maxVelocityZ, maxVelocityZ));

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

    public void SetSprintInput(InputAction.CallbackContext context)
    {
        sprintPressed = context.ReadValue<float>() != 0;
    }

    public void CrouchInput(InputAction.CallbackContext context)
    {
        crouchPressed = context.ReadValue<float>() != 0;
    }

    public void GrappleInput(InputAction.CallbackContext context)
    {
        grapplePressed = context.ReadValue<float>() != 0;
    }
    public void GrapplePullInput(InputAction.CallbackContext context)
    {
        grapplePullPressed = context.ReadValue<float>() != 0;
    }
}
