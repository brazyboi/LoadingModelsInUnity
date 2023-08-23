using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sens;

    public Transform orientation;

    float xRotation;
    float yRotation;

    // Update is called once per frame
    void Update()
    {
        //Get mouse inputs
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sens;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sens;

        //Add rotation based on mouse inputs
        yRotation += mouseX;
        xRotation -= mouseY;

        //Cannot vertically rotate camera beyond 90 degrees
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //Rotate cam and orientation
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
