using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interpolator : MonoBehaviour
{
    [SerializeField] private float timeElapsed = 0f;
    [SerializeField] private float timeToReachTarget = 0.05f;
    [SerializeField] private float movementThreshold = 0.05f;

    private readonly List<TransformUpdate> futureTransformUpdates = new List<TransformUpdate>();
    private float squareMovementThreshold;

    private TransformUpdate to;
    private TransformUpdate from;
    private TransformUpdate previous;

    private void Start()
    {
        squareMovementThreshold = movementThreshold * movementThreshold;

        to = new TransformUpdate(NetworkManager.Instance.ServerTick, transform.position);
        from = new TransformUpdate(NetworkManager.Instance.InterpolationTick, transform.position);
        previous = new TransformUpdate(NetworkManager.Instance.InterpolationTick, transform.position);
    }

    private void Update()
    {
        for(int i = 0; i < futureTransformUpdates.Count; i++)
        {
            if(NetworkManager.Instance.ServerTick >= futureTransformUpdates[i].Tick)
            {
                previous = to;
                to = futureTransformUpdates[i];
                from = new TransformUpdate(NetworkManager.Instance.InterpolationTick, transform.position);
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

    public void NewUpdate(ushort tick, Vector3 position)
    {
        if (tick <= NetworkManager.Instance.InterpolationTick)
            return;

        for(int i = 0; i < futureTransformUpdates.Count; i++)
        {
            if(tick < futureTransformUpdates[i].Tick)
            {
                futureTransformUpdates.Insert(i, new TransformUpdate(tick, position));
                return;
            }
        }

        futureTransformUpdates.Add(new TransformUpdate(tick, position));
    }
}
