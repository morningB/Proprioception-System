﻿using UnityEngine;
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
    Rigidbody m_BallRb;
    EnvironmentParameters m_ResetParams;

    // 수정부분
    private List<float> angles; // CSV에서 읽은 각도를 저장할 리스트
    private int currentIndex = 0; // 각도를 순차적으로 사용하기 위한 인덱스
    
    private void Start()
    {

        angles = LoadAnglesFromCSV("Assets/Resources/ml.csv");
        Debug.Log("성공적으로 angles에 할당");
    }
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
        // 행동 버퍼에서 2개의 행동 값을 가져옴 (0번은 x축, 1번은 z축 움직임)
        var actionZ = 2f * Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f);
        var actionX = 2f * Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f);

        // z축 회전 제한을 두고 회전 수행
        if ((gameObject.transform.rotation.z < 0.25f && actionZ > 0f) ||
            (gameObject.transform.rotation.z > -0.25f && actionZ < 0f))
        {
            gameObject.transform.Rotate(new Vector3(0, 0, 1), actionZ);
        }

        // x축 회전 제한을 두고 회전 수행
        if ((gameObject.transform.rotation.x < 0.25f && actionX > 0f) ||
            (gameObject.transform.rotation.x > -0.25f && actionX < 0f))
        {
            gameObject.transform.Rotate(new Vector3(1, 0, 0), actionX);
        }
        if (currentIndex >= angles.Count)
        {
            Debug.Log("모든 각도를 사용했습니다. 에피소드를 종료합니다.");
            EndEpisode();
            return; // 이후 코드를 실행하지 않도록 종료
        }
        // 각도 계산 (예: x축을 기준으로)
        float angle = angles[currentIndex];
        currentIndex = (currentIndex + 1) % angles.Count;
        Debug.Log("현재 사용중인 angle : "+angle);
        // 각도가 85도에서 95도 사이에 있을 때 보상 부여하고, 공이 떨어지지 않도록 유지
        if (angle >= 88f && angle <= 92f)
        {
            SetReward(0.1f);  // 목표 각도에 근접하면 보상 0.1점
            Debug.Log("성공");
        }
        // 각도가 95도를 초과하거나 85도 미만일 때 공이 떨어지도록 설정
        else
        {
            SetReward(-0.1f); // 잘못된 각도에 대해 패널티 부여
            // 큐브를 기울여 공이 떨어지게 만듦
            transform.Rotate(new Vector3(10f, 0f, 0f), Space.Self); // x축을 기준으로 10도 기울임
            EndEpisode();  // 에피소드 종료
            Debug.Log("종료: 각도가 범위를 벗어났습니다.");
        }
    }

    public override void OnEpisodeBegin()
    {
        gameObject.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
        gameObject.transform.Rotate(new Vector3(1, 0, 0), Random.Range(-10f, 10f));
        gameObject.transform.Rotate(new Vector3(0, 0, 1), Random.Range(-10f, 10f));
        m_BallRb.velocity = new Vector3(0f, 0f, 0f);
        ball.transform.position = new Vector3(Random.Range(-1.5f, 1.5f), 4f, Random.Range(-1.5f, 1.5f))
            + gameObject.transform.position;
        //Reset the parameters when the Agent is reset.
        SetResetParameters();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = -Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }

    public void SetBall()
    {
        //Set the attributes of the ball by fetching the information from the academy
        m_BallRb.mass = m_ResetParams.GetWithDefault("mass", 1.0f);
        var scale = m_ResetParams.GetWithDefault("scale", 1.0f);
        ball.transform.localScale = new Vector3(scale, scale, scale);
    }

    public void SetResetParameters()
    {
        SetBall();
    }
}
