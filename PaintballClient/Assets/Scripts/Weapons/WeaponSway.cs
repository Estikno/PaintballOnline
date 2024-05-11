using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    [Range(0f, 1f)]
    [SerializeField] private float intensity;
    [SerializeField] private float smooth;

    private Quaternion originRotation;
    private bool toSway = false;
    private float multiplier = 20f;

    private void Update()
    {
        if (!toSway) return;

        Sway();
    }

    public void Initiate()
    {
        originRotation = transform.localRotation;
        toSway = true;
    }

    public void Stop()
    {
        toSway = false;
        transform.localRotation = originRotation;
    }

    private void Sway()
    {
        //calculate target rotation
        Quaternion txAdj = Quaternion.AngleAxis(-intensity * CameraController.MouseX * multiplier, Vector3.up);
        Quaternion tyAdj = Quaternion.AngleAxis(intensity * CameraController.MouseY * multiplier, Vector3.right);
        Quaternion tzAdj = Quaternion.AngleAxis(intensity * CameraController.MouseX * multiplier, Vector3.forward);
        Quaternion targetRotation = originRotation * txAdj * tyAdj * tzAdj;

        //rotate towards target rotation
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * smooth);
    }
}
