using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformUpdate
{
    public ushort Tick {  get; private set; }
    public Vector3 Position { get; private set; }
    public Quaternion Rotation { get; private set; }

    public TransformUpdate(ushort tick, Vector3 position, Quaternion rotation)
    {
        Tick = tick;
        Position = position;
        Rotation = rotation;
    }
}
