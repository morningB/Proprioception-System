using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System.Collections;
using System;

public class TextSocket : MonoBehaviour
{
    public Text performanceMetricsText; // 성능 지표를 표시할 Text UI 요소


    void Start()
    {
        StartSocketClient();
    }

    void StartSocketClient()
    {
        try
        {
            using (TcpClient client = new TcpClient("localhost", 12345)) // 서버 주소와 포트 번호
            using (NetworkStream stream = client.GetStream())
            {
                byte[] buffer = new byte[2048];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string message = System.Text.Encoding.ASCII.GetString(buffer, 0, bytesRead);

                // 성능 지표와 이미지 데이터 분리
                string[] parts = message.Split(';');
                performanceMetricsText.text = parts[0]; // 성능 지표

            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("소켓 통신 오류: " + e.Message);
        }
    }

}
