//------------------------------------------------------------------------------
// <copyright file="SkeletonJointsFilterClippedLegs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// ClippedLegsFilter는 카메라 시야각(FOV) 하단에 의해 다리 관절이 클리핑될 때
/// 다리 관절의 위치를 부드럽게 조정하는 필터입니다. 스켈레탈 트래커에서 추론된
/// 관절 위치는 가끔 노이즈가 있거나 잘못될 수 있습니다. 특히 깊이 이미지에서
/// 다리의 일부만 관찰 가능한 경우가 해당됩니다. 이 필터는 더블 지수 필터를 사용하여
/// 높은 단계의 부드러움을 적용하며, 킥이나 높은 스텝 같은 움직임은 적절히 반영되도록 합니다.
/// 다리의 클리핑 및 추론 수준에 따라 필터링된 데이터를 스켈레톤 출력 데이터에 혼합합니다.
/// </summary>
public class ClippedLegsFilter
{
    // 모든 다리 관절이 추적된 경우의 혼합 가중치
    private readonly Vector3 allTracked;

    // 발 관절이 추론되거나 추적되지 않을 때의 혼합 가중치
    private readonly Vector3 footInferred;

    // 발목 및 그 이하가 추론되거나 추적되지 않을 때의 혼합 가중치
    private readonly Vector3 ankleInferred;

    // 무릎 및 그 이하가 추론되거나 추적되지 않을 때의 혼합 가중치
    private readonly Vector3 kneeInferred;

    // 관절 위치 필터
    private JointPositionsFilter filterJoints;

    // 왼쪽 무릎의 타임드 러프
    private TimedLerp lerpLeftKnee;

    // 왼쪽 발목의 타임드 러프
    private TimedLerp lerpLeftAnkle;

    // 왼쪽 발의 타임드 러프
    private TimedLerp lerpLeftFoot;

    // 오른쪽 무릎의 타임드 러프
    private TimedLerp lerpRightKnee;

    // 오른쪽 발목의 타임드 러프
    private TimedLerp lerpRightAnkle;

    // 오른쪽 발의 타임드 러프
    private TimedLerp lerpRightFoot;

    // 다리 필터링이 적용된 로컬 스켈레톤
    private KinectWrapper.NuiSkeletonData filteredSkeleton;

    /// <summary>
    /// 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    public ClippedLegsFilter()
    {
        this.lerpLeftKnee = new TimedLerp();
        this.lerpLeftAnkle = new TimedLerp();
        this.lerpLeftFoot = new TimedLerp();
        this.lerpRightKnee = new TimedLerp();
        this.lerpRightAnkle = new TimedLerp();
        this.lerpRightFoot = new TimedLerp();

        this.filterJoints = new JointPositionsFilter();
        this.filteredSkeleton = new KinectWrapper.NuiSkeletonData();

        // 무릎, 발목, 발 혼합 비율 설정
        this.allTracked = new Vector3(0.0f, 0.0f, 0.0f); // 모든 관절 추적
        this.footInferred = new Vector3(0.0f, 0.0f, 1.0f); // 발 추론
        this.ankleInferred = new Vector3(0.5f, 1.0f, 1.0f); // 발목 추론
        this.kneeInferred = new Vector3(1.0f, 1.0f, 1.0f); // 무릎 추론

        Reset();
    }

    // 필터 상태를 기본값으로 재설정합니다.
    public void Reset()
    {
        // 최대 부드러움을 위해 플로팅 더블 지수 필터 설정
        this.filterJoints.Init(0.5f, 0.3f, 1.0f, 1.0f, 1.0f);

        this.lerpLeftKnee.Reset();
        this.lerpLeftAnkle.Reset();
        this.lerpLeftFoot.Reset();
        this.lerpRightKnee.Reset();
        this.lerpRightAnkle.Reset();
        this.lerpRightFoot.Reset();
    }

    // 매 프레임 필터 로직을 구현합니다
    public bool FilterSkeleton(ref KinectWrapper.NuiSkeletonData skeleton, float deltaNuiTime)
    {
        //        if (null == skeleton)
        //        {
        //            return false;
        //        }

        // 스켈레톤의 추적 상태 확인
        if (skeleton.eTrackingState != KinectWrapper.NuiSkeletonTrackingState.SkeletonTracked)
        {
            filterJoints.Reset();
        }
        // 필터 적용을 위해 스켈레톤 복사
        KinectHelper.CopySkeleton(ref skeleton, ref filteredSkeleton);
        filterJoints.UpdateFilter(ref filteredSkeleton);

        // 타임드 러프 상태 업데이트
        this.lerpLeftKnee.Tick(deltaNuiTime);
        this.lerpLeftAnkle.Tick(deltaNuiTime);
        this.lerpLeftFoot.Tick(deltaNuiTime);
        this.lerpRightKnee.Tick(deltaNuiTime);
        this.lerpRightAnkle.Tick(deltaNuiTime);
        this.lerpRightFoot.Tick(deltaNuiTime);

        // 필터링된 데이터 및 혼합 비율 적용
        if ((!KinectHelper.IsTracked(skeleton, (int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter)) || 
			(!KinectHelper.IsTrackedOrInferred(skeleton, (int)KinectWrapper.NuiSkeletonPositionIndex.HipLeft)) || 
			(!KinectHelper.IsTrackedOrInferred(skeleton, (int)KinectWrapper.NuiSkeletonPositionIndex.HipRight)))
        {
            return false;
        }

        // 시야(FOV) 하단 클리핑 상태 확인
        bool clippedBottom = (skeleton.dwQualityFlags & (int)KinectWrapper.FrameEdges.Bottom) != 0;

        /// 왼쪽 다리의 관절 추적 상태에 따라 적절한 마스크 선택
        Vector3 leftLegMask = this.allTracked;

        if (!KinectHelper.IsTracked(skeleton, (int)KinectWrapper.NuiSkeletonPositionIndex.KneeLeft))
        {
            leftLegMask = this.kneeInferred;// 무릎 추론 상태
        }
        else if (!KinectHelper.IsTracked(skeleton, (int)KinectWrapper.NuiSkeletonPositionIndex.AnkleLeft))
        {
            leftLegMask = this.ankleInferred;// 발목 추론 상태
        }
        else if (!KinectHelper.IsTracked(skeleton, (int)KinectWrapper.NuiSkeletonPositionIndex.FootLeft))
        {
            leftLegMask = this.footInferred;// 발 추론 상태
        }

        // 오른쪽 다리의 관절 추적 상태에 따라 적절한 마스크 선택
        Vector3 rightLegMask = this.allTracked;

        if (!KinectHelper.IsTracked(skeleton, (int)KinectWrapper.NuiSkeletonPositionIndex.KneeRight))
        {
            rightLegMask = this.kneeInferred;// 무릎 추론 상태
        }
        else if (!KinectHelper.IsTracked(skeleton, (int)KinectWrapper.NuiSkeletonPositionIndex.AnkleRight))
        {
            rightLegMask = this.ankleInferred;// 발목 추론 상태
        }
        else if (!KinectHelper.IsTracked(skeleton, (int)KinectWrapper.NuiSkeletonPositionIndex.FootRight))
        {
            rightLegMask = this.footInferred;// 발 추론 상태
        }

        // 클리핑 여부에 따라 필터 데이터 혼합 비율 설정
        float clipMask = clippedBottom ? 1.0f : 0.5f;

        // 각 다리 관절에 대해 혼합 비율 설정
        this.lerpLeftKnee.SetEnabled(leftLegMask.x * clipMask);
        this.lerpLeftAnkle.SetEnabled(leftLegMask.y * clipMask);
        this.lerpLeftFoot.SetEnabled(leftLegMask.z * clipMask);
        this.lerpRightKnee.SetEnabled(rightLegMask.x * clipMask);
        this.lerpRightAnkle.SetEnabled(rightLegMask.y * clipMask);
        this.lerpRightFoot.SetEnabled(rightLegMask.z * clipMask);

        // 스켈레톤 업데이트 여부 플래그
        bool skeletonUpdated = false;

        // 왼쪽 무릎의 보간 적용
        if (this.lerpLeftKnee.IsLerpEnabled())
        {
            int jointIndex = (int)KinectWrapper.NuiSkeletonPositionIndex.KneeLeft;
            KinectHelper.LerpAndApply(ref skeleton, jointIndex, (Vector3)filteredSkeleton.SkeletonPositions[jointIndex], lerpLeftKnee.SmoothValue, KinectWrapper.NuiSkeletonPositionTrackingState.Tracked);
            skeletonUpdated = true;
        }

        // 왼쪽 발목의 보간 적용
        if (this.lerpLeftAnkle.IsLerpEnabled())
        {
            int jointIndex = (int)KinectWrapper.NuiSkeletonPositionIndex.AnkleLeft;
            KinectHelper.LerpAndApply(ref skeleton, jointIndex, (Vector3)filteredSkeleton.SkeletonPositions[jointIndex], lerpLeftAnkle.SmoothValue, KinectWrapper.NuiSkeletonPositionTrackingState.Tracked);
            skeletonUpdated = true;
        }

        // 왼쪽 발의 보간 적용
        if (this.lerpLeftFoot.IsLerpEnabled())
        {
            int jointIndex = (int)KinectWrapper.NuiSkeletonPositionIndex.FootLeft;
            KinectHelper.LerpAndApply(ref skeleton, jointIndex, (Vector3)filteredSkeleton.SkeletonPositions[jointIndex], lerpLeftFoot.SmoothValue, KinectWrapper.NuiSkeletonPositionTrackingState.Inferred);
            skeletonUpdated = true;
        }

        // 오른쪽 무릎의 보간 적용
        if (this.lerpRightKnee.IsLerpEnabled())
        {
            int jointIndex = (int)KinectWrapper.NuiSkeletonPositionIndex.KneeRight;
            KinectHelper.LerpAndApply(ref skeleton, jointIndex, (Vector3)filteredSkeleton.SkeletonPositions[jointIndex], lerpRightKnee.SmoothValue, KinectWrapper.NuiSkeletonPositionTrackingState.Tracked);
            skeletonUpdated = true;
        }

        // 오른쪽 발목의 보간 적용
        if (this.lerpRightAnkle.IsLerpEnabled())
        {
            int jointIndex = (int)KinectWrapper.NuiSkeletonPositionIndex.AnkleRight;
            KinectHelper.LerpAndApply(ref skeleton, jointIndex, (Vector3)filteredSkeleton.SkeletonPositions[jointIndex], lerpRightAnkle.SmoothValue, KinectWrapper.NuiSkeletonPositionTrackingState.Tracked);
            skeletonUpdated = true;
        }

        // 오른쪽 발의 보간 적용
        if (this.lerpRightFoot.IsLerpEnabled())
        {
            int jointIndex = (int)KinectWrapper.NuiSkeletonPositionIndex.FootRight;
            KinectHelper.LerpAndApply(ref skeleton, jointIndex, (Vector3)filteredSkeleton.SkeletonPositions[jointIndex], lerpRightFoot.SmoothValue, KinectWrapper.NuiSkeletonPositionTrackingState.Inferred);
            skeletonUpdated = true;
        }

        // 스켈레톤이 업데이트되었는지 여부 반환
        return skeletonUpdated;
    }
}

