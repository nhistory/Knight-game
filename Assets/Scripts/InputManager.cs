// InputManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InputManager : MonoBehaviour
{
    public LayerMask boxLayer; // 박스 오브젝트들이 있는 레이어 (Raycast에 사용)
    private Camera mainCamera;
    private BoardManager boardManager;

    private List<Box> selectedBoxes = new List<Box>();
    private BoxColor? currentDragColor = null; // 현재 드래그 중인 색상
    private bool isKnightDrag = false; // 기사 드래그 여부를 확인하는 변수

    [Header("UI")] // UI 관련 변수를 위한 헤더 추가
    public TextMeshProUGUI movesCountText; // Inspector에서 연결할 TextMeshPro UI 요소
    public TextMeshProUGUI turnsCountText; // Inspector에서 연결할 Turns TextMeshPro UI 요소
    private int currentTurnCount = 0; // 현재 턴 수를 저장하는 변수

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        boardManager = FindObjectOfType<BoardManager>(); // BoardManager 찾기
        if (boardManager == null) Debug.LogError("BoardManager not found!");

        InitializeTurnSystem();
        UpdateMovesCountUI();
    }

    void InitializeTurnSystem()
    {
        currentTurnCount = 0; // 게임 시작 시 턴 수 0으로 초기화
        UpdateTurnsCountUI(); // Turns UI 업데이트
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 터치 시작 또는 마우스 왼쪽 버튼 클릭
        {
            HandleTouchStart(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0) && isKnightDrag) // 드래그 중이고, '기사 드래그' 상태일 때만 HandleTouchDrag를 호출
        {
            HandleTouchDrag(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0) && isKnightDrag)
        {
            StartCoroutine(HandleTouchEnd());
        }
    }

    void HandleTouchStart(Vector3 screenPosition)
    {
        // 새로운 턴이 시작되었으므로, 이전 턴에 고정했던 기사의 앵커를 해제합니다.
        if (boardManager != null)
        {
            boardManager.UnanchorKnight();
        }

        ClearSelection();
        RaycastHit2D hit = Physics2D.Raycast(mainCamera.ScreenToWorldPoint(screenPosition), Vector2.zero, Mathf.Infinity, boxLayer);

        if (hit.collider != null)
        {
            Box box = hit.collider.GetComponent<Box>();
            if (box != null)
            {
                if (!box.isMatched && box.isKnight) // isKnight가 true일 때만 아래 로그가 찍힙니다.
                {
                    // Debug.Log("SUCCESS: Knight detected! Setting isKnightDrag to true.");
                    isKnightDrag = true;
                    selectedBoxes.Add(box);
                    HighlightBox(box, true);
                    UpdateMovesCountUI();
                }
            }
        }
        else
        {
            Debug.Log("HandleTouchStart: Clicked on empty space (or wrong layer).");
        }
    }

    void HandleTouchDrag(Vector3 screenPosition)
    {
        RaycastHit2D hit = Physics2D.Raycast(mainCamera.ScreenToWorldPoint(screenPosition), Vector2.zero, Mathf.Infinity, boxLayer);

        if (hit.collider != null)
        {
            Box box = hit.collider.GetComponent<Box>();

            if (box != null && !box.isKnight && !selectedBoxes.Contains(box))
            {
                // 1. 아직 드래그 색상이 정해지지 않았다면 (기사 다음에 오는 첫 타일)
                if (currentDragColor == null)
                {
                    Box knightBox = selectedBoxes[0];
                    bool isAdjacent = IsAdjacent(knightBox, box);

                    if (isAdjacent)
                    {
                        // Debug.Log("SUCCESS: Adding first tile and setting drag color!"); // 5. 성공 로그
                        currentDragColor = box.boxColor;
                        selectedBoxes.Add(box);
                        HighlightBox(box, true);
                        UpdateMovesCountUI();
                    }
                }
                // 2. 드래그 색상이 이미 정해져 있다면
                else if (box.boxColor == currentDragColor)
                {
                    Box lastBox = selectedBoxes[selectedBoxes.Count - 1];
                    bool isAdjacent = IsAdjacent(lastBox, box);
                    // Debug.Log($"Checking adjacency to LastBox ({lastBox.x},{lastBox.y}). Target: ({box.x},{box.y}). Is Adjacent: {isAdjacent}");

                    if (isAdjacent)
                    {
                        selectedBoxes.Add(box);
                        HighlightBox(box, true);
                        UpdateMovesCountUI();
                    }
                }
            }
        }
    }

    IEnumerator HandleTouchEnd()
    {
        bool processedMatch = false; // 실제로 매치가 처리되었는지 확인하는 플래그

        // 선택된 박스들 하이라이트 해제
        foreach (Box box in selectedBoxes)
        {
            if (box != null) HighlightBox(box, false); // 파괴되지 않은 박스만
        }

        // 유효한 매치가 있었는지 (예: 최소 2개 이상 선택) 확인 후 처리
        if (isKnightDrag && selectedBoxes.Count >= 2) // 최소 매치 개수 조건 (게임 규칙에 따라 조절)
        {
            Debug.Log("==================== Touch End Debug Start ====================");
            Debug.Log($"드래그한 타일 개수: {selectedBoxes.Count}개");
            foreach(var boxInPath in selectedBoxes)
            {
                // boxInPath가 null이 아닌지 먼저 확인 (안전장치)
                if (boxInPath != null)
                {
                    // 타일 위에 적이 있는지 확인하고 로그를 남깁니다.
                    if(boxInPath.enemyOnTop != null)
                    {
                        // 적이 있는 경우: 녹색으로 강조해서 출력
                        Debug.Log($"<color=green>Box at ({boxInPath.x},{boxInPath.y}) 에는 적 [{boxInPath.enemyOnTop.gameObject.name}] 이(가) 있습니다!</color>");
                    }
                    else
                    {
                        // 적이 없는 경우: 회색으로 출력
                        Debug.Log($"<color=grey>Box at ({boxInPath.x},{boxInPath.y}) 에는 적이 없습니다.</color>");
                    }
                }
            }
            Debug.Log("==================== Touch End Debug End ====================");

            if (boardManager != null)
            {
                // --- 기사 이동 및 타일 파괴 로직 (시작) ---
                Box knight = selectedBoxes[0];
                List<Vector3> movementPath = new List<Vector3>();
                foreach (Box box in selectedBoxes)
                {
                    movementPath.Add(box.transform.position);
                }

                // 데미지 판정 로직: 기사가 지나가는 경로에 적이 있는지 확인
                for (int i = 1; i < selectedBoxes.Count; i++) // 첫번째는 기사 시작위치이므로 제외
                {
                    Box pathTile = selectedBoxes[i];
                    // 1. 해당 타일 위에 적이 있는지 확인
                    // 2. 드래그한 타일 색상(currentDragColor)과 적이 서 있는 타일의 색상(pathTile.boxColor)이 같은지 확인
                    if (pathTile.enemyOnTop != null)
                    {
                        Debug.Log("Condition met! Damaging the enemy.");
                        pathTile.enemyOnTop.TakeDamage(1); // 적에게 데미지를 1 준다
                        // 한 번의 이동에 한 번만 데미지를 주려면 여기서 break;
                    }
                }

                List<Box> boxesToMatch = new List<Box>(selectedBoxes);
                boxesToMatch.RemoveAt(0);

                // 1. 기사의 시각적 이동 시작하고 끝날 때까지 대기
                Box finalDestination = selectedBoxes[selectedBoxes.Count - 1];
                yield return knight.StartMove(movementPath, finalDestination.x, finalDestination.y);

                // 2. 이동 완료 후 타일 파괴
                boardManager.ProcessMatches(boxesToMatch);

                // 3. 타일 파괴 후 기사의 논리적 위치 업데이트
                boardManager.UpdateBoxPosition(knight, finalDestination.x, finalDestination.y);
                // --- 기사 이동 및 타일 파괴 로직 (끝) ---
                processedMatch = true;
            }
        }

        // processedMatch가 true일 때만 턴을 증가 (게임 규칙에 따라 결정)
        if (processedMatch) // 매치가 성공했을 때만 턴을 증가
        {
            IncrementTurn();
        }

        ClearSelection(); // 선택 초기화 및 Moves UI 업데이트 포함
    }

    void IncrementTurn()
    {
        currentTurnCount++;
        UpdateTurnsCountUI();
    }

    void ClearSelection()
    {
        // 이전에 선택된 박스들 하이라이트 해제 (필요시)
        foreach (Box box in selectedBoxes)
        {
            if (box != null) HighlightBox(box, false); // 이미 파괴된 경우 null 체크
        }
        selectedBoxes.Clear();
        currentDragColor = null;
        isKnightDrag = false;
        UpdateMovesCountUI(); // 선택 초기화 시 Moves UI 업데이트
    }

    // 선택된 박스 개수를 UI에 업데이트하는 함수
    void UpdateMovesCountUI()
    {
        if (movesCountText != null)
        {
            movesCountText.text = $"Moves: {selectedBoxes.Count}";
        }
        else
        {
            Debug.LogWarning("MovesCountText_TM not assigned in InputManager.");
        }
    }

    // 턴 카운트 UI 업데이트 함수
    void UpdateTurnsCountUI()
    {
        if (turnsCountText != null)
        {
            turnsCountText.text = $"Turns: {currentTurnCount}";
        }
        else
        {
            Debug.LogWarning("TurnsCountText is not assigned in InputManager.");
        }
    }

    // 박스 선택/해제 시 시각적 피드백 (임시)
    void HighlightBox(Box box, bool highlight)
    {
        if (box == null) return;
        if (highlight)
        {
            box.transform.localScale = Vector3.one * 1.1f; // 살짝 크게
        }
        else
        {
            box.transform.localScale = Vector3.one; // 원래 크기로
        }
    }

    // 두 박스가 인접한지 확인 (상하좌우, 대각선 포함)
    bool IsAdjacent(Box box1, Box box2)
    {
        if (box1 == null || box2 == null) return false;
        bool xAdjacent = Mathf.Abs(box1.x - box2.x) <= 1;
        bool yAdjacent = Mathf.Abs(box1.y - box2.y) <= 1;
        return (xAdjacent && yAdjacent) && (box1 != box2);
    }
}
