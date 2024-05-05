using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interpolator : MonoBehaviour
{
    [Header("Values")]
    [SerializeField] private bool IsRotationNeeded = false;

    [Space(10)]

    [SerializeField] private float timeElapsed = 0f;
    [SerializeField] private float timeToReachTarget = 0.05f;
    [SerializeField] private float movementThreshold = 0.05f;

    private readonly List<TransformUpdate> futureTransformUpdates = new List<TransformUpdate>();
    private float squareMovementThreshold;

    private TransformUpdate to;
    private TransformUpdate from;
    private TransformUpdate previous;

    private bool interpolate;

    private void Start()
    {
        squareMovementThreshold = movementThreshold * movementThreshold;

        SetupInterpolator();
    }

    private void Update()
    {
        if (!interpolate)
            return;

        for (int i = 0; i < futureTransformUpdates.Count; i++)
        {
            if(NetworkManager.Instance.ServerTick >= futureTransformUpdates[i].Tick)
            {
                previous = to;
                to = futureTransformUpdates[i];
                from = new TransformUpdate(NetworkManager.Instance.InterpolationTick, transform.position, (IsRotationNeeded) ? transform.rotation : Quaternion.identity);
            }

            futureTransformUpdates.RemoveAt(i);
            i--;

            timeElapsed = 0f;
            float ticksToReach = (to.Tick - from.Tick);
            if (ticksToReach == 0f) ticksToReach = 0.001f;
            timeToReachTarget = ticksToReach * Time.fixedDeltaTime;
            //timeToReachTarget = (to.Tick - from.Tick) * Time.fixedDeltaTime;
        }

        timeElapsed += Time.deltaTime;
        InterpolatePosition(timeElapsed / timeToReachTarget);

        if (IsRotationNeeded)
            InterpolateRotation(timeElapsed / timeToReachTarget);
    }

    private void InterpolatePosition(float lerpAmount)
    {
        if((to.Position - previous.Position).sqrMagnitude < squareMovementThreshold)
        {
            if(to.Position != from.Position)
                transform.position = Vector3.Lerp(from.Position, to.Position, lerpAmount);

            return;
        }

        transform.position = Vector3.LerpUnclamped(from.Position, to.Position, lerpAmount);
    }

    private void InterpolateRotation(float lerpAmount)
    {
        if ((to.Rotation.eulerAngles - previous.Rotation.eulerAngles).sqrMagnitude < squareMovementThreshold)
        {
            if (to.Rotation != from.Rotation)
                transform.rotation = Quaternion.Lerp(from.Rotation, to.Rotation, lerpAmount);

            return;
        }

        transform.rotation = Quaternion.LerpUnclamped(from.Rotation, to.Rotation, lerpAmount);
    }

    public void NewUpdate(ushort tick, Vector3 position)
    {
        if (tick <= NetworkManager.Instance.InterpolationTick)
            return;

        for(int i = 0; i < futureTransformUpdates.Count; i++)
        {
            if(tick < futureTransformUpdates[i].Tick)
            {
                futureTransformUpdates.Insert(i, new TransformUpdate(tick, position, Quaternion.identity));
                return;
            }
        }

        futureTransformUpdates.Add(new TransformUpdate(tick, position, Quaternion.identity));
    }

    public void NewUpdate(ushort tick, Vector3 position, Quaternion rotation)
    {
        if (tick <= NetworkManager.Instance.InterpolationTick)
            return;

        for (int i = 0; i < futureTransformUpdates.Count; i++)
        {
            if (tick < futureTransformUpdates[i].Tick)
            {
                futureTransformUpdates.Insert(i, new TransformUpdate(tick, position, rotation));
                return;
            }
        }

        futureTransformUpdates.Add(new TransformUpdate(tick, position, rotation));
    }

    public void ClearTransforms()
    {
        interpolate = false;
        futureTransformUpdates.Clear();
    }

    public void SetupInterpolator()
    {
        to = new TransformUpdate(NetworkManager.Instance.ServerTick, transform.position, (IsRotationNeeded) ? transform.rotation : Quaternion.identity);
        from = new TransformUpdate(NetworkManager.Instance.InterpolationTick, transform.position, (IsRotationNeeded) ? transform.rotation : Quaternion.identity);
        previous = new TransformUpdate(NetworkManager.Instance.InterpolationTick, transform.position, (IsRotationNeeded) ? transform.rotation : Quaternion.identity);

        interpolate = true;
    }
}
