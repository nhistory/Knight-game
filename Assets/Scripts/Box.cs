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
    public bool isAnchored = false; // 이 타일이 고정되어 있는지 확인하는 플래그

    private SpriteRenderer spriteRenderer;

    [Header("Board Variables")]
    public BoardManager board; // BoardManager와 통신하기 위한 참조

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
    }


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
    public float moveSpeed = 0.5f; // 기사 이동 속도
    private Coroutine moveCoroutine; // 현재 진행중인 이동 코루틴 저장

    // InputManager에서 호출할 이동 시작 메서드
    public Coroutine StartMove(List<Vector3> path, int finalX, int finalY)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        moveCoroutine = StartCoroutine(MoveAlongPath(path, finalX, finalY));
        return moveCoroutine;
    }

    // 경로를 따라 순차적으로 이동하는 코루틴
    private IEnumerator MoveAlongPath(List<Vector3> path, int finalX, int finalY)
    {
        // 경로의 첫 번째 위치는 기사의 현재 위치이므로, 두 번째부터 따라갑니다.
        for (int i = 1; i < path.Count; i++)
        {
            Vector3 targetPosition = path[i];
            targetPosition.z = -1; // 기사가 항상 위에 보이도록 Z 위치를 -1로 고정

            while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                yield return null; // 다음 프레임까지 대기
            }
            transform.position = targetPosition; // 정확한 위치로 보정
        }

        // 논리적 위치 업데이트는 InputManager에서 처리하므로 여기서 제거합니다.
        // 이 코루틴은 이제 순수하게 시각적 이동만 담당합니다.
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
