using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SettingsManager
{
    public static float SensitivityX { get; private set; } = 400f;
    public static float SensitivityY { get; private set; } = 400f;
    public static bool GetMouseActivity = true;

    public static void SetSensitivityX(float sensitivityX)
    {
        SensitivityX = sensitivityX;
    }

    public static void SetSensitivityY(float sensitivityY)
    {
        SensitivityY = sensitivityY;
    }
}
