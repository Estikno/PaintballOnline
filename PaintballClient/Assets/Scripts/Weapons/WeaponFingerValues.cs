using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponFingerValues : MonoBehaviour
{
    [SerializeField] private List<Transform> leftFingers;
    [SerializeField] private List<Vector3> leftRotationsOfFingers;

    [SerializeField] private List<Transform> rightFingers;
    [SerializeField] private List<Vector3> rightRotationsOfFingers;

    [SerializeField] private Transform rightHandTarget;
    [SerializeField] private Transform leftHandTarget;

    [SerializeField] private Vector3 rightHandRotation;
    [SerializeField] private Vector3 leftHandRotation;

    private void Start()
    {
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
