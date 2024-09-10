using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputName : MonoBehaviour
{
    public TMP_InputField inputField; // UI InputField를 참조합니다.
    public static string inputText; // Static 변수를 추가하여 데이터를 저장합니다.

    // Start is called before the first frame update
    void Start()
    {
        // InputField의 OnValueChanged 이벤트에 리스너를 추가합니다.
        inputField.onValueChanged.AddListener(OnInputValueChanged);
    }

    // InputField의 값이 변경될 때 호출되는 메서드입니다.
    void OnInputValueChanged(string value)
    {
        inputText = value;
        Debug.Log("Input text changed: " + inputText);
    }
}
