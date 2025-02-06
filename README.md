
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

### 1. 실시간 자세 감지 및 시각화

- **Kinect** 센서를 통해 환자의 움직임을 감지하고, **Unity**를 통해  **아바타**로 시각화합니다.
- 환자는 자신의 **자세 상태를 실시간 확인**할 수 있습니다.
        
  <img src="https://github.com/user-attachments/assets/1a0b7880-54db-4921-8890-c11c441958d3" width="400" height="200"/>
  
  메인 화면        

  <img src="https://github.com/user-attachments/assets/5a175a30-5f6c-4643-9b86-cb8cd2efe1f9" width="400" height="200"/>
  
### 2. 자세 평가 및 피드백 제공

- 화면의 **별 색상**을 통해 사용자가 자세를 올바르게 수행했는지 여부를 확인합니다.
        
- **자세 각도 판별**할 수 있습니다.
    - 적절한 각도 → ⭐ **노란색**
    - 각도가 낮음 → ⚫ **검은색**
    - 각도가 높음 → 🔴 **빨간색**
- 총 **4가지 자세 측정**할 수 있습니다 (양팔, 양다리).

 
### 3. 자세 비교 및 데이터 분석(Paired T-test)

- 각도 수행 결과를 **그래프**로 비교하며 다른 사람들과의 **비교**할 수 있습니다.
    
    <img src="https://github.com/user-attachments/assets/fa5dd831-0777-4267-88c7-5d3252ed6e39" width="400" height="200"/>

    사람들의 각도 정보가 저장된 csv 파일을 불러와 막대 그래프 형태로 보여줌

  
- **Paired T-test**를 통해 실험 결과 분석:
        - 눈을 뜬 상태와 감은 상태 비교
        - 유의 수준 **p-value = 0.05** 기준
        - **Right Arm(0.03), Left Arm(0.02)** → **유의미한 차이 확인**


    <img src="https://github.com/user-attachments/assets/3b489191-b72c-42c4-8611-b6a769ea3bc4" width="400" height="200"/>
    실험 결과 표
  
### 4. [머신러닝](https://github.com/morningB/Proprioception-System/tree/main/PyCode) 기반 자세 분류

- **Scikit-learn**을 활용하여 **Decision Tree & Random Forest** 모델 적용했습니다.
- 최고 성능 **96%**, 최저 성능 **81%** 기록했습니다.
           
- 눈을 뜨고 진행한 데이터에는 open의 라벨링을 하고 눈을 감고 진행한 데이터에는 close로 라벨링을 진행 후 분류했습니다.
     <img src="https://github.com/user-attachments/assets/eeda066f-0074-4ea9-a9ee-7a5d2b25e2c0" width="400" height="200"/>

     머신러닝 성능 지표
      

---

## 📌성능 최적화 및 코드 개선

성능 측정은 유니티 내부의 Profiler를 사용했습니다.

**1. 문제 발견 및 분석**

- Kinect가 사람의 **Skeleton Point를 정상적으로 인식하지 못하는 경우**, 프레임 **Peak(최대 부하 시 순간적인 성능 저하)** 가 발생하는 문제를 발견했습니다.

<img src="https://github.com/user-attachments/assets/eeda066f-0074-4ea9-a9ee-7a5d2b25e2c0" width="400" height="200"/>

     머신러닝 성능 지표
![image.png](attachment:ab97ef69-2411-4c2c-b02c-0f322bcfe4e0:image.png)

1. **성능 최적화 효과**

| 항목 | **개선 전** | **개선 후** | **효과** |
| --- | --- | --- | --- |
| **최대 부하 실행 시간(`KinectManager.Update()` )** | **30.98ms** | **25.62ms** | **⏬ 17% 단축** |
| **GC 호출 횟수** | **98** | **64** | **⏬ 35% 감소 (메모리 최적화)** |
| **실시간 아바타 제어 시스템 FPS** | **29.3 FPS** | **33.3 FPS** | **⏫ 15% 향상** |
| **최소 부하 CPU 사용량** | **51.7%** | **47.5%** | **⏬ 4.2% 감소 (안정성 증가)** |

### 1. CPU 부하 및 GC 호출 최적화

### **🔹 기존 코드 (비효율적인 `List<uint>` 사용)**

- **문제:** `List.Contains()` 및 `List.Remove()` 호출로 인해 **O(N) 검색** 발생 → 성능 저하.
- **추가 문제:** `new` 연산을 통해 **불필요한 객체 생성** → GC 호출 증가.

```csharp
// 기존 코드: O(N) 검색으로 CPU 부하 및 메모리 할당 발생
List<uint> allUsers = new List<uint>();

for (int i = allUsers.Count - 1; i >= 0; i--) 
{
    uint userId = allUsers[i]; // O(N)
    RemoveUser(userId);
}

```

### **✅ 최적화 코드 (`HashSet<uint>` 사용)**

- **해결:** `HashSet<>` 사용으로 **검색 성능 O(N) → O(1) 개선**.
- **효과:** **CPU 부하 감소 & GC 발생 최소화.**

```csharp
private HashSet<uint> allUsers = new HashSet<uint>();

foreach (uint userId in allUsers)
{
    RemoveUser(userId); // O(1)
}
allUsers.Clear(); // HashSet 비우기
```

### 2. 싱글톤 패턴 적용 (CPU 부하 감소 & 메모리 최적화)

### **🔹 기존 코드 (`FindObjectOfType()` 반복 호출)**

- **문제:** `FindObjectOfType<KinectManager>()`는 씬 내 모든 오브젝트를 탐색 → **CPU 부하 증가**.
- **추가 문제:** `Awake()`에서 중복 인스턴스 생성 가능.

```csharp
public static KinectManager Instance
{
    get
    {
        return FindObjectOfType<KinectManager>(); // 매번 검색 발생 (비효율적)
    }
}
.
.
.
private void Awake()
{
    instance = this;
		DontDestroyOnLoad(gameObject);
}

```

### **✅ 최적화 코드 (싱글톤 패턴 적용)**

- **해결:** 한 번만 `FindObjectOfType<KinectManager>()`을 실행하고 **결과를 캐싱**하여 불필요한 탐색 제거.
- **효과:** **CPU 부하 감소 & 중복 인스턴스 방지.**

```csharp
// 최적화된 싱글톤 적용: 한 번만 검색하고 인스턴스를 캐싱
private static KinectManager instance;

public static KinectManager Instance
{
    get
    {
        if (instance == null)
        {
            instance = FindObjectOfType<KinectManager>();
        }
        return instance;
    }
}
.
.
.
private void Awake()
{
    if (instance == null)
    {
        instance = this;  // 싱글톤 초기화
        DontDestroyOnLoad(gameObject);  // 씬 변경 시 인스턴스 유지
    }
    else
    {
        Destroy(gameObject);  // 중복 인스턴스 방지
    }
}

```

### 요약

- `KinectManager.Update()`의 최대 부하 실행 시간을 **17%** 단축
- GC 호출 횟수를 **35%** 감소 → 불필요한 메모리 할당 제거
- 실시간 아바타 제어 시스템의 FPS를 **15%** 향상
- 최소 부하 상황에서는 CPU 부하를 **4.2%** 감소 → 안정적인 시스템 구현


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
