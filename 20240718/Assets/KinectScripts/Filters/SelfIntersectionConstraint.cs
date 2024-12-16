//------------------------------------------------------------------------------
// <copyright file="SkeletonJointsSelfIntersectionConstraint.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// 골격 팔 관절이 "몸"과 겹치지 않도록 하는 필터.
/// </summary>
public class SelfIntersectionConstraint
{
    // 원통 생성 파라미터
    public float ShoulderExtend = 0.5f; // 어깨 확장
    public float HipExtend = 6.0f; // 엉덩이 확장
    public float CollisionTolerance = 1.01f; // 충돌 허용 오차
    public float RadiusMultiplier = 1.3f; // 부피가 큰 아바타의 경우 원통 반지름 증가


    // 클래스의 새 인스턴스를 초기화합니다.
    public SelfIntersectionConstraint()
	{
	}

    // ConstrainSelfIntersection은 골격의 관절을 충돌시켜 손목과 손이 몸을 뚫지 않도록 합니다.
    // 원통을 생성하여 몸통을 나타내고, 겹치는 관절의 위치를 조정하여 몸통 밖으로 밀어냅니다.
    public void Constrain(ref KinectWrapper.NuiSkeletonData skeleton)
    {
//        if (null == skeleton)
//        {
//            return;
//        }

		int shoulderCenterIndex = (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter;
		int hipCenterIndex = (int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter;

        if (skeleton.eSkeletonPositionTrackingState[shoulderCenterIndex] != KinectWrapper.NuiSkeletonPositionTrackingState.NotTracked &&
            skeleton.eSkeletonPositionTrackingState[hipCenterIndex] != KinectWrapper.NuiSkeletonPositionTrackingState.NotTracked)
        {
            Vector3 shoulderDiffLeft = KinectHelper.VectorBetween(ref skeleton, shoulderCenterIndex, (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft);
            Vector3 shoulderDiffRight = KinectHelper.VectorBetween(ref skeleton, shoulderCenterIndex, (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight);
            float shoulderLengthLeft = shoulderDiffLeft.magnitude;
            float shoulderLengthRight = shoulderDiffRight.magnitude;

            // 어깨 간 거리를 평균내어 반지름을 계산
            float cylinderRadius = (shoulderLengthLeft + shoulderLengthRight) * 0.5f;

            // 어깨 중앙과 엉덩이 중앙을 계산하고, 각각 위아래로 확장합니다.
            Vector3 shoulderCenter = (Vector3)skeleton.SkeletonPositions[shoulderCenterIndex];
            Vector3 hipCenter = (Vector3)skeleton.SkeletonPositions[hipCenterIndex];
            Vector3 hipShoulder = hipCenter - shoulderCenter;
            hipShoulder.Normalize();

            shoulderCenter = shoulderCenter - (hipShoulder * (ShoulderExtend * cylinderRadius));
            hipCenter = hipCenter + (hipShoulder * (HipExtend * cylinderRadius));

            // 선택적으로 부피가 큰 아바타를 위한 반지름을 증가시킵니다.
            cylinderRadius *= RadiusMultiplier;

            // 충돌할 관절
            int[] collisionIndices = 
			{ 
				(int)KinectWrapper.NuiSkeletonPositionIndex.WristLeft, 
				(int)KinectWrapper.NuiSkeletonPositionIndex.HandLeft, 
				(int)KinectWrapper.NuiSkeletonPositionIndex.WristRight, 
				(int)KinectWrapper.NuiSkeletonPositionIndex.HandRight 
			};
    
            foreach (int j in collisionIndices)
            {
                Vector3 collisionJoint = (Vector3)skeleton.SkeletonPositions[j];
                
                Vector4 distanceNormal = KinectHelper.DistanceToLineSegment(shoulderCenter, hipCenter, collisionJoint);

                Vector3 normal = new Vector3(distanceNormal.x, distanceNormal.y, distanceNormal.z);

                // if distance is within the cylinder then push the joint out and away from the cylinder
                if (distanceNormal.w < cylinderRadius)
                {
                    collisionJoint += normal * ((cylinderRadius - distanceNormal.w) * CollisionTolerance);

                    skeleton.SkeletonPositions[j] = (Vector4)collisionJoint;
                }
            }
        }
    }
}
