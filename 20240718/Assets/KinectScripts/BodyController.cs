using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

public abstract class BodyController : MonoBehaviour
{
    public Button eyeOpen;
    public Button eyeClose;
    public Button reset;
    public Button result;
    public Button addC;

    public TextMeshProUGUI angleText;
    public TextMeshProUGUI angleText2;
    public TextMeshProUGUI angleText3;

    public Image stars;

    protected float openAngle;
    protected float closeAngle;

    protected abstract string CsvDirectory { get; }

    private string csvFilePath;
    private string angleFile;
    private string resultFile;

    protected virtual void Start()
    {
        csvFilePath = $"{CsvDirectory}/tes.csv";
        angleFile = $"{CsvDirectory}/an.csv";
        resultFile = $"{CsvDirectory}/re.csv";

        InitializeCSVFiles();

        eyeOpen.onClick.AddListener(OnEyeOpenClick);
        eyeClose.onClick.AddListener(OnEyeCloseClick);
        reset.onClick.AddListener(OnResetClick);
        result.onClick.AddListener(OnResultClick);
        addC.onClick.AddListener(OnAddCClick);
    }

    protected abstract float CalculateAngle();

    private void InitializeCSVFiles()
    {
        if (!File.Exists(csvFilePath)) File.WriteAllText(csvFilePath,"RightHandPosX,RightHandPosY,RightHandPosZ,RightShoulderPosX,RightShoulderPosY,RightShoulderPosZ,RightAnklePosX,RightAnklePosY,RightAnklePosZ");
        if (!File.Exists(angleFile)) File.WriteAllText(angleFile, "Angle");
        if (!File.Exists(resultFile)) File.WriteAllText(resultFile, "Result");
    }

    protected void SaveToCsv(string filePath, string content)
    {
        using (StreamWriter sw = File.AppendText(filePath))
        {
            sw.WriteLine(content);
        }
    }

    private void OnEyeOpenClick()
    {
        openAngle = CalculateAngle();
        angleText.text = $"Open: {openAngle:F2}";
        SaveToCsv(angleFile, openAngle.ToString());
    }

    private void OnEyeCloseClick()
    {
        closeAngle = CalculateAngle();
        angleText2.text = $"Close: {closeAngle:F2}";
        SaveToCsv(angleFile, closeAngle.ToString());
    }

    private void OnResultClick()
    {
        float angleDifference = Mathf.Abs(openAngle - closeAngle);
        angleText3.text = $"Difference: {angleDifference:F2}";
        SaveToCsv(resultFile, angleDifference.ToString());
    }

    private void OnResetClick()
    {
        openAngle = 0f;
        closeAngle = 0f;
        angleText.text = "";
        angleText2.text = "";
        angleText3.text = "";
    }

    private void OnAddCClick()
    {
        // Custom logic for AddC button
        Debug.Log("AddC Button Clicked");
    }
}
