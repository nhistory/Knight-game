using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BoxColor { White, LightBlue, LightOrange, Knight }
public class Box : MonoBehaviour
{
    public BoxColor boxColor;       // 이 박스의 색상 (Inspector에서 설정 가능)
    public int x;                   // 그리드 상의 x 좌표
    public int y;                   // 그리드 상의 y 좌표
    public bool isMatched = false;  // 매치되어 사라질 박스인지 여부

    public bool isKnight = false; // 이 타일이 기사인지 확인하는 플래그

    private SpriteRenderer spriteRenderer;
    private BoardManager board; // BoardManager와 통신하기 위한 참조

    // private static bool isDragging = false;
    // private static List<Box> currentMatchedBoxes;
    // private static Box dragOrigin = null;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError($"SpriteRenderer not found on {gameObject.name}!");
        }
    }

    void Start()
    {
        // // 씬에 있는 BoardManager 스크립트를 자동으로 찾아 연결합니다.
        // board = FindObjectOfType<BoardManager>();
        // if (board == null)
        // {
        //     Debug.LogError("BoardManager not found in scene!");
        // }
    }

    // 마우스를 처음 클릭했을 때 호출됩니다.
    // private void OnMouseDown()
    // {
    //     // 1. 핵심 로직: 이 타일이 기사가 아니면, 아무것도 하지 않고 즉시 함수를 종료합니다.
    //     if (!isKnight)
    //     {
    //         return;
    //     }

    //     // 2. 기사 타일을 클릭했다면, 드래그를 시작하고 매치 리스트를 새로 만듭니다.
    //     isDragging = true;
    //     currentMatchedBoxes = new List<Box>();
    //     dragOrigin = this; // 드래그 시작 위치를 현재 박스로 설정
    //     Debug.Log("Drag Started from Knight!");
    // }

    // // 마우스 버튼을 누른 상태로 자신의 콜라이더 위로 지나갈 때 호출됩니다.
    // private void OnMouseOver()
    // {
    //     // 드래그 중인 상태일 때만 아래 로직을 실행합니다.
    //     if (isDragging)
    //     {
    //         if (this == dragOrigin)
    //         {
    //             return;
    //         }
    //         // 아직 매치 리스트에 없고, 기사가 아닌 일반 타일일 경우에만 추가합니다.
    //         if (!currentMatchedBoxes.Contains(this) && !isKnight)
    //         {
    //             // TODO: 매치 로직을 더 정교하게 만들 수 있습니다.
    //             // (예: 같은 색상만, 혹은 인접한 타일만 추가되도록)
    //             // 지금은 간단하게 드래그하는 모든 일반 타일을 추가합니다.

    //             // 시각적 피드백 (예: 타일을 살짝 크게 만듦)
    //             transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
    //             currentMatchedBoxes.Add(this);
    //         }
    //     }
    // }

    // // 마우스 버튼에서 손을 뗐을 때 (어디서 떼든) 호출됩니다.
    // private void OnMouseUp()
    // {
    //     // 드래그 중이었을 때만 아래 로직을 실행합니다.
    //     if (isDragging)
    //     {
    //         isDragging = false; // 드래그 상태를 종료합니다.
    //         dragOrigin = null; // 드래그 시작 위치를 초기화합니다.
            
    //         // 시각적 피드백을 원래 크기로 되돌립니다.
    //         if (currentMatchedBoxes != null)
    //         {
    //             foreach (Box box in currentMatchedBoxes)
    //             {
    //                 if (box != null)
    //                 {
    //                     box.transform.localScale = Vector3.one;
    //                 }
    //             }
    //         }

    //         // 매치된 타일이 하나라도 있으면 BoardManager에 처리를 요청합니다.
    //         if (board != null && currentMatchedBoxes.Count > 0)
    //         {
    //             board.ProcessMatches(currentMatchedBoxes);
    //         }
    //     }
    // }


    // 박스 색상에 따라 스프라이트를 변경하는 함수 (필요하다면)
    // BoardManager에서 박스 생성 시 프리팹에 이미 스프라이트가 설정되어 있다면 이 함수는 당장 호출되지 않을 수 있습니다.
    // 하지만 나중에 게임 로직상 박스 색상이나 모양이 변해야 할 때 유용할 수 있습니다.
    public void SetSprite(Sprite newSprite)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = newSprite;
        }
        else
        {
            Debug.LogWarning($"Attempted to set sprite on {gameObject.name}, but SpriteRenderer is missing.");
        }
    }

    // 박스 파괴 (애니메이션 등 추가 가능)
    public void DestroyBox()
    {
        isMatched = true; // 이 플래그를 사용하여 중복 파괴를 방지하거나 다른 로직에 활용할 수 있습니다.
        Debug.Log($"Destroying Box: {gameObject.name} at ({x},{y}), Color: {boxColor}");

        // TODO: 여기에 파괴 애니메이션을 재생하는 코드를 추가할 수 있습니다.
        // 예: GetComponent<Animator>().SetTrigger("Destroy");

        // 애니메이션이 있다면 애니메이션 길이를 고려하여 Destroy 호출 시간을 조절합니다.
        // 즉시 파괴하고 싶다면 Destroy(gameObject, 0f); 또는 Destroy(gameObject);
        Destroy(gameObject, 0.1f); // 0.1초 후에 GameObject를 파괴합니다. (임시적인 딜레이)
    }


    // 특정 위치로 이동하는 함수 (낙하 효과)
    public void MoveTo(Vector3 targetPosition, float duration)
    {
        // TODO: 부드러운 이동 애니메이션 (예: LeanTween, DoTween 또는 Coroutine 사용)
        StartCoroutine(SmoothMove(targetPosition, duration));
    }

    private System.Collections.IEnumerator SmoothMove(Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPosition; // 정확한 위치로 설정
    }
}
