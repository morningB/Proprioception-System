using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneChanger : MonoBehaviour
{
    public void acredev1SceneChange()
    // acredev1SceneChange �̸��� �Լ� ����
    {
        SceneManager.LoadScene("KinectAvatarsDemo");
        //SceneManager �޼����� LoadScene �Լ��� ���� acredev1.scene���� �� ��ȯ
    }

    public void acredev2SceneChange()
    { 
    // acredev2SceneChanger
        SceneManager.LoadScene("changescen");
        //SceneManager �޼����� LoadScene �Լ��� ���� acredev2.scene���� �� ��ȯ
    }
    // ù ȭ������ ���ư���
    public void GoToMainScene()
    {
        // acredev2SceneChanger
        SceneManager.LoadScene("MainScene");
        //SceneManager �޼����� LoadScene �Լ��� ���� acredev2.scene���� �� ��ȯ
    }
    // ù ȭ�鿡�� �ٷ� �׷����� ����
    public void LoadData()
    {
        SceneManager.LoadScene("realGraph");
    }
    public void LH()
    {
        SceneManager.LoadScene("LeftHandScene");
    }
    public void RL()
    {
        SceneManager.LoadScene("RightLegScene");
    }
    public void LL()
    {
        SceneManager.LoadScene("LeftLegScene");
    }
}