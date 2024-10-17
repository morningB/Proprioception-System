using System;
using System.IO;
using System.Net.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MLModelVisualization : MonoBehaviour
{
    public RawImage displayImage;  // Unity UI�� RawImage�� ��ġ�Ͽ� �̹��� ǥ��
    public TextMeshProUGUI performanceText;   // ���� ��ǥ�� ǥ���� Text UI

    private TcpClient client;
    private NetworkStream stream;

    void Start()
    {
        ConnectToPythonServer();
    }
    // Stream���� 4����Ʈ�� ���۵� ������ ���̸� �д� �Լ�
    int ReadIntFromStream(NetworkStream stream)
    {
        byte[] lengthBytes = new byte[4];
        stream.Read(lengthBytes, 0, 4);
        return BitConverter.ToInt32(lengthBytes, 0);
    }

    // Stream���� ������ ���̸�ŭ ���ڿ��� �д� �Լ�
    string ReadFromStream(NetworkStream stream)
    {
        int dataLength = ReadIntFromStream(stream);  // ������ ���� �б�
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

            // Python �����κ��� �̹��� ����
            byte[] imageBytes = ReceiveImage();
            DisplayImage(imageBytes);

            // ���� ��ǥ ����
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
            if (bytesRead < buffer.Length) break;  // ������ ���� �Ϸ�
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
