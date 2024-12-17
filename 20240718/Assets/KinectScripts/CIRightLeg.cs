using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CIRightLeg : BodyController
{
    protected override string CsvDirectory => "Assets/Resources/Right Leg";

    private void Update()
    {
        // 이미지 보이게 하기
        float currentAngle = CalculateAngle();
        // 실시간 각도 측정
        angleText3.text = currentAngle.ToString("F2");

        

        Color originColor = Color.yellow;
        float alpha = Mathf.Clamp01(currentAngle / 45f); // 각도에 따라 알파 값 결정

        Color newColor = stars.color;

        if (currentAngle > 48f)
        {
            // 각도가 45를 넘으면 색상을 빨간색으로 변경
            newColor = Color.red;
        }
        else if (currentAngle < 43)
        {
            newColor = Color.blue;
        }
        else
        {
            // 각도가 45 이하이면 알파 값만 변경
            newColor.a = alpha;
            newColor = originColor;
        }

        stars.color = newColor;
    }
    protected override float CalculateAngle()
    {
        
        Vector3 AnkleLeft = GetJointPosition(KinectWrapper.NuiSkeletonPositionIndex.AnkleLeft);
        Vector3 HipLeft = GetJointPosition(KinectWrapper.NuiSkeletonPositionIndex.HipLeft);
        Vector3 AnkleRight = GetJointPosition(KinectWrapper.NuiSkeletonPositionIndex.AnkleRight);
        return Vector3.Angle(AnkleLeft - HipLeft, HipLeft - AnkleRight);
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
