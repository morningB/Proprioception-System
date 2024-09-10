using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq.Expressions;
using BarGraph.VittorCloud;
using System.IO;

public class BarGraphExample : MonoBehaviour
{
    // 막대 그래프에 데이터를 삽입하기 위한 공용 데이터 세트
    public List<BarGraphDataSet> exampleDataSet;

    // csv 파일 경로 변수
    public string csvPath;

    // BarGraphGenerator 인스턴스
    BarGraphGenerator barGraphGenerator;

    void Start()
    {
        // 파일 경로
        csvPath = "Assets/Resources/re.csv";
        // BarGraphGenerator 컴포넌트를 가져옴
        barGraphGenerator = GetComponent<BarGraphGenerator>();
        // CSV 파일에서 데이터 로드
        LoadDataFromCSV();

        // exampleDataSet 리스트가 비어 있으면 에러 메시지 출력 후 리턴
        if (exampleDataSet.Count == 0)
        {
            Debug.LogError("ExampleDataSet is Empty!");
            return;
        }

        // 데이터 세트를 막대 그래프에 추가
        for (int dataSetIndex = 0; dataSetIndex < exampleDataSet.Count; dataSetIndex++)
        {
            for (int xyValueIndex = 0; xyValueIndex < exampleDataSet[dataSetIndex].ListOfBars.Count; xyValueIndex++)
            {
                barGraphGenerator.AddNewDataSet(dataSetIndex, xyValueIndex, exampleDataSet[dataSetIndex].ListOfBars[xyValueIndex].YValue);
            }
        }

        // 막대 그래프 생성
        barGraphGenerator.GeneratBarGraph(exampleDataSet);
    }

    // 그래프 시작 애니메이션 완료 시 실행할 코루틴
    IEnumerator CreateDataSet()
    {
        // 무한 루프
        while (true)
        {
            // 임의의 데이터를 생성하거나 특정 값을 설정하는 로직을 여기에 추가 가능
            yield return new WaitForSeconds(0f);
        }
    }

    // CSV 파일에서 데이터 로드
    void LoadDataFromCSV()
    {
        try
        {
            using (var reader = new StreamReader(csvPath))
            {
                bool isFirstLine = true;
                BarGraphDataSet currentDataSet = new BarGraphDataSet
                {
                    GroupName = "CSV Data",
                    barColor = Color.red,
                    ListOfBars = new List<XYBarValues>()
                };

                // CSV 파일의 모든 줄을 읽음
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (isFirstLine)
                    {
                        isFirstLine = false;
                        continue; // 헤더 라인은 건너뜀
                    }

                    // 줄을 쉼표로 분리
                    var values = line.Split(',');

                    string xValue = values[0]; // X 값 (이름)
                    float yValue = float.Parse(values[1]); // Y 값 (결과)

                    // 막대 색상을 무작위로 설정
                    Color randomColor = UnityEngine.Random.ColorHSV();

                    // 현재 데이터 세트에 XY 값을 추가
                    currentDataSet.ListOfBars.Add(new XYBarValues
                    {
                        XValue = xValue,
                        YValue = yValue,
                    });
                }

                // exampleDataSet에 현재 데이터 세트 추가
                exampleDataSet.Add(currentDataSet);
            }
            Debug.Log("csv");
        }
        catch (Exception e)
        {
            // CSV 파일 로드 실패 시 에러 메시지 출력
            Debug.LogError("Failed to load data from CSV: " + e.Message);
        }
    }

    // 임의의 데이터를 생성하는 메서드 (현재는 주석 처리됨)
    /* public void StartUpdatingGraph()
    {
        StartCoroutine(CreateDataSet());
    }

    // 그래프의 데이터를 임의로 업데이트하는 메서드
    void GenerateRandomData()
    {
        int dataSetIndex = UnityEngine.Random.Range(0, exampleDataSet.Count);
        int xyValueIndex = UnityEngine.Random.Range(0, exampleDataSet[dataSetIndex].ListOfBars.Count);
        exampleDataSet[dataSetIndex].ListOfBars[xyValueIndex].YValue = UnityEngine.Random.Range(barGraphGenerator.yMinValue, barGraphGenerator.yMaxValue);

        barGraphGenerator.AddNewDataSet(dataSetIndex, xyValueIndex, exampleDataSet[dataSetIndex].ListOfBars[xyValueIndex].YValue);
    }

    // 모든 막대의 Y 값을 80으로 설정하는 메서드
    void SetAllBarsToEighty()
    {
        foreach (var dataSet in exampleDataSet)
        {
            foreach (var bar in dataSet.ListOfBars)
            {
                bar.YValue = 80; // 모든 Y 값을 80으로 설정
            }
        }

        // 새로운 값으로 막대 그래프를 업데이트
        for (int dataSetIndex = 0; dataSetIndex < exampleDataSet.Count; dataSetIndex++)
        {
            for (int xyValueIndex = 0; xyValueIndex < exampleDataSet[dataSetIndex].ListOfBars.Count; xyValueIndex++)
            {
                barGraphGenerator.AddNewDataSet(dataSetIndex, xyValueIndex, exampleDataSet[dataSetIndex].ListOfBars[xyValueIndex].YValue);
            }
        }
    }
    */
}
