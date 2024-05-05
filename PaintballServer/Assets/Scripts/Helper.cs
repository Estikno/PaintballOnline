using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class Helper
{
    private static Camera _camera;
    /// <summary>
    /// Main camera
    /// </summary>
    public static Camera Camera
    {
        get
        {
            if (_camera == null) _camera = Camera.main;
            return _camera;
        }

    }

    /// <summary>
    /// Gets the index of the SelectedWeapon var
    /// </summary>
    /// <param name="type">The type of the gun</param>
    /// <returns>The index as an int</returns>
    public static int GetGunIndexWithType(GunType type)
    {
        if (type == GunType.knife) return 2;
        if (type == GunType.pistol) return 1;
        else return 0;
    }


    private static readonly Dictionary<float, WaitForSeconds> WaitDictionary = new Dictionary<float, WaitForSeconds>();
    /// <summary>
    /// Non-allocating WaitForSeconds
    /// </summary>
    /// <param name="time">The amount of time (in seconds)</param>
    /// <returns></returns>
    public static WaitForSeconds GetWait(float time)
    {
        if (WaitDictionary.TryGetValue(time, out WaitForSeconds wait)) return wait;

        WaitDictionary[time] = new WaitForSeconds(time);
        return WaitDictionary[time];
    }

    private static PointerEventData _eventDataCurrentPosition;
    private static List<RaycastResult> _results;
    /// <summary>
    /// Sees if the mouse is over an UI object
    /// </summary>
    /// <returns>Returns true if it is over the UI</returns>
    public static bool IsOverGUI()
    {
        _eventDataCurrentPosition = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        _results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(_eventDataCurrentPosition, _results);
        return _results.Count > 0;
    }

    public static Vector3 GetWorldPositionOfCanvasElement(RectTransform element)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(element, element.position, Camera, out Vector3 result);
        return result;
    }

    /// <summary>
    /// Deletes all children
    /// </summary>
    /// <param name="t"></param>
    public static void DeleteChildren(this Transform t)
    {
        foreach (Transform child in t) Object.Destroy(child.gameObject);
    }
}
