using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using System.IO;

public class Ball3DAgent : Agent
{
    [Header("Specific to Ball3D")]
    public GameObject ball;
    [Tooltip("Whether to use vector observation. This option should be checked " +
        "in 3DBall scene, and unchecked in Visual3DBall scene. ")]
    public bool useVecObs;
    private Rigidbody m_BallRb;
    private EnvironmentParameters m_ResetParams;

    // 수정 부분
    private List<float> angles; // CSV에서 읽은 각도를 저장할 리스트
    private int currentIndex = 0; // 각도를 순차적으로 사용하기 위한 인덱스


    // CSV 파일에서 각도 값을 읽어오는 함수
    private List<float> LoadAnglesFromCSV(string filePath)
    {
        List<float> loadedAngles = new List<float>();
        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                // CSV 파일에서 각도 값을 읽고 리스트에 추가
                if (float.TryParse(line, out float angle))
                {
                    loadedAngles.Add(angle);
                }
            }
        }
        return loadedAngles;
    }

    public override void Initialize()
    {
        // CSV 파일에서 각도 값을 불러옴
        angles = LoadAnglesFromCSV("Assets/Resources/ml.csv");
        Debug.Log("성공적으로 angles에 할당");
        m_BallRb = ball.GetComponent<Rigidbody>();
        m_ResetParams = Academy.Instance.EnvironmentParameters;
        SetResetParameters();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (useVecObs)
        {
            sensor.AddObservation(gameObject.transform.rotation.z);
            sensor.AddObservation(gameObject.transform.rotation.x);
            sensor.AddObservation(ball.transform.position - gameObject.transform.position);
            sensor.AddObservation(m_BallRb.velocity);
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // 모든 각도 데이터를 사용한 경우 에피소드 종료
        if (currentIndex >= angles.Count)
        {
            Debug.Log("모든 각도를 사용했습니다. 에피소드를 종료합니다.");
            EndEpisode();
            return;
        }

        // 현재 각도를 가져옴
        float targetAngle = angles[currentIndex];
        currentIndex++;
        Debug.Log("현재 사용중인 angle : " + targetAngle);

        // 현재 큐브의 x축 회전을 가져옴
        float currentXRotation = gameObject.transform.eulerAngles.x;

        // 목표 각도와 현재 각도의 차이를 계산하고, 비율을 더 크게 조절
        float adjustedXRotation = Mathf.Lerp(currentXRotation, targetAngle, 1f); // 보간 비율을 0.2로 증가
        adjustedXRotation = Mathf.Clamp(adjustedXRotation, 88f, 92f); // 88도에서 92도 사이로 제한

        // 회전을 적용하여 큐브를 평평하게 유지
        gameObject.transform.rotation = Quaternion.Euler(adjustedXRotation, 0f, 0f);

        // 각도가 90도에 가까울수록 보상을 부여
        float angleDifference = Mathf.Abs(adjustedXRotation - 90f);
        float reward = Mathf.Max(0, 1f - (angleDifference / 2f)); // 각도 차이에 따라 보상을 계산
        SetReward(reward);

        // 각도가 범위를 벗어날 경우 에피소드 종료
        if (adjustedXRotation < 88f || adjustedXRotation > 92f)
        {
            SetReward(-1f); // 잘못된 각도에 대해 큰 패널티 부여
            EndEpisode();  // 에피소드 종료
            Debug.Log("종료: 각도가 범위를 벗어났습니다.");
        }
    }



    public override void OnEpisodeBegin()
    {
        // 에피소드 시작 시 초기화
        gameObject.transform.rotation = Quaternion.identity;
        gameObject.transform.Rotate(new Vector3(1, 0, 0), Random.Range(-10f, 10f));
        gameObject.transform.Rotate(new Vector3(0, 0, 1), Random.Range(-10f, 10f));
        m_BallRb.velocity = Vector3.zero;
        ball.transform.position = new Vector3(Random.Range(-1.5f, 1.5f), 4f, Random.Range(-1.5f, 1.5f))
            + gameObject.transform.position;
        SetResetParameters();
        currentIndex = 0; // 새로운 에피소드 시작 시 인덱스를 초기화
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = -Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }

    public void SetBall()
    {
        // 공의 속성 설정
        m_BallRb.mass = m_ResetParams.GetWithDefault("mass", 1.0f);
        var scale = m_ResetParams.GetWithDefault("scale", 1.0f);
        ball.transform.localScale = new Vector3(scale, scale, scale);
    }

    public void SetResetParameters()
    {
        SetBall();
    }
}
