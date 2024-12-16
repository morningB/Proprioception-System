//------------------------------------------------------------------------------
// <copyright file="BoneOrientationDoubleExponentialFilter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// 자세 데이터를 필터링하기 위한 Holt 이중 지수 평활 필터 구현입니다. 이중 지수 평활은 곡선을 부드럽게 하고 예측을 수행합니다.
/// 또한 노이즈 제거 및 최대 예측 범위를 설정합니다. 필터 파라미터는 Init 함수에 주석으로 설명되어 있습니다.
/// </summary>
public class BoneOrientationsFilter
{
    // 이전 프레임에서 필터링된 자세 데이터
    private FilterDoubleExponentialData[] history;

    // 이 필터를 위한 변환 스무딩 파라미터
    private KinectWrapper.NuiTransformSmoothParameters smoothParameters;

    // 필터 파라미터가 초기화되었는지 여부
    private bool init;

    // 클래스의 새 인스턴스를 초기화합니다.
    public BoneOrientationsFilter()
    {
        this.init = false;
    }

    // 기본 TransformSmoothParameters로 필터를 초기화합니다.
    public void Init()
    {
        // 적절한 기본값 설정
        this.Init(0.5f, 0.8f, 0.75f, 0.1f, 0.1f);
    }

    /// <summary>
    /// TransformSmoothParameters를 수동으로 설정하여 필터를 초기화합니다.
    /// </summary>
    /// <param name="smoothingValue">Smoothing = [0..1], 값이 낮을수록 원시 데이터에 가깝고 노이즈가 많습니다.</param>
    /// <param name="correctionValue">Correction = [0..1], 값이 높을수록 빠르게 교정되어 반응이 더 빠릅니다.</param>
    /// <param name="predictionValue">Prediction = [0..n], 미래 몇 프레임까지 예측할지 결정합니다.</param>
    /// <param name="jitterRadiusValue">JitterRadius = 노이즈를 정의하는 편차 각도(라디안)입니다.</param>
    /// <param name="maxDeviationRadiusValue">MaxDeviation = 필터링된 위치가 원시 데이터에서 벗어날 수 있는 최대 각도(라디안)입니다.</param>
    public void Init(float smoothingValue, float correctionValue, float predictionValue, float jitterRadiusValue, float maxDeviationRadiusValue)
    {
        this.smoothParameters = new KinectWrapper.NuiTransformSmoothParameters();

        this.smoothParameters.fMaxDeviationRadius = maxDeviationRadiusValue; // 최대 예측 반경, 너무 크면 노이즈 데이터로 돌아갈 수 있음
        this.smoothParameters.fSmoothing = smoothingValue;                   // 부드러움 정도, 너무 크면 지연 발생
        this.smoothParameters.fCorrection = correctionValue;                 // 예측에서 얼마나 빨리 교정할지, 너무 크면 스프링 효과
        this.smoothParameters.fPrediction = predictionValue;                 // 미래 몇 프레임을 예측할지 결정, 너무 크면 과도하게 예측
        this.smoothParameters.fJitterRadius = jitterRadiusValue;             // 노이즈를 제거하는 반경 크기, 너무 크면 과도한 부드러움 발생


        this.Reset();
        this.init = true;
    }

    // TransformSmoothParameters로 필터를 초기화합니다.
    public void Init(KinectWrapper.NuiTransformSmoothParameters smoothingParameters)
    {
        this.smoothParameters = smoothingParameters;
		
        this.Reset();
        this.init = true;
    }

    /// 필터를 기본값으로 재설정합니다.
    public void Reset()
    {
        this.history = new FilterDoubleExponentialData[(int)KinectWrapper.NuiSkeletonPositionIndex.Count];
    }

    // 골격 관절 회전 데이터에 이중 지수 평활 필터를 적용합니다.
    public void UpdateFilter(ref KinectWrapper.NuiSkeletonData skeleton, ref Matrix4x4[] jointOrientations)
    {
//        if (null == skeleton)
//        {
//            return;
//        }

//        if (skeleton.eTrackingState != KinectWrapper.NuiSkeletonTrackingState.SkeletonTracked)
//        {
//            return;
//        }

        if (this.init == false)
        {
            this.Init(); // 기본 파라미터로 초기화               
        }

        KinectWrapper.NuiTransformSmoothParameters tempSmoothingParams = new KinectWrapper.NuiTransformSmoothParameters();

        // 0으로 나누는 문제 방지, 0.1mm의 작은 값을 사용
        this.smoothParameters.fJitterRadius = Math.Max(0.0001f, this.smoothParameters.fJitterRadius);

        tempSmoothingParams.fSmoothing = smoothParameters.fSmoothing;
        tempSmoothingParams.fCorrection = smoothParameters.fCorrection;
        tempSmoothingParams.fPrediction = smoothParameters.fPrediction;
		
		int jointsCount = (int)KinectWrapper.NuiSkeletonPositionIndex.Count;
        for(int jointIndex = 0; jointIndex < jointsCount; jointIndex++)
        {
            //KinectWrapper.NuiSkeletonPositionIndex jt = (KinectWrapper.NuiSkeletonPositionIndex)jointIndex;

            // 관절이 추적되지 않는 경우 더 큰 Jitter 반경을 사용하여 더 부드럽게 필터링
            if (skeleton.eSkeletonPositionTrackingState[jointIndex] != KinectWrapper.NuiSkeletonPositionTrackingState.Tracked || 
				jointIndex == (int)KinectWrapper.NuiSkeletonPositionIndex.FootLeft || jointIndex == (int)KinectWrapper.NuiSkeletonPositionIndex.FootRight)
            {
                tempSmoothingParams.fJitterRadius = smoothParameters.fJitterRadius * 2.0f;
                tempSmoothingParams.fMaxDeviationRadius = smoothParameters.fMaxDeviationRadius * 2.0f;
            }
            else
            {
                tempSmoothingParams.fJitterRadius = smoothParameters.fJitterRadius;
                tempSmoothingParams.fMaxDeviationRadius = smoothParameters.fMaxDeviationRadius;
            }

            FilterJoint(ref skeleton, jointIndex, ref tempSmoothingParams, ref jointOrientations);
        }
    }

    // 하나의 관절에 대한 필터를 업데이트합니다.
    protected void FilterJoint(ref KinectWrapper.NuiSkeletonData skeleton, int jointIndex, ref KinectWrapper.NuiTransformSmoothParameters smoothingParameters, ref Matrix4x4[] jointOrientations)
    {
        //        if (null == skeleton)
        //        {
        //            return;
        //        }

        //        int jointIndex = (int)jt;
        // 필터링 초기화 또는 리셋
        Quaternion filteredOrientation;
        Quaternion trend;
        // 관절의 방향 데이터에서 forward 벡터를 가져옵니다.
        Vector3 fwdVector = (Vector3)jointOrientations[jointIndex].GetColumn(2);
		if(fwdVector == Vector3.zero)
			return;
        // 관절의 raw(원시) 방향을 계산합니다.
        Quaternion rawOrientation = Quaternion.LookRotation(fwdVector, jointOrientations[jointIndex].GetColumn(1));
        Quaternion prevFilteredOrientation = this.history[jointIndex].FilteredBoneOrientation;
        Quaternion prevTrend = this.history[jointIndex].Trend;
        Vector3 rawPosition = (Vector3)skeleton.SkeletonPositions[jointIndex];
        // 관절 데이터가 유효한지 확인합니다.
        bool orientationIsValid = KinectHelper.JointPositionIsValid(rawPosition) && KinectHelper.IsTrackedOrInferred(skeleton, jointIndex) && KinectHelper.BoneOrientationIsValid(rawOrientation);

        if (!orientationIsValid) // 유효하지 않으면
        {
            if (this.history[jointIndex].FrameCount > 0)
            {
                rawOrientation = history[jointIndex].FilteredBoneOrientation;
                history[jointIndex].FrameCount = 0;
            }
        }

        // Initial start values or reset values
        if (this.history[jointIndex].FrameCount == 0)
        {
            // 첫 번째 프레임
            filteredOrientation = rawOrientation;
            trend = Quaternion.identity; // 초기 추세는 단위 쿼터니언
        }
        else if (this.history[jointIndex].FrameCount == 1) // 두 번째 프레임
        {
            // Use average of two positions and calculate proper trend for end value
            Quaternion prevRawOrientation = this.history[jointIndex].RawBoneOrientation;
            filteredOrientation = KinectHelper.EnhancedQuaternionSlerp(prevRawOrientation, rawOrientation, 0.5f);

            Quaternion diffStarted = KinectHelper.RotationBetweenQuaternions(filteredOrientation, prevFilteredOrientation);
            trend = KinectHelper.EnhancedQuaternionSlerp(prevTrend, diffStarted, smoothingParameters.fCorrection);
        }
        else// 세 번째 이후 프레임
        {
            // 지터 필터 적용
            Quaternion diffJitter = KinectHelper.RotationBetweenQuaternions(rawOrientation, prevFilteredOrientation);
            float diffValJitter = (float)Math.Abs(KinectHelper.QuaternionAngle(diffJitter));

            if (diffValJitter <= smoothingParameters.fJitterRadius)
            {
                filteredOrientation = KinectHelper.EnhancedQuaternionSlerp(prevFilteredOrientation, rawOrientation, diffValJitter / smoothingParameters.fJitterRadius);
            }
            else
            {
                filteredOrientation = rawOrientation;
            }

            // 이중 지수 평활 필터 적용
            filteredOrientation = KinectHelper.EnhancedQuaternionSlerp(filteredOrientation, prevFilteredOrientation * prevTrend, smoothingParameters.fSmoothing);

            diffJitter = KinectHelper.RotationBetweenQuaternions(filteredOrientation, prevFilteredOrientation);
            trend = KinectHelper.EnhancedQuaternionSlerp(prevTrend, diffJitter, smoothingParameters.fCorrection);
        }

        // 지연을 줄이기 위해 추세를 사용하여 미래를 예측
        Quaternion predictedOrientation = filteredOrientation * KinectHelper.EnhancedQuaternionSlerp(Quaternion.identity, trend, smoothingParameters.fPrediction);

        // 원시 데이터와의 편차를 확인하고 제한
        Quaternion diff = KinectHelper.RotationBetweenQuaternions(predictedOrientation, filteredOrientation);
        float diffVal = (float)Math.Abs(KinectHelper.QuaternionAngle(diff));

        if (diffVal > smoothingParameters.fMaxDeviationRadius)
        {
            predictedOrientation = KinectHelper.EnhancedQuaternionSlerp(filteredOrientation, predictedOrientation, smoothingParameters.fMaxDeviationRadius / diffVal);
        }

        //        predictedOrientation.Normalize();
        //        filteredOrientation.Normalize();
        //        trend.Normalize();

        // 이 프레임의 데이터를 저장
        history[jointIndex].RawBoneOrientation = rawOrientation;
        history[jointIndex].FilteredBoneOrientation = filteredOrientation;
        history[jointIndex].Trend = trend;
        history[jointIndex].FrameCount++;

        // 필터링 및 예측된 데이터를 관절 방향에 설정
        if (KinectHelper.BoneOrientationIsValid(predictedOrientation))
		{
			jointOrientations[jointIndex].SetTRS(Vector3.zero, predictedOrientation, Vector3.one);
		}
    }

    /// 이중 지수 평활 필터의 히스토리 데이터를 저장하는 구조체
    private struct FilterDoubleExponentialData
    {
        public Quaternion RawBoneOrientation; // 원시 관절 방향
        public Quaternion FilteredBoneOrientation; // 필터링된 관절 방향
        public Quaternion Trend; // 관절 방향의 추세
        public uint FrameCount; // 프레임 수
    }
}
