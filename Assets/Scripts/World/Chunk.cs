using UnityEngine;

/// <summary>
/// 파괴 가능한 지형의 기본 단위인 '청크'를 관리하는 스크립트.
/// 이 스크립트는 Chunk 프리팹에 부착되어야함
/// </summary>
public class Chunk : MonoBehaviour
{
    // 주석: 청크의 체력. 1이면 한 번의 공격으로 파괴
    [Tooltip("청크의 체력. 1이면 한 번의 공격으로 파괴됩니다.")]
    public int health = 1;

    // 주석: 청크가 파괴될 때 사용할 아이템 드랍 테이블
    [Tooltip("청크가 파괴될 때 사용할 아이템 드랍 테이블입니다.")]
    public LootTable lootTable;

    /// <summary>
    /// 외부에서 이 청크에 데미지를 주기 위해 호출하는 함수
    /// </summary>
    /// <param name="damageAmount">입힐 데미지의 양</param>
    public void TakeDamage(int damageAmount)
    {
        // 주석: 받은 데미지만큼 체력을 감소
        health -= damageAmount;

        // 주석: 체력이 0 이하로 떨어졌는지 확인
        if (health <= 0)
        {
            // 주석: 체력이 다 닳으면 Die 함수를 호출하여 청크를 파괴
            Die();
        }
    }

    /// <summary>
    /// 청크가 파괴될 때 호출되는 함수
    /// </summary>
    private void Die()
    {
        // 주석: 할당된 드랍 테이블이 있는지 확인
        if (lootTable != null)
        {
            // 주석: 현재 청크의 위치에 아이템을 드랍
            lootTable.SpawnLoot(transform.position);
        }

        // 주석: 이 게임 오브젝트(청크)를 씬에서 파괴
        Destroy(gameObject);
    }
}
