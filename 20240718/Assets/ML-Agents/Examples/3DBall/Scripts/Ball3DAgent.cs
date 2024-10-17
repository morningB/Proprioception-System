using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;

public class Ball3DAgent : Agent
{
    [Header("Specific to Ball3D")]
    public GameObject ball;
    [Tooltip("Whether to use vector observation. This option should be checked " +
        "in 3DBall scene, and unchecked in Visual3DBall scene. ")]
    public bool useVecObs;
    Rigidbody m_BallRb;
    EnvironmentParameters m_ResetParams;

    private RightHand RightHandScript = new RightHand();

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
        // �ൿ ���ۿ��� 2���� �ൿ ���� ������ (0���� x��, 1���� z�� ������)
        var actionZ = 2f * Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f);
        var actionX = 2f * Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f);

        // z�� ȸ�� ������ �ΰ� ȸ�� ����
        if ((gameObject.transform.rotation.z < 0.25f && actionZ > 0f) ||
            (gameObject.transform.rotation.z > -0.25f && actionZ < 0f))
        {
            gameObject.transform.Rotate(new Vector3(0, 0, 1), actionZ);
        }

        // x�� ȸ�� ������ �ΰ� ȸ�� ����
        if ((gameObject.transform.rotation.x < 0.25f && actionX > 0f) ||
            (gameObject.transform.rotation.x > -0.25f && actionX < 0f))
        {
            gameObject.transform.Rotate(new Vector3(1, 0, 0), actionX);
        }

        // ���� ��� (��: x���� ��������)
        float angle = 190f;

        // ������ 85������ 95�� ���̿� ���� �� ���� �ο��ϰ�, ���� �������� �ʵ��� ����
        if (angle >= 85f && angle <= 95f)
        {
            SetReward(0.1f);  // ��ǥ ������ �����ϸ� ���� 0.1��
            Debug.Log("����");
        }
        // ������ 95���� �ʰ��ϰų� 85�� �̸��� �� ���� ���������� ����
        else
        {
            SetReward(-0.1f); // �߸��� ������ ���� �г�Ƽ �ο�

            // ť�긦 ��￩ ���� �������� ����
            transform.Rotate(new Vector3(10f, 0f, 0f), Space.Self); // x���� �������� 10�� �����

            EndEpisode();  // ���Ǽҵ� ����
            Debug.Log("����: ������ ������ ������ϴ�.");
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
