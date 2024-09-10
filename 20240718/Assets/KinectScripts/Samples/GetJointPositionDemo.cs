using UnityEngine;
using System.Collections;
using System.IO;

public class GetJointPositionDemo : MonoBehaviour
{
    // 추적할 조인트
    public KinectWrapper.NuiSkeletonPositionIndex joint = KinectWrapper.NuiSkeletonPositionIndex.HandRight;

    // 현재 Kinect 좌표계에서의 조인트 위치
    public Vector3 outputPosition;

    // CSV 파일에 데이터를 저장할지 여부
    public bool isSaving = false;

    // CSV 파일에 저장할 시간(초), 0이면 지속해서 저장
    public float secondsToSave = 0f;

    // CSV 파일 경로 (;로 구분)
    public string saveFilePath = "joint_pos.csv";


    // 데이터를 CSV 파일에 저장하기 시작한 시간
    private float saveStartTime = -1f;


    void Update()
    {
        if (isSaving)
        {
            // 파일이 없으면 생성
            if (!File.Exists(saveFilePath))
            {
                using (StreamWriter writer = File.CreateText(saveFilePath))
                {
                    // CSV 파일 헤더
                    string sLine = "time;joint;pos_x;pos_y;poz_z";
                    writer.WriteLine(sLine);
                }
            }

            // 시작 시간 확인
            if (saveStartTime < 0f)
            {
                saveStartTime = Time.time;
            }
        }

        // 조인트 위치 가져오기
        KinectManager manager = KinectManager.Instance;

        if (manager && manager.IsInitialized())
        {
            if (manager.IsUserDetected())
            {
                uint userId = manager.GetPlayer1ID();

                if (manager.IsJointTracked(userId, (int)joint))
                {
                    // 추적 중인 조인트의 위치를 출력
                    Vector3 jointPos = manager.GetJointPosition(userId, (int)joint);
                    outputPosition = jointPos;

                    if (isSaving)
                    {
                        if ((secondsToSave == 0f) || ((Time.time - saveStartTime) <= secondsToSave))
                        {
                            using (StreamWriter writer = File.AppendText(saveFilePath))
                            {
                                string sLine = string.Format("{0:F3};{1};{2:F3};{3:F3};{4:F3}", Time.time, (int)joint, jointPos.x, jointPos.y, jointPos.z);
                                writer.WriteLine(sLine);
                            }
                        }
                    }
                }
            }
        }

    }

}
