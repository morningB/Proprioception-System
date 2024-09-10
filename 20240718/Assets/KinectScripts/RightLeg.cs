using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class RightLeg : MonoBehaviour
{
    public Button eyeOpen;
    public Button eyeClose;
    public Button reset;
    public Button result;
    public Button addC;
    // ��ǥ ��ġ ����
    private Vector3 handPosition;
    private Vector3 twohandPosition;

    // scene�� ǥ���� ���� ����
    // ������ �� ����
    public TextMeshProUGUI angleText;
    // �������� �� ����
    public TextMeshProUGUI angleText2;
    // �ǽð� ����
    public TextMeshProUGUI angleText3;

    //���� ���̸� csv�� �����ϱ� ���� ����
    private float openAngle;
    private float closeAngle;

 //   public Image handImage;
    public Image stars;

    // CSV ���� ���
    private string csvFilePath = "Assets/Resources/RL/tes.csv";
    private string angleFile = "Assets/Resources/RL/an.csv";
    private string resultFile = "Assets/Resources/RL/re.csv";

    // ���� ���� �����ϱ� ���� ��ǥ
    private Vector3 previousRightHandPosition;
    private Vector3 previousRightShoulderPosition;
    private Vector3 previousRightAnklePosition;
    // ���� Ƚ��
    private int measurementCount;
    void Start()
    {
        
        openAngle = 0f;
        closeAngle = 0f;

        // �̹��� ��Ȱ��ȭ
        if (stars == null)
            Debug.LogError("Angle Image is not assigned.");
      //  handImage.enabled = false;

        // ��ư Ŭ�� �̺�Ʈ�� ���� ������ �߰�
        eyeOpen.onClick.AddListener(OnEyeOpenClick);
        eyeClose.onClick.AddListener(OnEyeCloseClick);
        reset.onClick.AddListener(OnResetClick);
        result.onClick.AddListener(OnResultClick);
        addC.onClick.AddListener(OnAddC);
        // CSV ���� �ʱ�ȭ
        InitializeCSVFiles();
    }
    private void Update()
    {
        // �̹��� ���̰� �ϱ�
        float currentAngle = getAngle();
        // �ǽð� ���� ����
        angleText3.text = currentAngle.ToString("F2");
        Color originColor = Color.yellow;
        float alpha = Mathf.Clamp01(currentAngle / 45f); // ������ ���� ���� �� ����

        Color newColor = stars.color;

        if (currentAngle > 48f)
        {
            // ������ 45�� ������ ������ ���������� ����
            newColor = Color.red;
        }
        else if(currentAngle <43){
            newColor = Color.blue;
        }
        else
        {
            // ������ 45 �����̸� ���� ���� ����
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
                sw3.WriteLine("result");
            }
        }
    }

    void SaveDataToCSVFilePath(Vector3 rightHandPos, Vector3 rightShoulderPos, Vector3 rightAnklePos)
    {
        // CSV ���Ͽ� ������ �߰�
        using (StreamWriter sw = File.AppendText(csvFilePath))
        {
            sw.WriteLine($"{rightHandPos.x},{rightHandPos.y},{rightHandPos.z},{rightShoulderPos.x},{rightShoulderPos.y},{rightShoulderPos.z},{rightAnklePos.x},{rightAnklePos.y},{rightAnklePos.z}");
        }
    }
    void OnAddC()
    {
        measurementCount++;
        using (StreamWriter sw = File.AppendText(angleFile))
        {
            sw.WriteLine($"{measurementCount}" + "��");
        }
    }
    void SaveDataToAngleFile(float angle)
    {
        // CSV ���Ͽ� ������ �߰�
        using (StreamWriter sw = File.AppendText(angleFile))
        {
            sw.WriteLine($"{angle}");
        }
    }

    void SaveDataTResultFile(string name,float re)
    {
        // CSV ���Ͽ� ������ �߰�
        using (StreamWriter sw = File.AppendText(resultFile))
        {
            sw.WriteLine($"{name},{re}");
        }
    }

    private void OnResultClick()
    {
        // Static ������ ����Ͽ� InputField�� �ؽ�Ʈ ��������
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

    // ��ư Ŭ�� �̺�Ʈ �ڵ鷯
    void OnEyeOpenClick()
    {
        float an = getAngle();
        angleText.text = "Open Angle : " + an.ToString("F2");

        //handPosition = GetRightHandPosition();

        // �̹��� Ȱ��ȭ
        //  handImage.enabled = true;

        // MoveImageToPosition(handPosition);
        SaveDataToAngleFile(an);
        openAngle = an;
    }

    void OnEyeCloseClick()
    {
        float an = getAngle();
        angleText2.text = "Close Angle : " + an.ToString("F2");
        SaveDataToAngleFile(an);
        // twohandPosition = GetRightHandPosition();
        closeAngle = an;
    }

    public float getAngle()
    {
        Vector3 rightHand = GetLeftAnklePosition();
        Vector3 rightShoulder = GetLeftHipPosition();
        Vector3 rightAnkle = GetRightAnklePosition();

        if (rightHand == Vector3.zero || rightShoulder == Vector3.zero || rightAnkle == Vector3.zero)
        {
            Debug.LogWarning("One or more joints are not tracked.");
            return 0.0f;
        }

        // ����� �߽����� �� �����հ� �����߸��� ���� ���
        Vector3 handVector = rightHand - rightShoulder;
        Vector3 ankleVector = rightAnkle - rightShoulder;

        // �� ���� ���� ���� ���
        float angle = Vector3.Angle(handVector, ankleVector);

        SaveDataToCSVFilePath(rightHand, rightShoulder, rightAnkle);

        return angle;
    }
    
    // Ű��Ʈ���� ������ ��ǥ�� �������� �Լ�
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

                    previousRightHandPosition = jointPos;
                    return jointPos;
                }
            }
        }

        Debug.LogWarning("Right hand joint not tracked. Using previous position.");
        return previousRightHandPosition;
    }

    private Vector3 GetLeftHipPosition()
    {
        KinectWrapper.NuiSkeletonPositionIndex joint = KinectWrapper.NuiSkeletonPositionIndex.HipLeft;
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