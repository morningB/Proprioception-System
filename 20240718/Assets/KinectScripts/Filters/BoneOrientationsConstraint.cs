using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// 관절 위치와 관절 방향을 사람이 가능한 범위 내로 제한하기 위한 필터.
/// </summary>
public class BoneOrientationsConstraint
{
    public enum ConstraintAxis { X = 0, Y = 1, Z = 2 }

    // 관절 제한 목록
    private readonly List<BoneOrientationConstraint> jointConstraints = new List<BoneOrientationConstraint>();

    //private GameObject debugText;

    /// <summary>
    /// BoneOrientationConstraints 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    public BoneOrientationsConstraint()
    {
        //debugText = GameObject.Find("CalibrationText");
    }

    // 주어진 관절에 대한 골격 제한 구조를 찾습니다.
    // 리스트에서 해당 구조의 인덱스를 반환하거나, 찾을 수 없으면 -1을 반환합니다.
    private int FindBoneOrientationConstraint(int joint)
    {
        for (int i = 0; i < jointConstraints.Count; i++)
        {
            if (jointConstraints[i].joint == joint)
                return i;
        }

        // 찾지 못한 경우
        return -1;
    }

    // AddJointConstraint - Adds a joint constraint to the system.  
    public void AddBoneOrientationConstraint(int joint, ConstraintAxis axis, float angleMin, float angleMax)
    {
		int index = FindBoneOrientationConstraint(joint);
		
		BoneOrientationConstraint jc = index >= 0 ? jointConstraints[index] : new BoneOrientationConstraint(joint);
		
		if(index < 0)
		{
			index = jointConstraints.Count;
			jointConstraints.Add(jc);
		}
		
		AxisOrientationConstraint constraint = new AxisOrientationConstraint(axis, angleMin, angleMax);
		jc.axisConstrainrs.Add(constraint);
		
		jointConstraints[index] = jc;
     }

    // AddDefaultConstraints - Adds a set of default joint constraints for normal human poses.  
    // This is a reasonable set of constraints for plausible human bio-mechanics.
    public void AddDefaultConstraints()
    {
//        // 척추
//        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.Spine, ConstraintAxis.X, -10f, 45f);
//        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.Spine, ConstraintAxis.Y, -10f, 30f);
//        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.Spine, ConstraintAxis.Z, -10f, 30f);
        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.Spine, ConstraintAxis.X, -90f, 95f);
        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.Spine, ConstraintAxis.Y, -90f, 90f);
        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.Spine, ConstraintAxis.Z, -90f, 90f);

        // 어깨 중심
        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter, ConstraintAxis.X, -30f, 10f);
        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter, ConstraintAxis.Y, -20f, 20f);
        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter, ConstraintAxis.Z, -20f, 20f);

        // 왼쪽 어깨, 오른쪽 어깨
        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft, ConstraintAxis.X, 0f, 30f);
        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight, ConstraintAxis.X, 0f, 30f);
        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft, ConstraintAxis.Y, -60f, 60f);
        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight, ConstraintAxis.Y, -30f, 90f);
        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft, ConstraintAxis.Z, -90f, 90f);
        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight, ConstraintAxis.Z, -90f, 90f);

        //   // 왼쪽 팔꿈치, 오른쪽 팔꿈치
        //        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.ElbowLeft, ConstraintAxis.X, 300f, 360f);
        //        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.ElbowRight, ConstraintAxis.X, 300f, 360f);
        //        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.ElbowLeft, ConstraintAxis.Y, 210f, 340f);
        //        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.ElbowRight, ConstraintAxis.Y, 0f, 120f);
        //        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.ElbowLeft, ConstraintAxis.Z, -90f, 30f);
        //        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.ElbowRight, ConstraintAxis.Z, 0f, 120f);
        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.ElbowLeft, ConstraintAxis.X, -90f, 90f);
        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.ElbowRight, ConstraintAxis.X, -90f, 90f);
        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.ElbowLeft, ConstraintAxis.Y, -90f, 90f);
        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.ElbowRight, ConstraintAxis.Y, -90f, 90f);
        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.ElbowLeft, ConstraintAxis.Z, -90f, 90f);
        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.ElbowRight, ConstraintAxis.Z, -90f, 90f);

        // 왼쪽 손목, 오른쪽 손목
        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.WristLeft, ConstraintAxis.X, -90f, 90f);
        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.WristRight, ConstraintAxis.X, -90f, 90f);
        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.WristLeft, ConstraintAxis.Y, -90f, 90f);
        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.WristRight, ConstraintAxis.Y, -90f, 90f);
        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.WristLeft, ConstraintAxis.Z, -90f, 90f);
        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.WristRight, ConstraintAxis.Z, -90f, 90f);

        //        // HipLeft, HipRight
        //        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.HipLeft, ConstraintAxis.X, 0f, 90f);
        //        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.HipRight, ConstraintAxis.X, 0f, 90f);
        //        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.HipLeft, ConstraintAxis.Y, 0f, 0f);
        //        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.HipRight, ConstraintAxis.Y, 0f, 0f);
        //        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.HipLeft, ConstraintAxis.Z, 270f, 360f);
        //        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.HipRight, ConstraintAxis.Z, 0f, 90f);

        // 왼쪽 무릎, 오른쪽 무릎
        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.KneeLeft, ConstraintAxis.X, 270f, 360f);
        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.KneeRight, ConstraintAxis.X, 270f, 360f);
        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.KneeLeft, ConstraintAxis.Y, 0f, 0f);
        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.KneeRight, ConstraintAxis.Y, 0f, 0f);
        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.KneeLeft, ConstraintAxis.Z, 0f, 0f);
        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.KneeRight, ConstraintAxis.Z, 0f, 0f);

        // 왼쪽 발목, 오른쪽 발목
        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.AnkleLeft, ConstraintAxis.X, 0f, 0f);
        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.AnkleRight, ConstraintAxis.X, 0f, 0f);
        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.AnkleLeft, ConstraintAxis.Y, -40f, 40f);
        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.AnkleRight, ConstraintAxis.Y, -40f, 40f);
        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.AnkleLeft, ConstraintAxis.Z, 0f, 0f);
        AddBoneOrientationConstraint((int)KinectWrapper.NuiSkeletonPositionIndex.AnkleRight, ConstraintAxis.Z, 0f, 0f);

	}

    // ApplyBoneOrientationConstraints - 관절 회전 제한을 적용합니다.
    public void Constrain(ref Matrix4x4[] jointOrientations, ref bool[] jointTracked)
    {
        // 관절 회전 제한은 부모 관절 벡터에 대해 정의된 벡터와 제한 각도로 설정됩니다.
        // 0.0-1.0은 관절이 제한 범위 내에 있다는 의미이고, 1.0 이상이면 범위를 벗어났다는 의미입니다.

        for (int i = 0; i < this.jointConstraints.Count; i++)
        {
            BoneOrientationConstraint jc = this.jointConstraints[i];

            if (!jointTracked[i] || jc.joint == (int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter) 
            {
                // 관절이 추적되지 않거나 Hip Center는 부모가 없어서 계산을 수행할 수 없습니다.

                continue;
            }

            // 부모가 있는 경우, 관절의 방향을 제한된 범위 내로 설정합니다.
            int parentIdx = KinectWrapper.GetSkeletonJointParent(jc.joint);

            // 부모에 대한 상대적 로컬 관절 방향
            Matrix4x4 localOrientationMatrix = jointOrientations[parentIdx].inverse * jointOrientations[jc.joint];
			
			Vector3 localOrientationZ = (Vector3)localOrientationMatrix.GetColumn(2);
			Vector3 localOrientationY = (Vector3)localOrientationMatrix.GetColumn(1);
			if(localOrientationZ == Vector3.zero || localOrientationY == Vector3.zero)
				continue;
            // 각도를 제한합니다.
            Quaternion localRotation = Quaternion.LookRotation(localOrientationZ, localOrientationY);
			Vector3 eulerAngles = localRotation.eulerAngles;
			bool isConstrained = false;
			
			//Matrix4x4 globalOrientationMatrix = jointOrientations[jc.joint];
			//Quaternion globalRotation = Quaternion.LookRotation(globalOrientationMatrix.GetColumn(2), globalOrientationMatrix.GetColumn(1));
			
			for(int a = 0; a < jc.axisConstrainrs.Count; a++)
			{
				AxisOrientationConstraint ac = jc.axisConstrainrs[a];
				
				Quaternion axisRotation = Quaternion.AngleAxis(localRotation.eulerAngles[ac.axis], ac.rotateAround);
				//Quaternion axisRotation = Quaternion.AngleAxis(globalRotation.eulerAngles[ac.axis], ac.rotateAround);
				float angleFromMin = Quaternion.Angle(axisRotation, ac.minQuaternion);
				float angleFromMax = Quaternion.Angle(axisRotation, ac.maxQuaternion);
				 
				if(!(angleFromMin <= ac.angleRange && angleFromMax <= ac.angleRange))
				{
                    // 다른 축에 대한 현재 회전 값을 유지하고  범위를 벗어난 축만 수정합니다.
                    //Vector3 euler = globalRotation.eulerAngles;

                    if (angleFromMin > angleFromMax)
					{
						eulerAngles[ac.axis] = ac.angleMax;
					}
					else
					{
						eulerAngles[ac.axis] = ac.angleMin;
					}
					
					isConstrained = true;
				}
			}
			
			if(isConstrained)
			{
				Quaternion constrainedRotation = Quaternion.Euler(eulerAngles);

                // 수정된 회전을 뼈대 방향에 다시 적용합니다.
                localOrientationMatrix.SetTRS(Vector3.zero, constrainedRotation, Vector3.one); 
				jointOrientations[jc.joint] = jointOrientations[parentIdx] * localOrientationMatrix;
				//globalOrientationMatrix.SetTRS(Vector3.zero, constrainedRotation, Vector3.one); 
				//jointOrientations[jc.joint] = globalOrientationMatrix;
				
				switch(jc.joint)
				{
					case (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter:
						jointOrientations[(int)KinectWrapper.NuiSkeletonPositionIndex.Head] = jointOrientations[jc.joint];
						break;
					case (int)KinectWrapper.NuiSkeletonPositionIndex.WristLeft:
						jointOrientations[(int)KinectWrapper.NuiSkeletonPositionIndex.HandLeft] = jointOrientations[jc.joint];
						break;
					case (int)KinectWrapper.NuiSkeletonPositionIndex.WristRight:
						jointOrientations[(int)KinectWrapper.NuiSkeletonPositionIndex.HandRight] = jointOrientations[jc.joint];
						break;
					case (int)KinectWrapper.NuiSkeletonPositionIndex.AnkleLeft:
						jointOrientations[(int)KinectWrapper.NuiSkeletonPositionIndex.FootLeft] = jointOrientations[jc.joint];
						break;
					case (int)KinectWrapper.NuiSkeletonPositionIndex.AnkleRight:
						jointOrientations[(int)KinectWrapper.NuiSkeletonPositionIndex.FootRight] = jointOrientations[jc.joint];
						break;
				}
			}
			
//			globalRotation = Quaternion.LookRotation(globalOrientationMatrix.GetColumn(2), globalOrientationMatrix.GetColumn(1));
//			string stringToDebug = string.Format("{0}, {2}", (KinectWrapper.NuiSkeletonPositionIndex)jc.joint, 
//				globalRotation.eulerAngles, localRotation.eulerAngles);
//			Debug.Log(stringToDebug);
//			
//			if(debugText != null)
//				debugText.guiText.text = stringToDebug;
			
        }
    }


    // 관절의 축, 각도, 원뿔 방향 및 관련 관절을 저장하는 제약 구조체
    private struct BoneOrientationConstraint
    {
		// skeleton joint
		public int joint;

        // 이 뼈에 대한 축 제약 리스트
        public List<AxisOrientationConstraint> axisConstrainrs;
		
		
        public BoneOrientationConstraint(int joint)
        {
            this.joint = joint;
			axisConstrainrs = new List<AxisOrientationConstraint>();
        }
    }
	
	
	private struct AxisOrientationConstraint
	{
        // 회전할 축
        public int axis;
        public Vector3 rotateAround;

        // 허용된 각도의 최소, 최대 값과 범위
        public float angleMin;
		public float angleMax;
		
		public Quaternion minQuaternion;
		public Quaternion maxQuaternion;
		public float angleRange;
				
		
		public AxisOrientationConstraint(ConstraintAxis axis, float angleMin, float angleMax)
		{
            // 회전할 축 설정
            this.axis = (int)axis;
			
			switch(axis)
			{
				case ConstraintAxis.X:
					this.rotateAround = Vector3.right;
					break;
				 
				case ConstraintAxis.Y:
					this.rotateAround = Vector3.up;
					break;
				 
				case ConstraintAxis.Z:
					this.rotateAround = Vector3.forward;
					break;
			
				default:
					this.rotateAround = Vector3.zero;
					break;
			}

            // 최소 및 최대 회전 각도를 설정 (도 단위)
            this.angleMin = angleMin;
            this.angleMax = angleMax;

            // 쿼터니언 공간에서 최소 및 최대 회전 설정
            this.minQuaternion = Quaternion.AngleAxis(angleMin, this.rotateAround);
			this.maxQuaternion = Quaternion.AngleAxis(angleMax, this.rotateAround);
			this.angleRange = angleMax - angleMin;
		}
	}
	
}