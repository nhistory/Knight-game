// Enemy.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public int maxHealth = 3;   // 최대 체력
    private int currentHealth;  // 현재 체력

    private Box currentTile; // 자신이 올라가 있는 타일

    [Header("UI")]
    public Slider healthBarSlider; // Inspector에서 연결할 체력 바 슬라이더

    void Update()
    {
        // 만약 내가 따라다녀야 할 타일(currentTile)이 정해져 있다면,
        if (currentTile != null)
        {
            // 내 위치를 그 타일의 위치와 일치시킨다. (Z값만 다르게 해서 위에 보이도록)
            Vector3 targetPosition = currentTile.transform.position;
            targetPosition.z = -2f; // Z값은 항상 -2로 유지 (기사보다 앞에 보이게)
            transform.position = targetPosition;
        }
    }

    // 적이 처음 생성될 때 호출되는 초기화 함수
    public void Initialize(Box tile)
    {
        currentTile = tile;

        // 체력 초기화
        currentHealth = maxHealth;

        // 슬라이더 초기 설정
        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = maxHealth; // 슬라이더의 최대값을 최대 체력으로 설정
            healthBarSlider.value = currentHealth; // 슬라이더의 현재 값을 현재 체력으로 설정
        }

        Debug.Log($"Enemy has spawned on a {tile.boxColor} tile at ({tile.x}, {tile.y}) with {currentHealth}/{maxHealth} HP.");
    }

    // 데미지를 받는 함수
    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        // 체력이 0보다 아래로 내려가지 않도록 방지
        currentHealth = Mathf.Max(currentHealth, 0);

        // 이제 'damageAmount' 변수를 정상적으로 사용할 수 있습니다.
        Debug.Log($"Enemy took {damageAmount} damage! Remaining HP: {currentHealth}");

        if (healthBarSlider != null)
        {
            healthBarSlider.value = currentHealth;
        }

        // TODO: 여기에 데미지 받는 애니메이션이나 효과음 재생 코드 추가 가능

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 사망 처리 함수
    private void Die()
    {
        Debug.Log("Enemy has been defeated!");
        
        // 자신이 서 있던 타일에게 이제 적이 없다고 알려줌
        if (currentTile != null)
        {
            currentTile.enemyOnTop = null; 
        }

        // TODO: 여기에 사망 애니메이션이나 파티클 효과 재생 코드 추가 가능

        Destroy(gameObject); // 적 오브젝트 파괴
    }
}
