using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float sensX = 100f;
    [SerializeField] private float sensY = 100f;

    [SerializeField] private Transform camHolder;

    float mouseX;
    float mouseY;

    float xRotation;
    float yRotation;

    private void Awake() //locks the cursor so that it's invisible
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update() //moves the camera by the mouses inputs
    {
        mouseX = Input.GetAxisRaw("Mouse X");
        mouseY = Input.GetAxisRaw("Mouse Y");

        yRotation += mouseX * sensX * Time.deltaTime;
        xRotation -= mouseY * sensY * Time.deltaTime;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        camHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }
}
