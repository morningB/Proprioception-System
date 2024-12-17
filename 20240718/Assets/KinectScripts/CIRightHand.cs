using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CIightHand : BodyController
{
    protected override string CsvDirectory => "Assets/Resources/Right Hand";

    protected override float CalculateAngle()
    {
        Vector3 shoulder = GetJointPosition(KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight);
        Vector3 elbow = GetJointPosition(KinectWrapper.NuiSkeletonPositionIndex.ElbowRight);
        Vector3 hand = GetJointPosition(KinectWrapper.NuiSkeletonPositionIndex.HandRight);

        return Vector3.Angle(elbow - shoulder, hand - elbow);
    }

    private Vector3 GetJointPosition(KinectWrapper.NuiSkeletonPositionIndex joint)
    {
        KinectManager manager = KinectManager.Instance;
        if (manager != null && manager.IsInitialized() && manager.IsUserDetected())
        {
            uint userId = manager.GetPlayer1ID();
            if (manager.IsJointTracked(userId, (int)joint))
                return manager.GetJointPosition(userId, (int)joint);
        }
        return Vector3.zero;
    }
}
