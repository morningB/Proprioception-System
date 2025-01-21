
## 프로젝트 개요

- 뇌졸중 및 재활 치료 환자의 **고유수용감각 평가 방식**은 주관적인 관찰에 의존하여 **정확도가 떨어집니다**.
- 이를 개선하고자 **Kinect와 Unity**를 활용한 **실시간 고유수용감각 평가 시스템**을 개발했습니다.
- **20개의 Skeleton point**를 활용하여 인체의 움직임을 정량적으로 분석하고 실시간으로 모니터링합니다.
- 사용자의 자세를 측정하고 색상 변화로 즉각적인 피드백을 제공하여, 재활 진행 상태를 직관적으로 파악할 수 있습니다.
- **Scikit-learn**을 활용하여 4가지 자세 데이터에 대한 머신러닝 분석을 수행했습니다.
- 이를 통해 환자들이 자신의 재활 상태를 시각적으로 확인하며 치료에 대한 **몰입도를 높일** 수 있게 되었습니다.

담당 인원 : 1명

---

## 주요 기능 및 기여한 역할

- **Kinect** 센서를 통해 환자의 움직임을 감지하고, **Unity**를 통해 **접근성을** 향상시키고, **아바타**로 시각화하는 등 환자가 자신의 상태를 실시간으로 확인
        
  <img src="https://github.com/user-attachments/assets/1a0b7880-54db-4921-8890-c11c441958d3" width="400" height="200"/>
  
  메인 화면        

  <img src="https://github.com/user-attachments/assets/5a175a30-5f6c-4643-9b86-cb8cd2efe1f9" width="400" height="200"/>
  
  초기 화면
- 화면의 **별의 상태**에 따라서 자신의 자세 수행이 잘 되었는지 아닌지를 확인
        
    - 자세 수행은 **각도로 판별**하며, 자세를 적절한 각도로 수행했으면 별이 노란색으로, 각도가 낮으면 **검은색**으로, 높으면 빨간색으로 변화합니다.
    - 또한, 양 팔과 양 다리에 대해 측정하여 총 **4가지 자세**를 측정할 수 있습니다.
    - 자세를 수행하면 화면 아래 버튼을 통해 데이터를 기록할 수 있으며, 데이터는 **csv파일**의 형태로 기록됩니다.
    
         <img src="https://github.com/user-attachments/assets/75e66d21-01b2-4b9a-9d33-df7968457ca9" width="400" height="200"/>

         csv 파일로 저장되는 데이터
 
- 각도 수행 결과를 **그래프**로 비교하며 다른 사람들과의 **비교**
    
    <img src="https://github.com/user-attachments/assets/fa5dd831-0777-4267-88c7-5d3252ed6e39" width="400" height="200"/>

    사람들의 각도 정보가 저장된 csv 파일을 불러와 막대 그래프 형태로 보여줌

  
- **Paired T-test**를 통해 결과 분석
    - 눈 뜨기와 감기를 기준으로 **차이가 있는지**를 분석
    - 귀무 가설로는 둘 사이에는 **차이가 없다고 가정**
    - 유의 수준(p-value)는 **0.05**로 설정
    - Right Arm과 Left Arm에서 눈을 뜬 상태와 눈을 뜨고 무게를 추가하여 부하를 가했을 때, p-value 값이 각각 **0.03**과 **0.02**로 유의미한 차이를 보임

        <img src="https://github.com/user-attachments/assets/3b489191-b72c-42c4-8611-b6a769ea3bc4" width="400" height="200"/>

        실험 결과 표
- **Python의 Scikit-learn**을 사용하여 **Decision Tree**와 **Random Forest**로 분류했으며, 최고 성능은 **96%**, 최저는 **81%** 를 기록
            
    - [머신러닝 코드 깃허브](https://github.com/morningB/Proprioception-System/tree/main/PyCode)
    - 머신 러닝 모델 성능 지표
    - 눈을 뜨고 진행한 데이터에는 **open의 라벨링**을 하고 눈을 감고 진행한 데이터에는 **close로 라벨링**을 진행 후 분류

      <img src="https://github.com/user-attachments/assets/eeda066f-0074-4ea9-a9ee-7a5d2b25e2c0" width="400" height="200"/>

      머신러닝 성능 지표
      

---

## 코드 설명

- **3D 공간 변화 및 Skeleton Rendering**
    - 자세를 측정하기 위해 벡터로 변환하여 각도를 계산하는 코드
    
    ```csharp
    public float getAngle()
    {
        Vector3 rightHand = GetRightHandPosition();
        Vector3 rightShoulder = GetRightShoulderPosition();
        Vector3 rightAnkle = GetRightAnklePosition();
    
        if (rightHand == Vector3.zero || rightShoulder == Vector3.zero || rightAnkle == Vector3.zero)
        {
            Debug.LogWarning("One or more joints are not tracked.");
            return 0.0f;
        }
    
        // 어깨를 중심으로 한 오른손과 오른발목의 벡터 계산
        Vector3 handVector = rightHand - rightShoulder;
        Vector3 ankleVector = rightAnkle - rightShoulder;
    
        // 두 벡터 간의 각도 계산
        float angle = Vector3.Angle(handVector, ankleVector);
    
        SaveDataToCSVFilePath(rightHand, rightShoulder, rightAnkle);
    
        return angle;
    }
    
    ```
    
    - rightHand와 같이 해당 Skeleton point를 가져오는 함수
    
    ```csharp
    private Vector3 GetRightHandPosition()
    {
        KinectWrapper.NuiSkeletonPositionIndex joint = KinectWrapper.NuiSkeletonPositionIndex.HandRight;
        KinectManager manager = KinectManager.Instance;
        Vector3 jointPos = Vector3.zero;
    
        if (manager && manager.IsInitialized())
        {
            if (manager.IsUserDetected())
            {
                uint userId = manager.GetPlayer1ID();
    
                if (manager.IsJointTracked(userId, (int)joint))
                {
                    jointPos = manager.GetJointPosition(userId, (int)joint);
                    
                    previousRightHandPosition = jointPos;
                    return jointPos;
                }
            }
        }
    
        Debug.LogWarning("Right hand joint not tracked. Using previous position.");
        return previousRightHandPosition;
    }
    ```
    
- **시각화 시스템 개선**
    - Kinect의 Skeleton 데이터를 바탕으로 관절들 간의 선을 **Texture2D**를 이용하여 실시간으로 표현
    
    ```csharp
    private void DrawSkeleton(Texture2D aTexture, ref KinectWrapper.NuiSkeletonData skeletonData, ref bool[] playerJointsTracked)
    {
        int jointsCount = (int)KinectWrapper.NuiSkeletonPositionIndex.Count;
        
        for (int i = 0; i < jointsCount; i++)
        {   // 부모 관절을 찾음
            int parent = KinectWrapper.GetSkeletonJointParent(i);  
            // 양쪽 관절이 추적되고 있으면
            if (playerJointsTracked[i] && playerJointsTracked[parent])  
            {
                Vector3 posParent = KinectWrapper.MapSkeletonPointToDepthPoint(skeletonData.SkeletonPositions[parent]);  // 부모 관절의 위치
                Vector3 posJoint = KinectWrapper.MapSkeletonPointToDepthPoint(skeletonData.SkeletonPositions[i]);  // 자식 관절의 위치
    
                // 관절이 추적되고 있으면, 선 색을 빨간색으로 설정하고, 그렇지 않으면 노란색으로 설정하여 선을 그립니다.
                DrawLine(aTexture, (int)posParent.x, (int)posParent.y, (int)posJoint.x, (int)posJoint.y, Color.yellow);  // 텍스처에 선을 그림
            }
        }
    }
    
    ```
    
    - 두 점 사이를 그리기 위해 **Bresenham 알고리즘**을 사용
    - 이 알고리즘은 선의 **기울기와 방향**에 따라 각 픽셀을 텍스처에 찍어줌
    
    ```csharp
    private void DrawLine(Texture2D a_Texture, int x1, int y1, int x2, int y2, Color a_Color)
    {
        int width = a_Texture.width;
        int height = a_Texture.height;
        
        int dy = y2 - y1;
        int dx = x2 - x1;
     
        int stepy = 1;
        if (dy < 0) 
        {
            dy = -dy; 
            stepy = -1;
        }
        
        int stepx = 1;
        if (dx < 0) 
        {
            dx = -dx; 
            stepx = -1;
        }
        
        dy <<= 1;
        dx <<= 1;
        
        if (x1 >= 0 && x1 < width && y1 >= 0 && y1 < height)
            for(int x = -1; x <= 1; x++)
                for(int y = -1; y <= 1; y++)
                    a_Texture.SetPixel(x1 + x, y1 + y, a_Color);
    
        if (dx > dy) 
        {
            int fraction = dy - (dx >> 1);
            
            while (x1 != x2) 
            {
                if (fraction >= 0) 
                {
                    y1 += stepy;
                    fraction -= dx;
                }
                
                x1 += stepx;
                fraction += dy;
                
                if(x1 >= 0 && x1 < width && y1 >= 0 && y1 < height)
                    for(int x = -1; x <= 1; x++)
                        for(int y = -1; y <= 1; y++)
                            a_Texture.SetPixel(x1 + x, y1 + y, a_Color);
            }
        }
        else 
        {
            int fraction = dx - (dy >> 1);
            
            while (y1 != y2) 
            {
                if (fraction >= 0) 
                {
                    x1 += stepx;
                    fraction -= dy;
                }
                
                y1 += stepy;
                fraction += dx;
                
                if(x1 >= 0 && x1 < width && y1 >= 0 && y1 < height)
                    for(int x = -1; x <= 1; x++)
                        for(int y = -1; y <= 1; y++)
                            a_Texture.SetPixel(x1 + x, y1 + y, a_Color);
            }
        }
    }
    
    ```
    
- **필터 사용하여 안정성을 강화**
    - **Holt의 이중 지수 평활 필터** (Double Exponential Smoothing Filter)를 사용하여 **스켈레톤 트래킹** 데이터를 부드럽게 하고, 예측을 통해 **지연**을 줄이며, **노이즈 제거** 기능을 제공
        - [**TrackingStateFilter**](https://github.com/morningB/Proprioception-System/blob/main/20240718/Assets/KinectScripts/Filters/TrackingStateFilter.cs)
    - Kinect 센서를 사용하여 사람의 관절 **회전 값을 제한**하는 기능을 제공
    - 각 관절의 **회전 범위를 설정**해 관절이 가능한 범위 내에서만 움직일 수 있도록 제한
        - [**BoneOrientationsConstraint**](https://github.com/morningB/Proprioception-System/blob/main/20240718/Assets/KinectScripts/Filters/BoneOrientationsConstraint.cs)

---

## 트러블 슈팅1️⃣

### 🥵문제 배경

- **문제**
    - 실시간 Skeleton point 인식 중 일부 값이 인식되지 않거나 비정상적으로 튀는 현상이 발생했습니다.
    - 이는 움직임을 정확하게 인식하지 못하는 치명적인 문제입니다.

## 트러블 슈팅2️⃣

### 🥵문제 배경

- **문제**
    - 다양한 머신러닝 모델을 사용하여 데이터를 분류 했었지만, 성능 지표가 너무 낮은 것을 확인했습니다.
    - 처음 라벨링을 진행한 후 머신 러닝 성능 지표입니다. 최고 성능은 **0.95**이지만, 최저 성능은 **0.17**로 매우 낮은 지표였습니다.
     
        
- **원인**
    1. 12명의 데이터를 활용하였기에 매우 적은 데이터였습니다.
    2. 데이터 전처리가 잘 못 되어 머신러닝 모델에 사용하기에 적절하지 않은 데이터였습니다.

### 😁해결 방법

- **해결**
    - 데이터 전처리를 다시 진행하고, 라벨링 기준을 다시 잡으며 성능을 확인하였습니다.

---

## 성과

- **활용 가능성**
    - **시각화**를 통해 실제 사람 **12명의 데이터**를 수집.
    - 환자들의 재활 치료에 대한 참여도를 높이고, **물리 치료에 종사**하시는 교수님께 문의를 드려 실제 임상 현장에서 사용할 수 있는 도구로 발전할 가능성을 보여줌
- **학회 발표**
    - 2024년도 **의료정보공학회**, 2024 **IEEE BHI** 학회 포스터 발표함
- **학술제 발표**
    - 순천향대학교 제 1회 **융합대학 학술제 장려상 수상**
          
        
- **SW 등록**
- **논문 투고 (under reivew)**

---

## 사용 기술

- **C#**: Kinect 데이터 수집 및 분석을 위한 핵심 언어로 사용
- **Unity**: 환자의 움직임을 시각적으로 피드백하는 3D 시뮬레이션 구현
- **Python, Scikit-learn**: 머신러닝 모델을 통한 움직임 데이터 분석 및 분류
- **Git**: 버전 관리를 위해 사용
- **Excel**: 데이터 분석 및 유의성 검사 도구로 사용

---
[노션 포트폴리오 정리](https://www.notion.so/Kinect-Unity-aae3568ed0554c19b9d0819bcbd39f41?pvs=4)
