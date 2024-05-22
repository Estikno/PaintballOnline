using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponFingerValues : MonoBehaviour
{
    [SerializeField] private IK rightHandIk;
    [SerializeField] private IK leftHandIk;

    [SerializeField] private List<Transform> leftFingers;
    [SerializeField] private List<Vector3> leftRotationsOfFingers;

    [SerializeField] private List<Transform> rightFingers;
    [SerializeField] private List<Vector3> rightRotationsOfFingers;

    [SerializeField] private Vector3 rightHandRotation;
    [SerializeField] private Vector3 leftHandRotation;

    public void Init(Transform rightHandTarget, Transform leftHandTarget, Transform rightPole, Transform leftPole)
    {
        rightHandIk.Target = rightHandTarget;
        rightHandIk.Pole = rightPole;

        leftHandIk.Target = leftHandTarget;
        leftHandIk.Pole = leftPole;

        rightHandIk.Init();
        leftHandIk.Init();

        rightHandTarget.localRotation = Quaternion.Euler(rightHandRotation);
        leftHandTarget.localRotation = Quaternion.Euler(leftHandRotation);

        for (int i = 0; i < rightFingers.Count; i++)
        {
            rightFingers[i].localRotation = Quaternion.Euler(rightRotationsOfFingers[i]);
        }

        for (int i = 0; i < leftFingers.Count; i++)
        {
            leftFingers[i].localRotation = Quaternion.Euler(leftRotationsOfFingers[i]);
        }
    }
}
