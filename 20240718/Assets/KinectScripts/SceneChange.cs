using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneChanger : MonoBehaviour
{
    public void acredev1SceneChange()
    // acredev1SceneChange 이름의 함수 선언
    {
        SceneManager.LoadScene("KinectAvatarsDemo");
        //SceneManager 메서드의 LoadScene 함수를 통해 acredev1.scene으로 씬 전환
    }

    public void acredev2SceneChange()
    { 
    // acredev2SceneChanger
        SceneManager.LoadScene("changescen");
        //SceneManager 메서드의 LoadScene 함수를 통해 acredev2.scene으로 씬 전환
    }
    // 첫 화면으로 돌아가기
    public void GoToMainScene()
    {
        // acredev2SceneChanger
        SceneManager.LoadScene("MainScene");
        //SceneManager 메서드의 LoadScene 함수를 통해 acredev2.scene으로 씬 전환
    }
    // 첫 화면에서 바로 그래프로 가기
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