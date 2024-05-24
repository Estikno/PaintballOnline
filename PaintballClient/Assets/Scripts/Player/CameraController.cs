using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static float MouseX { get; private set; }
    public static float MouseY { get; private set; }

    /*[SerializeField] private float sensX = 100f;
    [SerializeField] private float sensY = 100f;*/

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
        if (Input.GetKeyDown(KeyCode.Escape) && !Cursor.visible)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SettingsManager.GetMouseActivity = false;
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            SettingsManager.GetMouseActivity = true;
        }

        if (!SettingsManager.GetMouseActivity) return;

        //mouse input
        mouseX = Input.GetAxisRaw("Mouse X");
        mouseY = Input.GetAxisRaw("Mouse Y");

        //set static variables
        MouseX = mouseX;
        MouseY = mouseY;

        //apply rotations
        yRotation += mouseX * SettingsManager.SensitivityX * Time.deltaTime;
        xRotation -= mouseY * SettingsManager.SensitivityY * Time.deltaTime;

        xRotation = Mathf.Clamp(xRotation, -88f, 88f);

        camHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }
}
