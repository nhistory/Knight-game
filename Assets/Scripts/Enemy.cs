using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int health = 3; // 적의 체력 (예: 3)
    private Box currentTile; // 자신이 올라가 있는 타일

    // 적이 처음 생성될 때 호출되는 초기화 함수
    public void Initialize(Box tile)
    {
        currentTile = tile;
        Debug.Log($"Enemy has spawned on a {tile.boxColor} tile at ({tile.x}, {tile.y})");
    }

    // 데미지를 받는 함수
    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        Debug.Log($"Enemy took {damageAmount} damage! Remaining HP: {health}");

        // TODO: 여기에 데미지 받는 애니메이션이나 효과음 재생 코드 추가 가능

        if (health <= 0)
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
