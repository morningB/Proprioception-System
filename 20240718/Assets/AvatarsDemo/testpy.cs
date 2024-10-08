using System;
using System.IO;
using System.Net.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MLModelVisualization : MonoBehaviour
{
    public RawImage displayImage;  // Unity UI에 RawImage를 배치하여 이미지 표시
    public TextMeshProUGUI performanceText;   // 성능 지표를 표시할 Text UI

    private TcpClient client;
    private NetworkStream stream;

    void Start()
    {
        ConnectToPythonServer();
    }
    // Stream에서 4바이트로 전송된 데이터 길이를 읽는 함수
    int ReadIntFromStream(NetworkStream stream)
    {
        byte[] lengthBytes = new byte[4];
        stream.Read(lengthBytes, 0, 4);
        return BitConverter.ToInt32(lengthBytes, 0);
    }

    // Stream에서 지정된 길이만큼 문자열을 읽는 함수
    string ReadFromStream(NetworkStream stream)
    {
        int dataLength = ReadIntFromStream(stream);  // 데이터 길이 읽기
        byte[] dataBytes = new byte[dataLength];
        stream.Read(dataBytes, 0, dataLength);
        return System.Text.Encoding.UTF8.GetString(dataBytes);
    }
    void ConnectToPythonServer()
    {
        try
        {
            client = new TcpClient("localhost", 5000);
            stream = client.GetStream();

            // Python 서버로부터 이미지 수신
            byte[] imageBytes = ReceiveImage();
            DisplayImage(imageBytes);

            // 성능 지표 수신
            string confusionMatrix = ReadFromStream(stream);
            string classificationReport = ReadFromStream(stream);

            DisplayPerformanceMetrics(confusionMatrix, classificationReport);
        }
        catch (Exception e)
        {
            Debug.Log("Error: " + e.Message);
        }
    }

    byte[] ReceiveImage()
    {
        MemoryStream memoryStream = new MemoryStream();
        byte[] buffer = new byte[1024];
        int bytesRead;

        while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
        {
            memoryStream.Write(buffer, 0, bytesRead);
            if (bytesRead < buffer.Length) break;  // 데이터 수신 완료
        }

        return memoryStream.ToArray();
    }

    void DisplayImage(byte[] imageBytes)
    {
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(imageBytes);
        displayImage.texture = tex;
    }

    string ReadFromStream()
    {
        StreamReader reader = new StreamReader(stream);
        return reader.ReadLine();
    }

    void DisplayPerformanceMetrics(string confusionMatrix, string classificationReport)
    {
        performanceText.text = "Confusion Matrix:\n" + confusionMatrix + "\n" +
                               "Classification Report:\n" + classificationReport;
    }

    void OnApplicationQuit()
    {
        stream.Close();
        client.Close();
    }
}
