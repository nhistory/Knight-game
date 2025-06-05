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
        else if (Input.GetMouseButton(0) && currentDragColor.HasValue) // 드래그 중
        {
            HandleTouchDrag(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0) && selectedBoxes.Count > 0) // 터치 종료 또는 마우스 왼쪽 버튼 뗌
        {
            HandleTouchEnd();
        }
    }

    void HandleTouchStart(Vector3 screenPosition)
    {
        ClearSelection();
        RaycastHit2D hit = Physics2D.Raycast(mainCamera.ScreenToWorldPoint(screenPosition), Vector2.zero, Mathf.Infinity, boxLayer);

        if (hit.collider != null)
        {
            Box box = hit.collider.GetComponent<Box>();
            if (box != null && !box.isMatched)
            {
                selectedBoxes.Add(box);
                currentDragColor = box.boxColor;
                HighlightBox(box, true);
                UpdateMovesCountUI();
            }
        }
    }

    void HandleTouchDrag(Vector3 screenPosition)
    {
        RaycastHit2D hit = Physics2D.Raycast(mainCamera.ScreenToWorldPoint(screenPosition), Vector2.zero, Mathf.Infinity, boxLayer);

        if (hit.collider != null)
        {
            Box box = hit.collider.GetComponent<Box>();
            if (box != null && box.boxColor == currentDragColor && !selectedBoxes.Contains(box))
            {
                // 마지막으로 선택된 박스와 인접한지 확인 (선택사항, 대각선 포함 등 규칙 정하기)
                if (selectedBoxes.Count > 0 && IsAdjacent(selectedBoxes[selectedBoxes.Count - 1], box))
                {
                    selectedBoxes.Add(box);
                    // TODO: 박스 선택 시 시각적 피드백
                    HighlightBox(box, true);
                    UpdateMovesCountUI();
                }
            }
        }
    }

    void HandleTouchEnd()
    {
        bool processedMatch = false; // 실제로 매치가 처리되었는지 확인하는 플래그

        // 선택된 박스들 하이라이트 해제
        foreach (Box box in selectedBoxes)
        {
            if (box != null) HighlightBox(box, false); // 파괴되지 않은 박스만
        }

        // 유효한 매치가 있었는지 (예: 최소 2개 이상 선택) 확인 후 처리
        if (selectedBoxes.Count >= 2) // 최소 매치 개수 조건 (게임 규칙에 따라 조절)
        {
            if (boardManager != null)
            {
                boardManager.ProcessMatches(new List<Box>(selectedBoxes));
                processedMatch = true; // 매치 로직이 호출되었음을 표시
            }
        }

        // 유효한 드래그 완료(매치 처리 여부와 관계없이, 유효한 선택이 있었다면 턴으로 간주 가능)
        // 또는, processedMatch가 true일 때만 턴을 증가시킬 수도 있습니다. (게임 규칙에 따라 결정)
        if (selectedBoxes.Count > 0) // 드래그를 통해 하나라도 선택했었다면 턴으로 간주
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
