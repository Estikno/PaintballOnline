using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private static CameraController instance;
    public static CameraController Instance
    {
        get => instance;
        private set
        {
            if (instance == null)
            {
                instance = value;
            }
            else if (instance != null)
            {
                Debug.Log($"{nameof(CameraController)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

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
        Instance = this;

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

            //pause menu
            PauseWindow.Instance.DisplayPauseMenu(true);
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            Resume();
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

    public void Resume()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SettingsManager.GetMouseActivity = true;

        PauseWindow.Instance.DisplayPauseMenu(false);
    }
}
