using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class LeftLeg : MonoBehaviour
{
    public Button eyeOpen;
    public Button eyeClose;
    public Button reset;
    public Button result;
    public Button addC;

    // 좌표 위치 정보
    private Vector3 handPosition;
    private Vector3 twohandPosition;

    // scene에 표현할 각도 정보
    // 눈떴을 때 각도
    public TextMeshProUGUI angleText;
    // 눈감았을 때 각도
    public TextMeshProUGUI angleText2;
    // 실시간 각도
    public TextMeshProUGUI angleText3;


    //각도 차이를 csv에 저장하기 위한 변수
    private float openAngle;
    private float closeAngle;

 //   public Image handImage;
    public Image stars;

    // CSV 파일 경로
    private string csvFilePath = "Assets/Resources/Left Leg/tes.csv";
    private string angleFile = "Assets/Resources/Left Leg/an.csv";
    private string resultFile = "Assets/Resources/re.csv";

    // 측정 횟수
    private int measurementCount;

    // 이전 값을 저장하기 위한 좌표
    private Vector3 previousRightHandPosition;
    private Vector3 previousRightShoulderPosition;
    private Vector3 previousRightAnklePosition;
    void Start()
    {
        measurementCount = 0;
        openAngle = 0f;
        closeAngle = 0f;

        // 이미지 비활성화
        if (stars == null)
            Debug.LogError("Angle Image is not assigned.");
      //  handImage.enabled = false;

        // 버튼 클릭 이벤트에 대한 리스너 추가
        eyeOpen.onClick.AddListener(OnEyeOpenClick);
        eyeClose.onClick.AddListener(OnEyeCloseClick);
        reset.onClick.AddListener(OnResetClick);
        result.onClick.AddListener(OnResultClick);
        addC.onClick.AddListener(OnAddC);

        // CSV 파일 초기화
        InitializeCSVFiles();
    }
    private void Update()
    {
        // 이미지 보이게 하기
        float currentAngle = getAngle();
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
    private void InitializeCSVFiles()
    {
        if (!File.Exists(csvFilePath))
        {
            using (StreamWriter sw = new StreamWriter(csvFilePath))
            {
                sw.WriteLine("RightHandPosX,RightHandPosY,RightHandPosZ,RightShoulderPosX,RightShoulderPosY,RightShoulderPosZ,RightAnklePosX,RightAnklePosY,RightAnklePosZ");
            }
        }
        if (!File.Exists(angleFile))
        {
            using (StreamWriter sw2 = new StreamWriter(angleFile))
            {
                sw2.WriteLine("angle");
            }
        }
        if (!File.Exists(resultFile))
        {
            using (StreamWriter sw3 = new StreamWriter(resultFile))
            {
                sw3.WriteLine("name,result");
            }
        }
    }
    void OnAddC()
    {
        measurementCount++;
        using (StreamWriter sw = File.AppendText(angleFile))
        {
            sw.WriteLine($"{measurementCount}"+ "차");
        }
    }
    void SaveDataToCSVFilePath(Vector3 rightHandPos, Vector3 rightShoulderPos, Vector3 rightAnklePos)
    {
        // CSV 파일에 데이터 추가
        using (StreamWriter sw = File.AppendText(csvFilePath))
        {
            sw.WriteLine($"{rightHandPos.x},{rightHandPos.y},{rightHandPos.z},{rightShoulderPos.x},{rightShoulderPos.y},{rightShoulderPos.z},{rightAnklePos.x},{rightAnklePos.y},{rightAnklePos.z}");
        }
    }

    void SaveDataToAngleFile(float angle)
    {
        // CSV 파일에 데이터 추가
        using (StreamWriter sw = File.AppendText(angleFile))
        {
            sw.WriteLine($"{angle}");
        }
    }

    void SaveDataTResultFile(string name,float re)
    {
        // CSV 파일에 데이터 추가
        using (StreamWriter sw = File.AppendText(resultFile))
        {
            sw.WriteLine($"{name},{re}");
        }
    }

    private void OnResultClick()
    {
        // Static 변수를 사용하여 InputField의 텍스트 가져오기
        string inputText = InputName.inputText;
        if (inputText == null || inputText == "")
        {
            inputText = "apfhd";
        }
        float re = Mathf.Abs(openAngle - closeAngle);
        SaveDataTResultFile(inputText, re);

        if (openAngle != 0 && closeAngle != 0)
        {

        }


        Debug.Log("Input text from InputName script: " + inputText);
    }

    private void OnResetClick()
    {
       // handImage.enabled = false;
        angleText.text = "";
        angleText2.text = "";
        Color a = stars.color;
        a.a = 0f;
        stars.color = a;
    }

    // 버튼 클릭 이벤트 핸들러
    void OnEyeOpenClick()
    {
        float an = getAngle();
        angleText.text = "Open Angle : " + an.ToString("F2");

        //handPosition = GetRightHandPosition();

        // 이미지 활성화
        //  handImage.enabled = true;

        // MoveImageToPosition(handPosition);
        SaveDataToAngleFile(an);
         openAngle = an;
    }

    void OnEyeCloseClick()
    {
        float an = getAngle();
        angleText2.text = "Close Angle : " + an.ToString("F2");

        // twohandPosition = GetRightHandPosition();
        SaveDataToAngleFile(an);
         closeAngle = an;
    }

    public float getAngle()
    {
        Vector3 rightHand = GetRightAnklePosition();
        Vector3 rightShoulder = GetRightHipPosition();
        Vector3 rightAnkle = GetLeftAnklePosition();

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

    // 키넥트에서 오른손 좌표를 가져오는 함수
    private Vector3 GetRightHipPosition()
    {
        KinectWrapper.NuiSkeletonPositionIndex joint = KinectWrapper.NuiSkeletonPositionIndex.HipRight;
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

    private Vector3 GetLeftAnklePosition()
    {
        KinectWrapper.NuiSkeletonPositionIndex joint = KinectWrapper.NuiSkeletonPositionIndex.AnkleLeft;
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
                  
                    previousRightShoulderPosition = jointPos;
                    return jointPos;
                }
            }
        }

        Debug.LogWarning("Right shoulder joint not tracked. Using previous position.");
        return previousRightShoulderPosition;
    }

    private Vector3 GetRightAnklePosition()
    {
        KinectWrapper.NuiSkeletonPositionIndex joint = KinectWrapper.NuiSkeletonPositionIndex.AnkleRight;
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
                 //   jointPos.x += 3.0f;
                  //  jointPos.y += 0.47f;
                    previousRightAnklePosition = jointPos;
                    return jointPos;
                }
            }
        }

        Debug.LogWarning("Right ankle joint not tracked. Using previous position.");
        return previousRightAnklePosition;
    }

    /*
    void MoveImageToPosition(Vector3 position)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(position);

        RectTransform rectTransform = handImage.GetComponent<RectTransform>();
        rectTransform.position = screenPos;
    }*/
}