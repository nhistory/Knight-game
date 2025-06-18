using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BoardManager : MonoBehaviour
{
    private Box theKnight;

    [Header("Grid Settings")]
    public int gridWidth = 7;
    public int gridHeight = 10;
    public float cellSpacing = 0f; // 박스 사이의 간격 (선택 사항)
    public Vector2 startOffset = Vector2.zero; // 그리드 시작 위치 오프셋

    [Header("Prefabs")]
    public GameObject[] boxPrefabs; // 0: White, 1: LightBlue, 2: LightOrange (순서 중요)
    public GameObject knightPrefab;
    public GameObject enemyPrefab; // Inspector에서 연결할 Enemy 프리팹

    [Header("Player Settings")]
    public Vector2Int knightStartPosition = new Vector2Int(3, 1);

    [Header("Enemy Settings")]
    public Vector2Int enemyStartPosition = new Vector2Int(3, 7); // 적 시작 위치

    private Box[,] allBoxes; // 그리드 상의 모든 박스를 저장할 2차원 배열
    private Camera mainCamera;     // mainCamera 변수 선언
    private float boxSize;

    [Header("UI")] // UI 관련 변수를 위한 헤더 추가
    public TextMeshProUGUI tileCountText; // Inspector에서 연결할 TextMeshPro UI 요소

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        allBoxes = new Box[gridWidth, gridHeight];

        if (boxPrefabs == null || boxPrefabs.Length == 0 || boxPrefabs[0] == null)
        {
            Debug.LogError("Box prefabs are not assigned or the first prefab is null!");
            return;
        }
        // 첫 번째 프리팹의 스프라이트 크기를 기준으로 boxSize 계산
        SpriteRenderer sr = boxPrefabs[0].GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null)
        {
            Debug.LogError("First box prefab is missing SpriteRenderer or Sprite!");
            boxSize = 1f; // 기본값
        }
        else
        {
            // PPU를 고려한 실제 월드 유닛 크기
            boxSize = sr.sprite.bounds.size.x;
        }


        AdjustCamera(); // 카메라 먼저 조정
        SetupBoard();
        UpdateTileCountUI();
    }

    void AdjustCamera()
    {
        // 그리드의 전체 너비와 높이 (간격 포함)
        float totalGridPhysicalWidth = gridWidth * boxSize + (gridWidth - 1) * cellSpacing;
        float totalGridPhysicalHeight = gridHeight * boxSize + (gridHeight - 1) * cellSpacing;

        // 화면 비율에 따라 카메라 Orthographic Size 설정
        // 여기서는 세로 모드(1179x2556)에서 그리드가 화면 가로 폭에 꽉 차도록 설정하는 것을 목표로 합니다.
        // 또는 세로 높이에 꽉 차도록 할 수도 있습니다. 요구사항에 따라 기준을 정해야 합니다.

        // 예시: 그리드가 화면 가로 폭에 꽉 차도록 설정
        mainCamera.orthographicSize = (totalGridPhysicalWidth / mainCamera.aspect) / 2f;

        // 만약 그리드가 화면 세로 높이에 꽉 차도록 하려면:
        // mainCamera.orthographicSize = totalGridPhysicalHeight / 2f;

        // 카메라 위치: 그리드의 중앙 하단이 (0,0) 근처에 오도록 하거나,
        // 그리드 전체가 화면 중앙에 오도록 조정
        float cameraX = (totalGridPhysicalWidth - boxSize) / 2f; // 그리드 중앙 X
        float cameraY = (totalGridPhysicalHeight - boxSize) / 2f; // 그리드 중앙 Y (그리드가 (0,0)에서 시작할 때의 중심)

        // 화면 하단에 그리드의 첫 번째 행이 오도록 하려면 카메라 Y 위치를 조정해야 합니다.
        // 카메라의 Y 위치는 (화면 높이의 절반 - 그리드 높이의 절반) 만큼 아래로 내려가면 안 되고,
        // 그리드의 가장 아래쪽이 화면의 특정 y_offset에 오도록 설정해야 합니다.

        // 여기서는 그리드의 중앙이 카메라 뷰의 중앙에 오도록 설정합니다.
        mainCamera.transform.position = new Vector3(cameraX, cameraY, -10f);
    }

    void SetupBoard()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                // 현재 좌표가 기사 시작 위치와 같다면 기사를 생성
                if (x == knightStartPosition.x && y == knightStartPosition.y)
                {
                    SpawnKnight(x, y);
                }
                else // 그 외의 모든 위치에는 랜덤 박스를 생성
                {
                    SpawnNewBox(x, y);
                }
            }
        }
        SpawnEnemy();
    }

    // 그리드 좌표를 월드 좌표로 변환
    Vector3 GetWorldPosition(int x, int y)
    {
        float boxSize = boxPrefabs[0].GetComponent<SpriteRenderer>().bounds.size.x; // 첫 번째 프리팹의 크기를 기준
        return new Vector3(
            startOffset.x + x * (boxSize + cellSpacing),
            startOffset.y + y * (boxSize + cellSpacing),
            0
        );
    }

    // 기사 생성 및 배치
    void SpawnKnight(int x, int y)
    {
        if (knightPrefab == null)
        {
            Debug.LogError("Knight Prefab is not assigned in BoardManager!");
            return;
        }

        Vector3 knightPos = GetWorldPosition(x, y);
        knightPos.z = -1; // 다른 타일들보다 앞에 보이도록 Z 위치 조정
        GameObject knightGO = Instantiate(knightPrefab, knightPos, Quaternion.identity);
        knightGO.transform.SetParent(this.transform);

        Box boxScript = knightGO.GetComponent<Box>();
        if (boxScript != null)
        {
            boxScript.board = this; // 의존성 주입
            boxScript.x = x;
            boxScript.y = y;
            // 필요하다면 기사 타일의 색상을 특별하게 지정할 수 있습니다.
            // boxScript.boxColor = BoxColor.Knight; // (BoxColor Enum에 Knight 추가 필요)
            boxScript.isKnight = true;
            allBoxes[x, y] = boxScript;
            theKnight = boxScript;
        }
        else
        {
            Debug.LogError("Knight Prefab is missing the 'Box' component!");
        }
    }

    // 새 박스 생성 및 배치
    void SpawnNewBox(int x, int y)
    {
        if (allBoxes[x, y] != null) // 이미 박스가 있다면 생성하지 않음
        {
            Debug.LogWarning($"Box already exists at ({x},{y})");
            return;
        }

        int randomIndex = Random.Range(0, boxPrefabs.Length);
        GameObject newBoxGO = Instantiate(boxPrefabs[randomIndex], GetWorldPosition(x, y), Quaternion.identity);
        newBoxGO.transform.SetParent(this.transform); // BoardManager의 자식으로 설정 (정리 목적)

        Box boxScript = newBoxGO.GetComponent<Box>();
        boxScript.board = this; // 의존성 주입
        boxScript.x = x;
        boxScript.y = y;
        // boxScript.boxColor는 프리팹에 이미 설정되어 있어야 함. 또는 여기서 설정 가능
        // boxScript.boxColor = (BoxColor)randomIndex; // 만약 프리팹에 색상 설정 안했다면

        boxScript.isKnight = false; // 일반 박스는 기사가 아님을 확실히 설정
        allBoxes[x, y] = boxScript;
    }

    // 적 생성 및 배치
    void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy Prefab is not assigned in BoardManager!");
            return;
        }

        // 1. 적이 올라설 타일을 찾습니다.
        Box targetTile = allBoxes[enemyStartPosition.x, enemyStartPosition.y];
        if (targetTile == null)
        {
            Debug.LogError($"Cannot spawn enemy. No tile at designated start position ({enemyStartPosition.x}, {enemyStartPosition.y})");
            return;
        }

        // 2. 적 오브젝트를 생성하고 위치를 설정합니다.
        Vector3 enemyPos = targetTile.transform.position;
        enemyPos.z = -2; // 기사(-1)보다도 앞에 보이도록 Z 위치 조정
        GameObject enemyGO = Instantiate(enemyPrefab, enemyPos, Quaternion.identity);
        enemyGO.transform.SetParent(this.transform);

        // 3. 생성된 적과 타일을 서로 연결합니다.
        Enemy enemyScript = enemyGO.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.Initialize(targetTile); // 적에게 자신이 어느 타일 위에 있는지 알려줌
            targetTile.enemyOnTop = enemyScript; // 타일에게 자신 위에 어떤 적이 있는지 알려줌
        }
        else
        {
            Debug.LogError("Enemy Prefab is missing the 'Enemy' component!");
        }
    }

    // --- 게임 로직 (매치, 파괴, 낙하 등)은 여기에 추가 ---
    public void ProcessMatches(List<Box> matchedBoxes)
    {
        if (matchedBoxes == null || matchedBoxes.Count < 2) // 최소 2개 이상 매치되어야 함 (규칙에 따라 조절)
        {
            Debug.Log("Not enough boxes to match or list is null. Minimum 2 required.");
            return;
        }

        Debug.Log($"Processing {matchedBoxes.Count} matched boxes.");

        bool madeAMatch = false;
        foreach (Box box in matchedBoxes)
        {
            if (box != null && !box.isMatched)
            {
                // 배열 인덱스 범위 확인
                if (box.x >= 0 && box.x < gridWidth && box.y >= 0 && box.y < gridHeight)
                {
                    if (allBoxes[box.x, box.y] == box) // 배열에 있는 박스와 현재 박스가 동일한지 확인
                    {
                        allBoxes[box.x, box.y] = null; // 배열에서 제거
                        box.DestroyBox();
                        madeAMatch = true;
                    }
                    else
                    {
                        Debug.LogWarning($"Box at ({box.x},{box.y}) in array is not the one being matched, or was already null. Current box: {box.name}");
                    }
                }
                else
                {
                    Debug.LogError($"Box coordinates ({box.x},{box.y}) are out of bounds for allBoxes array!");
                }
            }
            else if (box == null)
            {
                Debug.LogWarning("A null box was passed in matchedBoxes list.");
            }
            else if (box.isMatched)
            {
                Debug.LogWarning($"Box ({box.x},{box.y}) - {box.name} is already matched.");
            }
        }

        if (madeAMatch)
        {
            // 매치 처리 후 빈 공간 채우기
            StartCoroutine(FillEmptySpacesAndUpdateUI());
        }
        else
        {
            Debug.Log("No actual matches were processed...");
            UpdateTileCountUI(); // 매치가 없었더라도 혹시 모르니 UI 업데이트
        }
    }

    // 기사를 새로운 그리드 위치로 이동시키는 함수
    public void MoveKnightTo(Box knightToMove, int targetX, int targetY)
    {
        // 안전 장치: 전달받은 박스가 실제로 기사인지 확인
        if (knightToMove == null || !knightToMove.isKnight)
        {
            Debug.LogError("MoveKnightTo: Attempted to move a non-knight box.");
            return;
        }

        // 1. 기사의 이전 위치를 그리드에서 비웁니다.
        if (allBoxes[knightToMove.x, knightToMove.y] == knightToMove)
        {
            allBoxes[knightToMove.x, knightToMove.y] = null;
        }

        // 2. 기사의 내부 좌표를 새로운 좌표로 업데이트합니다.
        knightToMove.x = targetX;
        knightToMove.y = targetY;

        // 3. 새로운 위치에 기사를 배치합니다.
        // (이 위치에 있던 타일은 곧 ProcessMatches에 의해 파괴될 것이므로 덮어써도 괜찮습니다.)
        allBoxes[targetX, targetY] = knightToMove;

        // 4. 기사 오브젝트에게 새로운 월드 좌표로 시각적 이동을 명령합니다.
        Vector3 targetPos = GetWorldPosition(targetX, targetY);
        targetPos.z = -1f; // 기사가 항상 위에 보이도록 Z값을 조정합니다.
        knightToMove.MoveTo(targetPos, 0.3f);

        // 5. 기사를 "앵커"로 설정하여 이번 턴의 낙하에 영향을 받지 않도록 합니다.
        knightToMove.isAnchored = true;
    }

    // FillEmptySpaces 코루틴이 끝난 후 UI를 업데이트하도록 수정
    private System.Collections.IEnumerator FillEmptySpacesAndUpdateUI()
    {
        yield return StartCoroutine(FillEmptySpaces()); // 기존 FillEmptySpaces 코루틴 실행 기다림
        UpdateTileCountUI(); // FillEmptySpaces 완료 후 UI 업데이트
    }


    // 각 색상별 타일 개수를 계산하고 UI를 업데이트하는 함수
    public void UpdateBoxPosition(Box box, int newX, int newY)
    {
        if (allBoxes[box.x, box.y] == box)
        {
            allBoxes[box.x, box.y] = null;
        }

        box.x = newX;
        box.y = newY;
        allBoxes[newX, newY] = box;
    }

    private void UpdateTileCountUI()
    {
        if (tileCountText == null)
        {
            Debug.LogWarning("TileCountText is not assigned in BoardManager.");
            return;
        }

        int whiteCount = 0;
        int blueCount = 0;
        int orangeCount = 0;

        // allBoxes 배열을 순회하며 각 색상별 개수 카운트
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (allBoxes[x, y] != null) // 해당 셀에 박스가 존재하면
                {
                    switch (allBoxes[x, y].boxColor)
                    {
                        case BoxColor.White:
                            whiteCount++;
                            break;
                        case BoxColor.LightBlue:
                            blueCount++;
                            break;
                        case BoxColor.LightOrange:
                            orangeCount++;
                            break;
                    }
                }
            }
        }

        // UI 텍스트 업데이트
        tileCountText.text = $"White: {whiteCount}  Blue: {blueCount}  Orange: {orangeCount}";
    }

    private System.Collections.IEnumerator FillEmptySpaces()
    {
        yield return new WaitForSeconds(0.2f); // 박스 파괴 애니메이션 시간 등 고려

        // 1. 위에 있는 박스들 아래로 내리기 (낙하)
        for (int x = 0; x < gridWidth; x++)
        {
            int emptySpacesInColumn = 0;
            for (int y = 0; y < gridHeight; y++) // 아래에서부터 위로 스캔
            {
                if (allBoxes[x, y] == null)
                {
                    emptySpacesInColumn++;
                }
                else if (emptySpacesInColumn > 0) // 위에 박스가 있고 아래에 빈 공간이 있다면
                {
                    Box boxToMove = allBoxes[x, y];

                    // 박스가 앵커 상태라면 이동하지 않음
                    // (앵커 상태는 낙하 애니메이션에서 제외되도록 함)
                    if (boxToMove.isAnchored)
                    {
                        continue;
                    }

                    allBoxes[x, y - emptySpacesInColumn] = boxToMove; // 배열상 위치 이동
                    allBoxes[x, y] = null; // 이전 위치는 비움

                    boxToMove.y = y - emptySpacesInColumn; // 박스의 y 좌표 업데이트
                    boxToMove.MoveTo(GetWorldPosition(x, boxToMove.y), 0.3f); // 새 위치로 이동
                }
            }
        }

        yield return new WaitForSeconds(0.4f); // 낙하 애니메이션 시간 등 고려

        // 2. 최상단 빈 공간에 새 박스 생성
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (allBoxes[x, y] == null)
                {
                    SpawnNewBox(x, y);
                    // TODO: 새로 생성된 박스도 떨어지는 애니메이션 추가 가능
                    Box newBox = allBoxes[x, y];
                    Vector3 startPos = GetWorldPosition(x, gridHeight); // 화면 위에서 떨어지는 것처럼
                    startPos.z = 0; // Z 위치를 0으로 강제
                    newBox.transform.position = startPos;

                    Vector3 endPos = GetWorldPosition(x, y);
                    endPos.z = 0; // Z 위치를 0으로 강제
                    newBox.MoveTo(endPos, 0.3f);
                }
            }
        }
        // TODO: 매치 가능한 상태인지 다시 확인하는 로직 (Shuffle 등)
    }
    
    // 다음 턴이 시작될 때 앵커를 해제하는 함수
    public void UnanchorKnight() // <--- 이 함수 전체를 새로 추가!
    {
        if (theKnight != null)
        {
            theKnight.isAnchored = false;
        }
    }
}
