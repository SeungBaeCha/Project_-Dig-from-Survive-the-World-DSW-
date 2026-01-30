using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 3D 모델링으로 만든 상자 오브젝트에 이 스크립트를 추가해야 해.
public class LootBox : MonoBehaviour
{
    // 디자이너가 상자에 들어갈 수 있는 아이템 목록을 유니티 에디터에서 설정할 수 있도록 리스트를 만든다.
    [Header("Loot-Table")]
    [Tooltip("이 상자에서 나올 수 있는 모든 아이템의 목록")]
    public List<ItemData> possibleLoot; // 상자에서 나올 수 있는 아이템 후보 리스트

    [Tooltip("상자를 열었을 때 생성될 아이템의 최소 개수")]
    public int minItemsToDrop = 1; // 최소 드랍 아이템 수

    [Tooltip("상자를 열었을 때 생성될 아이템의 최대 개수")]
    public int maxItemsToDrop = 5; // 최대 드랍 아이템 수

    // 상자가 열렸을 때, 실제로 생성된 아이템들이 담길 리스트.
    // 이 리스트의 내용이 LootBoxUI에 표시될 거야.
    [Header("Generated-Loot")]
    public List<ItemData> currentLoot;

    private bool isOpened = false; // 상자가 이미 열렸는지 확인하는 변수

    /// <summary>
    /// 상자가 열릴 때 호출되는 함수.
    /// 내부에 있는 아이템을 랜덤으로 생성하고 UI를 연다.
    /// </summary>
    public void OpenBox()
    {
        // 상자가 처음 열리는 경우에만 아이템을 생성
        if (!isOpened)
        {
            GenerateLoot();
            isOpened = true;
        }

        // LootBoxUI 싱글톤 인스턴스를 찾아 Open 함수를 호출하고, 자기 자신(이 LootBox 인스턴스)을 넘겨준다.
        LootBoxUI.Instance.Open(this);
    }

    /// <summary>
    /// possibleLoot 리스트에서 정해진 수량만큼 랜덤 아이템을 뽑아 currentLoot 리스트에 채운다.
    /// </summary>
    private void GenerateLoot()
    {
        // currentLoot 리스트를 초기화한다.
        currentLoot = new List<ItemData>();

        // minItemsToDrop과 maxItemsToDrop 사이에서 랜덤한 개수를 정한다.
        int itemsToDrop = Random.Range(minItemsToDrop, maxItemsToDrop + 1);

        // possibleLoot 리스트가 비어있지 않은지 확인
        if (possibleLoot != null && possibleLoot.Count > 0)
        {
            // 정해진 개수만큼 반복하여 아이템을 추가
            for (int i = 0; i < itemsToDrop; i++)
            {
                // possibleLoot 리스트에서 랜덤한 인덱스를 선택
                int randomIndex = Random.Range(0, possibleLoot.Count);
                // 랜덤하게 선택된 아이템을 currentLoot 리스트에 추가
                currentLoot.Add(possibleLoot[randomIndex]);
            }
        }
        else
        {
            Debug.LogWarning("경고: LootBox에 'possibleLoot'이(가) 설정되지 않았습니다.");
        }
    }

    /// <summary>
    /// 플레이어가 LootSlot에서 아이템을 가져갈 때 호출될 함수.
    /// </summary>
    /// <param name="item">가져갈 아이템</param>
    public void RemoveItem(ItemData item)
    {
        if (currentLoot.Contains(item))
        {
            currentLoot.Remove(item);
        }
    }

    /// <summary>
    /// 이 LootBox 오브젝트가 파괴될 때 자동으로 호출된다.
    /// 만약 이 상자의 UI가 열려있다면, 그 UI를 닫도록 처리한다.
    /// </summary>
    private void OnDestroy()
    {
        // LootBoxUI 싱글톤 인스턴스가 아직 존재하는지 확인
        if (LootBoxUI.Instance != null)
        {
            // UI에 열려있는 상자가 지금 파괴되려는 이 상자인지 확인
            if (LootBoxUI.Instance.IsCurrentLootBox(this))
            {
                // 맞다면, UI를 닫도록 요청한다.
                LootBoxUI.Instance.Close();
            }
        }
    }
}
