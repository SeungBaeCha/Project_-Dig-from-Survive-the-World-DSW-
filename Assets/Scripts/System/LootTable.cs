using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 하나의 아이템에 대한 드랍 정보
/// </summary>
[System.Serializable]
public class LootItem
{
    // 주석: 드랍될 아이템의 프리팹
    public GameObject itemPrefab;

    // 주석: 아이템이 드랍될 확률 (0.0 ~ 1.0 사이, 1.0 = 100%)
    [Range(0f, 1f)]
    public float dropChance = 1.0f;

    // 주석: 드랍될 아이템의 최소/최대 개수
    public int minQuantity = 1;
    public int maxQuantity = 1;
}

/// <summary>
/// 여러 LootItem을 포함하는 드랍 테이블. ScriptableObject로 만들어 에셋으로 관리
/// </summary>
[CreateAssetMenu(fileName = "NewLootTable", menuName = "System/Loot Table")]
public class LootTable : ScriptableObject
{
    // 주석: 이 테이블에 포함된 아이템 목록
    public List<LootItem> items;

    /// <summary>
    /// 이 테이블을 기반으로 아이템을 드랍시킵니다.
    /// </summary>
    /// <param name="position">아이템이 드랍될 월드 위치</param>
    public void SpawnLoot(Vector3 position)
    {
        // 테이블에 있는 모든 아이템을 순회
        foreach (var lootItem in items)
        {
            // 드랍 확률 체크 (Random.value는 0.0과 1.0 사이의 값을 반환)
            if (Random.value <= lootItem.dropChance)
            {




                // 드랍될 개수를 최소/최대값 사이에서 랜덤으로 결정
                int quantity = Random.Range(lootItem.minQuantity, lootItem.maxQuantity + 1);

                // 결정된 개수만큼 아이템 생성
                for (int i = 0; i < quantity; i++)
                {
                    // 아이템 프리팹이 설정되어 있을 때만 생성
                    if (lootItem.itemPrefab != null)
                    {
                        // 아이템을 지정된 위치에 생성
                        Instantiate(lootItem.itemPrefab, position, Quaternion.identity);
                    }
                }
            }
        }
    }
}
