// ML-Agents 관련 라이브러리들 포함
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class RollerAgent : Agent
{
    // Rigidbody: 물리적 움직임을 처리하는 컴포넌트
    Rigidbody rBody;

    // Unity의 Start() 함수는 에이전트가 시작될 때 호출
    void Start()
    {
        // Rigidbody 컴포넌트를 가져와서 변수에 저장
        rBody = GetComponent<Rigidbody>();
    }

    // 목표물을 설정 (Target 오브젝트의 위치를 기준으로 학습)
    public Transform Target;

    // 에피소드 시작 시 호출, 학습을 반복하면서 에피소드가 여러 번 실행됨
    public override void OnEpisodeBegin()
    {
        // 에이전트가 바닥에 떨어졌을 경우 에이전트의 위치와 속도를 초기화
        if (this.transform.localPosition.y < 0)
        {
            // 각속도와 이동 속도를 0으로 설정하여 물리적 움직임을 멈춤
            this.rBody.angularVelocity = Vector3.zero;
            this.rBody.velocity = Vector3.zero;

            // 에이전트의 위치를 초기화 (바닥에서 다시 시작)
            this.transform.localPosition = new Vector3(0, 0.5f, 0);
        }

        // 목표물(Target)의 위치를 랜덤하게 설정하여 매 에피소드마다 다른 목표로 학습하도록 설정
        Target.localPosition = new Vector3(Random.value * 8 - 4, 0.5f, Random.value * 8 - 4);
    }

    // 환경에 대한 관찰 데이터를 수집하는 메서드 (에이전트가 학습할 데이터를 제공)
    public override void CollectObservations(VectorSensor sensor)
    {
        // 목표물(Target)의 위치 관찰
        sensor.AddObservation(Target.localPosition);

        // 에이전트의 현재 위치 관찰
        sensor.AddObservation(this.transform.localPosition);

        // 에이전트의 속도(물리적인 x, z 방향 속도) 관찰
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.z);
    }

    // 이동 속도에 곱해질 힘의 비율 (가속도와 비슷한 역할)
    public float forceMultiplier = 10;

    // 에이전트의 행동이 처리되는 메서드 (매 프레임마다 에이전트가 어떻게 움직일지 결정)
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // 행동 버퍼에서 2개의 행동 값을 가져옴 (0번은 x축, 1번은 z축 움직임)
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.z = actionBuffers.ContinuousActions[1];

        // Rigidbody에 힘을 가해 에이전트를 이동시킴
        rBody.AddForce(controlSignal * forceMultiplier);

        // 에이전트와 목표물 사이의 거리 계산
        float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);

        // 목표물에 도착했을 때 보상을 주고 에피소드 종료
        if (distanceToTarget < 1.42f)
        {
            SetReward(1.0f);  // 목표에 도달하면 보상 1점
            EndEpisode();     // 에피소드 종료
        }

        // 에이전트가 바닥에서 떨어지면 에피소드 종료하고 페널티 (-3점)
        else if (this.transform.localPosition.y < 0)
        {
            SetReward(-3.0f); // 떨어졌을 때 페널티
            EndEpisode();     // 에피소드 종료
        }
    }

    // Heuristic()은 수동으로 행동을 테스트할 때 사용됨 (키보드 입력 등을 통한 수동 제어)
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // 사용자의 키보드 입력을 통해 행동을 결정 (좌우/상하 이동)
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");  // 좌우 이동
        continuousActionsOut[1] = Input.GetAxis("Vertical");    // 상하 이동
    }

}
