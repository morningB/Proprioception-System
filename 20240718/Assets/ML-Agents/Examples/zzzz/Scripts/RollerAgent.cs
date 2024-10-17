// ML-Agents ���� ���̺귯���� ����
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class RollerAgent : Agent
{
    // Rigidbody: ������ �������� ó���ϴ� ������Ʈ
    Rigidbody rBody;

    // Unity�� Start() �Լ��� ������Ʈ�� ���۵� �� ȣ��
    void Start()
    {
        // Rigidbody ������Ʈ�� �����ͼ� ������ ����
        rBody = GetComponent<Rigidbody>();
    }

    // ��ǥ���� ���� (Target ������Ʈ�� ��ġ�� �������� �н�)
    public Transform Target;

    // ���Ǽҵ� ���� �� ȣ��, �н��� �ݺ��ϸ鼭 ���Ǽҵ尡 ���� �� �����
    public override void OnEpisodeBegin()
    {
        // ������Ʈ�� �ٴڿ� �������� ��� ������Ʈ�� ��ġ�� �ӵ��� �ʱ�ȭ
        if (this.transform.localPosition.y < 0)
        {
            // ���ӵ��� �̵� �ӵ��� 0���� �����Ͽ� ������ �������� ����
            this.rBody.angularVelocity = Vector3.zero;
            this.rBody.velocity = Vector3.zero;

            // ������Ʈ�� ��ġ�� �ʱ�ȭ (�ٴڿ��� �ٽ� ����)
            this.transform.localPosition = new Vector3(0, 0.5f, 0);
        }

        // ��ǥ��(Target)�� ��ġ�� �����ϰ� �����Ͽ� �� ���Ǽҵ帶�� �ٸ� ��ǥ�� �н��ϵ��� ����
        Target.localPosition = new Vector3(Random.value * 8 - 4, 0.5f, Random.value * 8 - 4);
    }

    // ȯ�濡 ���� ���� �����͸� �����ϴ� �޼��� (������Ʈ�� �н��� �����͸� ����)
    public override void CollectObservations(VectorSensor sensor)
    {
        // ��ǥ��(Target)�� ��ġ ����
        sensor.AddObservation(Target.localPosition);

        // ������Ʈ�� ���� ��ġ ����
        sensor.AddObservation(this.transform.localPosition);

        // ������Ʈ�� �ӵ�(�������� x, z ���� �ӵ�) ����
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.z);
    }

    // �̵� �ӵ��� ������ ���� ���� (���ӵ��� ����� ����)
    public float forceMultiplier = 10;

    // ������Ʈ�� �ൿ�� ó���Ǵ� �޼��� (�� �����Ӹ��� ������Ʈ�� ��� �������� ����)
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // �ൿ ���ۿ��� 2���� �ൿ ���� ������ (0���� x��, 1���� z�� ������)
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.z = actionBuffers.ContinuousActions[1];

        // Rigidbody�� ���� ���� ������Ʈ�� �̵���Ŵ
        rBody.AddForce(controlSignal * forceMultiplier);

        // ������Ʈ�� ��ǥ�� ������ �Ÿ� ���
        float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);

        // ��ǥ���� �������� �� ������ �ְ� ���Ǽҵ� ����
        if (distanceToTarget < 1.42f)
        {
            SetReward(1.0f);  // ��ǥ�� �����ϸ� ���� 1��
            EndEpisode();     // ���Ǽҵ� ����
        }

        // ������Ʈ�� �ٴڿ��� �������� ���Ǽҵ� �����ϰ� ���Ƽ (-3��)
        else if (this.transform.localPosition.y < 0)
        {
            SetReward(-3.0f); // �������� �� ���Ƽ
            EndEpisode();     // ���Ǽҵ� ����
        }
    }

    // Heuristic()�� �������� �ൿ�� �׽�Ʈ�� �� ���� (Ű���� �Է� ���� ���� ���� ����)
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // ������� Ű���� �Է��� ���� �ൿ�� ���� (�¿�/���� �̵�)
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");  // �¿� �̵�
        continuousActionsOut[1] = Input.GetAxis("Vertical");    // ���� �̵�
    }

}
