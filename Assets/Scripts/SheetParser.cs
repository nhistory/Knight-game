using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq; // Skip()과 같은 Linq 기능을 사용하기 위해 필요합니다.

public class SheetParser : MonoBehaviour
{
    // 1. 생성한 3개의 TSV 내보내기 URL
    private const string URL_ADJUSTMENTS = "https://docs.google.com/spreadsheets/d/1vr346nPYCVDXW1BCxhlASbMHoP_K5Kkn28sKXCTKctE/export?format=tsv&gid=1843781558&range=A8:B10";
    private const string URL_KNIGHT_LEVELS = "https://docs.google.com/spreadsheets/d/1vr346nPYCVDXW1BCxhlASbMHoP_K5Kkn28sKXCTKctE/export?format=tsv&gid=1843781558&range=A13:J17";
    private const string URL_ITEM_STATS = "https://docs.google.com/spreadsheets/d/1vr346nPYCVDXW1BCxhlASbMHoP_K5Kkn28sKXCTKctE/export?format=tsv&gid=1843781558&range=A21:J23";

    // 2. 파싱된 데이터를 저장할 변수들
    // public으로 선언하여 다른 스크립트에서 접근하기 쉽게 할 수 있습니다.
    public Dictionary<string, float> adjustmentsData = new Dictionary<string, float>();
    public Dictionary<string, List<string>> knightLevelsData = new Dictionary<string, List<string>>();
    public Dictionary<string, List<string>> itemStatsData = new Dictionary<string, List<string>>();

    // 3. 게임 시작 시 모든 데이터를 불러오는 메인 함수
    void Start()
    {
        StartCoroutine(LoadAllDataCoroutine());
    }

    private IEnumerator LoadAllDataCoroutine()
    {
        Debug.Log("모든 구글 시트 데이터 로딩을 시작합니다...");

        // 각 테이블을 순차적으로 불러옵니다.
        yield return StartCoroutine(DownloadDataCoroutine(URL_ADJUSTMENTS, ParseAdjustmentsTable));
        yield return StartCoroutine(DownloadDataCoroutine(URL_KNIGHT_LEVELS, text => ParseMatrixTable(text, knightLevelsData)));
        yield return StartCoroutine(DownloadDataCoroutine(URL_ITEM_STATS, text => ParseMatrixTable(text, itemStatsData)));

        Debug.Log("모든 데이터 로딩 및 파싱 완료!");
        
        // 로딩된 데이터를 확인하기 위한 예시 출력
        PrintLoadedDataExample();
    }

    // 4. 데이터를 다운로드하고, 완료되면 파싱 함수를 실행하는 범용 코루틴
    private IEnumerator DownloadDataCoroutine(string url, System.Action<string> onComplete)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                onComplete(www.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"데이터 다운로드 실패: {url}\n에러: {www.error}");
            }
        }
    }

    // 5. 각 테이블의 텍스트를 파싱하여 Dictionary에 저장하는 함수들

    // Adjustments 테이블 파싱 (키-값)
    private void ParseAdjustmentsTable(string tsvText)
    {
        adjustmentsData.Clear();
        string[] rows = tsvText.Trim().Split('\n');
        foreach (string row in rows)
        {
            string[] columns = row.Trim().Split('\t');
            if (columns.Length >= 2)
            {
                string key = columns[0];
                float.TryParse(columns[1], out float value);
                adjustmentsData[key] = value;
            }
        }
        Debug.Log("Adjustments 테이블 파싱 완료.");
    }

    // Knight Levels, Item Stats와 같은 행렬 테이블 파싱 (범용)
    private void ParseMatrixTable(string tsvText, Dictionary<string, List<string>> targetDictionary)
    {
        targetDictionary.Clear();
        string[] rows = tsvText.Trim().Split('\n');
        foreach (string row in rows)
        {
            string[] columns = row.Trim().Split('\t');
            if (columns.Length > 1)
            {
                string key = columns[0];
                List<string> values = columns.Skip(1).ToList(); // 첫 열(키)을 제외한 나머지를 리스트로
                targetDictionary[key] = values;
            }
        }
        Debug.Log($"행렬 테이블 파싱 완료. {rows.Length}개 행 처리됨.");
    }

    // 6. 로딩된 데이터를 실제로 사용하는 방법 예시
    private void PrintLoadedDataExample()
    {
        Debug.Log("--- 로드된 데이터 확인 ---");

        // Adjustments 데이터 접근
        if (adjustmentsData.ContainsKey("MA"))
        {
            Debug.Log($"MA 조정값: {adjustmentsData["MA"]}"); // 예: 10
        }

        // Knight Levels 데이터 접근 (인덱스는 0부터 시작)
        if (knightLevelsData.ContainsKey("hp"))
        {
            // 1레벨 체력 -> 첫 번째 값이므로 인덱스 0
            Debug.Log($"기사 1레벨 체력: {knightLevelsData["hp"][0]}"); // 예: 300
            // 9레벨 체력 -> 아홉 번째 값이므로 인덱스 8
            Debug.Log($"기사 9레벨 체력: {knightLevelsData["hp"][8]}"); // 예: 700
        }

        // Item Stats 데이터 접근 및 형 변환
        if (itemStatsData.ContainsKey("Armor"))
        {
            string armorTier5_str = itemStatsData["Armor"][4]; // "10.0%"
            Debug.Log($"방어구 5티어 (원본 문자열): {armorTier5_str}");

            // 실제 사용을 위해 문자열에서 '%'를 제거하고 100으로 나누어 float으로 변환
            float armorTier5_float = float.Parse(armorTier5_str.Replace("%", "")) / 100f;
            Debug.Log($"방어구 5티어 (float 값): {armorTier5_float}"); // 예: 0.1
        }
    }
}
