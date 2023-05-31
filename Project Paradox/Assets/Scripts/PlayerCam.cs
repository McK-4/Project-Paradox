using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(PlayerController))]
public class PlayerCam : MonoBehaviour
{
    [SerializeField] Transform PlayerCamera;
    [SerializeField] float upAndDownMaxs = 90f;
    public float sensitivityLeftAndRight = 10f;
    public float sensitivityUpAndDown = 50f;

    PlayerController input;

    Vector2 MouseInput;
    // Start is called before the first frame update
    void Start()
    {
        //getting input for camera
        input = gameObject.GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        updateInput();
        //checking if mouse is active
        if(MouseInput != Vector2.zero)
        {
            //initilizing to the current rotation
            Vector3 newCameraRot = new Vector3(PlayerCamera.transform.rotation.eulerAngles.x, PlayerCamera.transform.rotation.eulerAngles.y, PlayerCamera.transform.rotation.eulerAngles.z);
            Vector3 newPlayerRot = new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
            if(newCameraRot.x > 180)
            {
                //gives us the underside of the half circle instead of having it go up to 360. 
                //this works by subtracting 360 by the to large number then setting it to be negetive
                //ex. -(360 - 344) -> -(16) -> -16  
                newCameraRot.x = -(360 - newCameraRot.x);
            }
            //add our up and down to camera
            newCameraRot.x = Mathf.Clamp(newCameraRot.x + (MouseInput.y * sensitivityUpAndDown * Time.deltaTime), -upAndDownMaxs, upAndDownMaxs);

            //add out left and right to the player (turning the player to look where we want)
            newPlayerRot.y = newPlayerRot.y + (MouseInput.x * sensitivityLeftAndRight * Time.deltaTime); 
            
            //setting our rotations to be right
            PlayerCamera.transform.rotation = Quaternion.Euler(newCameraRot);
            transform.rotation = Quaternion.Euler(newPlayerRot);
        }
    }


    /// <summary>
    /// updates input
    /// </summary>
    private void updateInput()
    {
        //getting mouse input from the player controller
        MouseInput = input.MouseInput;
        MouseInput.y *= -1;
    }
}
