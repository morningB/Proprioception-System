using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Holt 이중 지수 평활 필터의 구현입니다. 
/// 이중 지수는 곡선을 부드럽게 하고 예측하며, 노이즈 제거 기능도 포함됩니다.
/// </summary>
public class TrackingStateFilter
{
    // 필터의 과거 데이터
    private FilterDoubleExponentialData[] history;

    // 필터에 사용되는 변환 평활 매개변수
    private KinectWrapper.NuiTransformSmoothParameters smoothParameters;

    // 필터 매개변수가 초기화되었는지 여부
    private bool init;

    /// 클래스의 새 인스턴스를 초기화합니다.
    public TrackingStateFilter()
    {
        this.init = false;
    }

    // 기본 TransformSmoothParameters를 사용하여 필터를 초기화합니다.
    public void Init()
    {
        // 기본값으로 초기화
        //this.Init(0.25f, 0.25f, 0.25f, 0.03f, 0.05f);
        this.Init(0.5f, 0.5f, 0.5f, 0.05f, 0.04f);
    }

    /// <summary>
    /// 수동으로 지정된 TransformSmoothParameters를 사용하여 필터를 초기화합니다.
    /// </summary>
    /// <param name="smoothingValue">부드러움 = [0..1], 값이 낮을수록 원시 데이터와 더 가깝고 더 노이즈가 많습니다.</param>
    /// <param name="correctionValue">보정 = [0..1], 값이 높을수록 더 빠르게 보정되고 더 반응적입니다.</param>
    /// <param name="predictionValue">예측 = [0..n], 미래의 몇 프레임을 예측할 것인지 결정합니다.</param>
    /// <param name="jitterRadiusValue">진동 반경 = m 단위로 노이즈를 정의하는 편차 거리입니다.</param>
    /// <param name="maxDeviationRadiusValue">최대 편차 = 필터링된 위치가 원시 데이터에서 벗어날 수 있는 최대 거리입니다.</param>
    public void Init(float smoothingValue, float correctionValue, float predictionValue, float jitterRadiusValue, float maxDeviationRadiusValue)
    {
        this.smoothParameters = new KinectWrapper.NuiTransformSmoothParameters();

        this.smoothParameters.fSmoothing = smoothingValue;                   // 부드럽게 처리하는 정도. 너무 높으면 지연 발생
        this.smoothParameters.fCorrection = correctionValue;                 // 예측에서 복귀하는 정도. 너무 높으면 탄력성이 증가
        this.smoothParameters.fPrediction = predictionValue;                 // 미래 예측 프레임 수. 너무 높으면 과도한 예측 발생
        this.smoothParameters.fJitterRadius = jitterRadiusValue;             // 진동을 제거하는 반경 크기. 너무 높으면 과도한 평활화
        this.smoothParameters.fMaxDeviationRadius = maxDeviationRadiusValue; // 최대 예측 반경 크기. 너무 높으면 노이즈 데이터로 돌아갈 수 있음

        this.Reset();
        this.init = true;
    }

    // 지정된 TransformSmoothParameters로 필터를 초기화합니다.
    public void Init(KinectWrapper.NuiTransformSmoothParameters smoothingParameters)
    {
        this.smoothParameters = smoothingParameters;
		
        this.Reset();
        this.init = true;
    }

    // Resets the filter to default values.
    public void Reset()
    {
        this.history = new FilterDoubleExponentialData[(int)KinectWrapper.NuiSkeletonPositionIndex.Count];
    }

    // 새 프레임 데이터를 사용해 필터를 업데이트하고 부드럽게 처리합니다.
    public void UpdateFilter(ref KinectWrapper.NuiSkeletonData skeleton)
    {
        if (skeleton.eTrackingState != KinectWrapper.NuiSkeletonTrackingState.SkeletonTracked)
        {
            return;
        }

        if (this.init == false)
        {
            this.Init();    // 기본 매개변수로 초기화  
        }

        // 0으로 나누는 오류 방지. 0.1mm의 epsilon 값 사용
        smoothParameters.fJitterRadius = Math.Max(0.0001f, smoothParameters.fJitterRadius);

		int jointsCount = (int)KinectWrapper.NuiSkeletonPositionIndex.Count;
        for(int jointIndex = 0; jointIndex < jointsCount; jointIndex++)
        {
            FilterJoint(ref skeleton, jointIndex, ref smoothParameters);
        }
    }

    // 특정 관절에 대한 필터를 업데이트합니다.
    protected void FilterJoint(ref KinectWrapper.NuiSkeletonData skeleton, int jointIndex, ref KinectWrapper.NuiTransformSmoothParameters smoothingParameters)
    {
        float filteredState;
        float trend;
        float diffVal;

        float rawState = (float)skeleton.eSkeletonPositionTrackingState[jointIndex];
        float prevFilteredState = history[jointIndex].FilteredState;
        float prevTrend = history[jointIndex].Trend;
        float prevRawState = history[jointIndex].RawState;

        // 관절 필터링 로직 수행
        if (rawState == 0f)
        {
            history[jointIndex].FrameCount = 0;
        }

        // Initial start values
        if (history[jointIndex].FrameCount == 0)
        {
            filteredState = rawState;
            trend = 0f;
        }
        else if (this.history[jointIndex].FrameCount == 1)
        {
            filteredState = (rawState + prevRawState) * 0.5f;
            diffVal = filteredState - prevFilteredState;
            trend = (diffVal * smoothingParameters.fCorrection) + (prevTrend * (1.0f - smoothingParameters.fCorrection));
        }
        else
        {              

			
            filteredState = rawState;


            filteredState = (filteredState * (1.0f - smoothingParameters.fSmoothing)) + ((prevFilteredState + prevTrend) * smoothingParameters.fSmoothing);

            diffVal = filteredState - prevFilteredState;
            trend = (diffVal * smoothingParameters.fCorrection) + (prevTrend * (1.0f - smoothingParameters.fCorrection));
        }      


        float predictedState = filteredState + (trend * smoothingParameters.fPrediction);


        diffVal = predictedState - rawState;

        if (diffVal > smoothingParameters.fMaxDeviationRadius)
        {
            predictedState = (predictedState * (smoothingParameters.fMaxDeviationRadius / diffVal)) + (rawState * (1.0f - (smoothingParameters.fMaxDeviationRadius / diffVal)));
        }

        // Save the data from this frame
        history[jointIndex].RawState = rawState;
        history[jointIndex].FilteredState = filteredState;
        history[jointIndex].Trend = trend;
        history[jointIndex].FrameCount++;
        
        // Set the filtered data back into the joint
		skeleton.eSkeletonPositionTrackingState[jointIndex] = (KinectWrapper.NuiSkeletonPositionTrackingState)(predictedState + 0.5f);
    }


    // 필터 데이터 구조체
    private struct FilterDoubleExponentialData
    {
        public float RawState;        // 원시 상태 데이터
        public float FilteredState;  // 필터링된 상태 데이터
        public float Trend;          // 추세 데이터
        public uint FrameCount;      // 프레임 카운트
    }
}
