using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text; 


[RequireComponent(typeof(Animator))]
public class AvatarController : MonoBehaviour
{
    // 캐릭터가 (플레이어를 향해) 반대로 움직이도록 하는 bool 값. 기본값은 false.
    public bool mirroredMovement = false;

    // 아바타가 수직 방향으로 이동할 수 있는지 여부를 결정하는 bool 값.
    public bool verticalMovement = false;

    // 아바타가 씬을 가로질러 이동하는 비율. 이 비율은 이동 속도에 곱해진다 (.001f, 즉 유니티의 프레임 속도로 나누는 것).
    protected int moveRate = 1;

    // Slerp 부드러움 계수
    public float smoothFactor = 5f;

    // 오프셋 노드를 사용자가 제공한 센서 좌표에 맞게 다시 위치시킬지 여부.
    public bool offsetRelativeToSensor = false;

    // 몸체 루트 노드
    protected Transform bodyRoot;

    // 모델을 회전시키려면 필요한 변수.
    protected GameObject offsetNode;

    // 모든 뼈를 저장할 변수. 초기 회전 값과 동일한 크기로 초기화됨.
    protected Transform[] bones;

    // Kinect 추적이 시작될 때 뼈들의 회전 값.
    protected Quaternion[] initialRotations;
    protected Quaternion[] initialLocalRotations;

    // 변환의 초기 위치 및 회전
    protected Vector3 initialPosition;
    protected Quaternion initialRotation;

    // 캐릭터 위치 보정을 위한 오프셋 변수.
    protected bool offsetCalibrated = false;
    protected float xOffset, yOffset, zOffset;

    // KinectManager의 개인 인스턴스
    protected KinectManager kinectManager;

    // transform 캐싱은 성능 향상을 위해 사용. Unity는 매번 GetComponent<Transform>()를 호출하기 때문.
    private Transform _transformCache;
    public new Transform transform
    {
		get
		{
			if (!_transformCache) 
				_transformCache = base.transform;
			
			return _transformCache;
		}
	}

    public void Awake()
    {
        // 이중 시작 여부 확인
        if (bones != null)
            return;

        // 뼈 배열 초기화
        bones = new Transform[22];

        // 뼈들의 초기 회전 및 방향 설정.
        initialRotations = new Quaternion[bones.Length];
        initialLocalRotations = new Quaternion[bones.Length];

        // Kinect가 추적하는 포인트에 뼈를 매핑
        MapBones();

        // 초기 뼈 회전값 가져오기
        GetInitialRotations();
        
    }

    // 매 프레임마다 아바타를 업데이트
    public void UpdateAvatar(uint UserID)
    {
        if (!transform.gameObject.activeInHierarchy)
            return;

        // KinectManager 인스턴스 가져오기
        if (kinectManager == null)
        {
            kinectManager = KinectManager.Instance;
        }

        // 아바타를 Kinect 위치로 이동
        MoveAvatar(UserID);

        for (var boneIndex = 0; boneIndex < bones.Length; boneIndex++)
        {
            if (!bones[boneIndex])
                continue;

            if (boneIndex2JointMap.ContainsKey(boneIndex))
            {
                KinectWrapper.NuiSkeletonPositionIndex joint = !mirroredMovement ? boneIndex2JointMap[boneIndex] : boneIndex2MirrorJointMap[boneIndex];
                TransformBone(UserID, joint, boneIndex, !mirroredMovement);
            }
            
        }
    }

    // 뼈를 초기 위치와 회전으로 되돌리기
    public void ResetToInitialPosition()
    {
        if (bones == null)
            return;

        if (offsetNode != null)
        {
            offsetNode.transform.rotation = Quaternion.identity;
        }
        else
        {
            transform.rotation = Quaternion.identity;
        }

        // 정의된 각 뼈에 대해 초기 위치로 되돌리기.
        for (int i = 0; i < bones.Length; i++)
        {
            if (bones[i] != null)
            {
                bones[i].rotation = initialRotations[i];
            }
        }

        if (bodyRoot != null)
        {
            bodyRoot.localPosition = Vector3.zero;
            bodyRoot.localRotation = Quaternion.identity;
        }

        // 오프셋 위치 및 회전 복원
        if (offsetNode != null)
        {
            offsetNode.transform.position = initialPosition;
            offsetNode.transform.rotation = initialRotation;
        }
        else
        {
            transform.position = initialPosition;
            transform.rotation = initialRotation;
        }
    }

    // 플레이어가 성공적으로 보정되었을 때 호출
    public void SuccessfulCalibration(uint userId)
    {
        // 모델의 위치 초기화
        if (offsetNode != null)
        {
            offsetNode.transform.rotation = initialRotation;
        }

        // 위치 오프셋을 다시 보정
        offsetCalibrated = false;
    }

    // Kinect에서 추적한 회전을 뼈에 적용
    protected void TransformBone(uint userId, KinectWrapper.NuiSkeletonPositionIndex joint, int boneIndex, bool flip)
    {
        Transform boneTransform = bones[boneIndex];
        if (boneTransform == null || kinectManager == null)
            return;

        int iJoint = (int)joint;
        if (iJoint < 0)
            return;

        // Kinect 조인트의 회전 값 가져오기
        Quaternion jointRotation = kinectManager.GetJointOrientation(userId, iJoint, flip);
        if (jointRotation == Quaternion.identity)
            return;

        // 새로운 회전으로 부드럽게 전환
        Quaternion newRotation = Kinect2AvatarRot(jointRotation, boneIndex);

        if (smoothFactor != 0f)
            boneTransform.rotation = Quaternion.Slerp(boneTransform.rotation, newRotation, smoothFactor * Time.deltaTime);
        else
            boneTransform.rotation = newRotation;
    }


    // 3D 공간에서 아바타를 이동시킴 - 척추의 추적된 위치를 가져와 루트에 적용
    // 위치만 가져오고 회전은 적용하지 않음
    protected void MoveAvatar(uint UserID)
    {
        if (bodyRoot == null || kinectManager == null)
            return;
        if (!kinectManager.IsJointTracked(UserID, (int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter))
            return;

        // 몸의 위치를 가져와 저장
        Vector3 trans = kinectManager.GetUserPosition(UserID);

        // 아바타를 처음 이동시키는 경우 오프셋을 설정하고, 그렇지 않으면 무시
        if (!offsetCalibrated)
        {
            offsetCalibrated = true;

            xOffset = !mirroredMovement ? trans.x * moveRate : -trans.x * moveRate;
            yOffset = trans.y * moveRate;
            zOffset = -trans.z * moveRate;

            if (offsetRelativeToSensor)
            {
                Vector3 cameraPos = Camera.main.transform.position;

                float yRelToAvatar = (offsetNode != null ? offsetNode.transform.position.y : transform.position.y) - cameraPos.y;
                Vector3 relativePos = new Vector3(trans.x * moveRate, yRelToAvatar, trans.z * moveRate);
                Vector3 offsetPos = cameraPos + relativePos;

                if (offsetNode != null)
                {
                    offsetNode.transform.position = offsetPos;
                }
                else
                {
                    transform.position = offsetPos;
                }
            }
        }

        // 새로운 위치로 부드럽게 전환
        Vector3 targetPos = Kinect2AvatarPos(trans, verticalMovement);

        if (smoothFactor != 0f)
            bodyRoot.localPosition = Vector3.Lerp(bodyRoot.localPosition, targetPos, smoothFactor * Time.deltaTime);
        else
            bodyRoot.localPosition = targetPos;
    }

    // 매핑할 뼈가 선언되어 있다면, 해당 뼈를 모델에 매핑
    protected virtual void MapBones()
    {
        // 모델 변환의 부모로 OffsetNode를 만듦
        offsetNode = new GameObject(name + "Ctrl") { layer = transform.gameObject.layer, tag = transform.gameObject.tag };
        offsetNode.transform.position = transform.position;
        offsetNode.transform.rotation = transform.rotation;
        offsetNode.transform.parent = transform.parent;

        transform.parent = offsetNode.transform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        // 모델 변환을 body root로 설정
        bodyRoot = transform;

        // 애니메이터 컴포넌트에서 뼈 변환을 가져옴
        var animatorComponent = GetComponent<Animator>();

        for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++)
        {
            if (!boneIndex2MecanimMap.ContainsKey(boneIndex))
                continue;

            bones[boneIndex] = animatorComponent.GetBoneTransform(boneIndex2MecanimMap[boneIndex]);
        }
    }

    // 뼈의 초기 회전 값을 캡처
    protected void GetInitialRotations()
    {
        // 초기 회전 값 저장
        if (offsetNode != null)
        {
            initialPosition = offsetNode.transform.position;
            initialRotation = offsetNode.transform.rotation;

            offsetNode.transform.rotation = Quaternion.identity;
        }
        else
        {
            initialPosition = transform.position;
            initialRotation = transform.rotation;

            transform.rotation = Quaternion.identity;
        }

        for (int i = 0; i < bones.Length; i++)
        {
            if (bones[i] != null)
            {
                initialRotations[i] = bones[i].rotation; // * Quaternion.Inverse(initialRotation);
                initialLocalRotations[i] = bones[i].localRotation;
            }
        }

        // 초기 회전 값 복원
        if (offsetNode != null)
        {
            offsetNode.transform.rotation = initialRotation;
        }
        else
        {
            transform.rotation = initialRotation;
        }
    }

    // Kinect의 조인트 회전을 아바타의 조인트 회전으로 변환, 조인트의 초기 회전과 오프셋 회전을 기준으로 변환
    protected Quaternion Kinect2AvatarRot(Quaternion jointRotation, int boneIndex)
    {
        // 새로운 회전 값 적용
        Quaternion newRotation = jointRotation * initialRotations[boneIndex];

        // OffsetNode가 지정되어 있다면, 해당 변환과 회전 값을 결합하여 사실상 뼈대가 해당 노드를 기준으로 하도록 만듦
        if (offsetNode != null)
        {
            // 오일러 회전 값과 오프셋의 오일러 값을 더해서 전체 회전 값 계산
            Vector3 totalRotation = newRotation.eulerAngles + offsetNode.transform.rotation.eulerAngles;
            // 새 회전 값 계산
            newRotation = Quaternion.Euler(totalRotation);
        }

        return newRotation;
    }

    // Kinect 위치를 아바타의 뼈대 위치로 변환, 초기 위치, 반전 여부 및 이동 비율에 따라 변환
    protected Vector3 Kinect2AvatarPos(Vector3 jointPosition, bool bMoveVertically)
    {
        float xPos;
        float yPos;
        float zPos;

        // 이동이 반전된 경우, 위치를 반전
        if (!mirroredMovement)
            xPos = jointPosition.x * moveRate - xOffset;
        else
            xPos = -jointPosition.x * moveRate - xOffset;

        yPos = jointPosition.y * moveRate - yOffset;
        zPos = -jointPosition.z * moveRate - zOffset;

        // 수직 이동을 추적 중이라면 y 값 업데이트, 그렇지 않으면 그대로 둠
        Vector3 avatarJointPos = new Vector3(xPos, bMoveVertically ? yPos : 0f, zPos);

        return avatarJointPos;
    }

    // 뼈 처리 속도를 높이기 위한 딕셔너리들
    private readonly Dictionary<int, HumanBodyBones> boneIndex2MecanimMap = new Dictionary<int, HumanBodyBones>
	{
		{0, HumanBodyBones.Hips},
		{1, HumanBodyBones.Spine},
		{2, HumanBodyBones.Neck},
		{3, HumanBodyBones.Head},
		
		{4, HumanBodyBones.LeftShoulder},
		{5, HumanBodyBones.LeftUpperArm},
		{6, HumanBodyBones.LeftLowerArm},
		{7, HumanBodyBones.LeftHand},
		{8, HumanBodyBones.LeftIndexProximal},

		{9, HumanBodyBones.RightShoulder},
		{10, HumanBodyBones.RightUpperArm},
		{11, HumanBodyBones.RightLowerArm},
		{12, HumanBodyBones.RightHand},
		{13, HumanBodyBones.RightIndexProximal},

		{14, HumanBodyBones.LeftUpperLeg},
		{15, HumanBodyBones.LeftLowerLeg},
		{16, HumanBodyBones.LeftFoot},
		{17, HumanBodyBones.LeftToes},
		
		{18, HumanBodyBones.RightUpperLeg},
		{19, HumanBodyBones.RightLowerLeg},
		{20, HumanBodyBones.RightFoot},
		{21, HumanBodyBones.RightToes},
	};
	
	protected readonly Dictionary<int, KinectWrapper.NuiSkeletonPositionIndex> boneIndex2JointMap = new Dictionary<int, KinectWrapper.NuiSkeletonPositionIndex>
	{
		{0, KinectWrapper.NuiSkeletonPositionIndex.HipCenter},
		{1, KinectWrapper.NuiSkeletonPositionIndex.Spine},
		{2, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter},
		{3, KinectWrapper.NuiSkeletonPositionIndex.Head},
		
		{5, KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft},
		{6, KinectWrapper.NuiSkeletonPositionIndex.ElbowLeft},
		{7, KinectWrapper.NuiSkeletonPositionIndex.WristLeft},
		{8, KinectWrapper.NuiSkeletonPositionIndex.HandLeft},
		
		{10, KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight},
		{11, KinectWrapper.NuiSkeletonPositionIndex.ElbowRight},
		{12, KinectWrapper.NuiSkeletonPositionIndex.WristRight},
		{13, KinectWrapper.NuiSkeletonPositionIndex.HandRight},
		
		{14, KinectWrapper.NuiSkeletonPositionIndex.HipLeft},
		{15, KinectWrapper.NuiSkeletonPositionIndex.KneeLeft},
		{16, KinectWrapper.NuiSkeletonPositionIndex.AnkleLeft},
		{17, KinectWrapper.NuiSkeletonPositionIndex.FootLeft},
		
		{18, KinectWrapper.NuiSkeletonPositionIndex.HipRight},
		{19, KinectWrapper.NuiSkeletonPositionIndex.KneeRight},
		{20, KinectWrapper.NuiSkeletonPositionIndex.AnkleRight},
		{21, KinectWrapper.NuiSkeletonPositionIndex.FootRight},
	};
	
	protected readonly Dictionary<int, List<KinectWrapper.NuiSkeletonPositionIndex>> specIndex2JointMap = new Dictionary<int, List<KinectWrapper.NuiSkeletonPositionIndex>>
	{
		{4, new List<KinectWrapper.NuiSkeletonPositionIndex> {KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter} },
		{9, new List<KinectWrapper.NuiSkeletonPositionIndex> {KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter} },
	};
	
	protected readonly Dictionary<int, KinectWrapper.NuiSkeletonPositionIndex> boneIndex2MirrorJointMap = new Dictionary<int, KinectWrapper.NuiSkeletonPositionIndex>
	{
		{0, KinectWrapper.NuiSkeletonPositionIndex.HipCenter},
		{1, KinectWrapper.NuiSkeletonPositionIndex.Spine},
		{2, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter},
		{3, KinectWrapper.NuiSkeletonPositionIndex.Head},
		
		{5, KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight},
		{6, KinectWrapper.NuiSkeletonPositionIndex.ElbowRight},
		{7, KinectWrapper.NuiSkeletonPositionIndex.WristRight},
		{8, KinectWrapper.NuiSkeletonPositionIndex.HandRight},
		
		{10, KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft},
		{11, KinectWrapper.NuiSkeletonPositionIndex.ElbowLeft},
		{12, KinectWrapper.NuiSkeletonPositionIndex.WristLeft},
		{13, KinectWrapper.NuiSkeletonPositionIndex.HandLeft},
		
		{14, KinectWrapper.NuiSkeletonPositionIndex.HipRight},
		{15, KinectWrapper.NuiSkeletonPositionIndex.KneeRight},
		{16, KinectWrapper.NuiSkeletonPositionIndex.AnkleRight},
		{17, KinectWrapper.NuiSkeletonPositionIndex.FootRight},
		
		{18, KinectWrapper.NuiSkeletonPositionIndex.HipLeft},
		{19, KinectWrapper.NuiSkeletonPositionIndex.KneeLeft},
		{20, KinectWrapper.NuiSkeletonPositionIndex.AnkleLeft},
		{21, KinectWrapper.NuiSkeletonPositionIndex.FootLeft},
	};
	
	protected readonly Dictionary<int, List<KinectWrapper.NuiSkeletonPositionIndex>> specIndex2MirrorJointMap = new Dictionary<int, List<KinectWrapper.NuiSkeletonPositionIndex>>
	{
		{4, new List<KinectWrapper.NuiSkeletonPositionIndex> {KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter} },
		{9, new List<KinectWrapper.NuiSkeletonPositionIndex> {KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter} },
	};
	
}

