using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BoxColor { White, LightBlue, LightOrange } // 박스 색상 종류 정의
public class Box : MonoBehaviour
{
    public BoxColor boxColor;       // 이 박스의 색상 (Inspector에서 설정 가능)
    public int x;                   // 그리드 상의 x 좌표
    public int y;                   // 그리드 상의 y 좌표
    public bool isMatched = false;  // 매치되어 사라질 박스인지 여부

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError($"SpriteRenderer not found on {gameObject.name}!");
        }
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
