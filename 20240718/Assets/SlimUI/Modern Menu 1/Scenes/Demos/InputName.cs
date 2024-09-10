using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputName : MonoBehaviour
{
    public TMP_InputField inputField; // UI InputField�� �����մϴ�.
    public static string inputText; // Static ������ �߰��Ͽ� �����͸� �����մϴ�.

    // Start is called before the first frame update
    void Start()
    {
        // InputField�� OnValueChanged �̺�Ʈ�� �����ʸ� �߰��մϴ�.
        inputField.onValueChanged.AddListener(OnInputValueChanged);
    }

    // InputField�� ���� ����� �� ȣ��Ǵ� �޼����Դϴ�.
    void OnInputValueChanged(string value)
    {
        inputText = value;
        Debug.Log("Input text changed: " + inputText);
    }
}
