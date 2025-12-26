using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro를 사용하기 위함

/// <summary>
/// 제작 창(CraftingWindow)에 표시되는 개별 레시피 슬롯.
/// </summary>
public class CraftingSlot : MonoBehaviour
{
    [Header("UI 요소")]
    [Tooltip("결과물 아이템의 아이콘을 표시하는 이미지")]
    [SerializeField] private Image resultIcon;
    [Tooltip("결과물 아이템의 이름을 표시하는 텍스트")]
    [SerializeField] private TextMeshProUGUI resultNameText;
    [Tooltip("제작 버튼")]
    [SerializeField] private Button craftButton;

    // 이 슬롯이 표시하는 레시피
    private CraftingRecipe currentRecipe;

    /// <summary>
    /// 슬롯을 특정 레시피로 초기화하고 UI를 설정하는 함수.
    /// </summary>
    public void Initialize(CraftingRecipe recipe)
    {
        currentRecipe = recipe;
        
        // UI 갱신
        resultIcon.sprite = recipe.result.icon;
        resultNameText.text = recipe.result.itemName;
        
        // 버튼 클릭 이벤트에 Craft() 함수 연결
        craftButton.onClick.AddListener(OnCraftButtonClicked);
    }

    /// <summary>
    /// 제작 버튼을 클릭했을 때 호출되는 함수.
    /// </summary>
    private void OnCraftButtonClicked()
    {
        if (currentRecipe != null)
        {
            // CraftingSystem에 제작 요청
            bool success = CraftingSystem.Instance.CraftItem(currentRecipe);

            if (success)
            {
                // 제작 성공 피드백(ex. 사운드, 이펙트)
                Debug.Log("제작 성공!");
            }
            else
            {
                // 제작 실패 피드백(ex. 재료 부족 사운드)
                Debug.Log("제작 실패!");
            }
        }
    }

    // 슬롯이 파괴될 때 버튼 리스너를 정리해서 메모리 누수를 방지
    private void OnDestroy()
    {
        if (craftButton != null)
        {
            craftButton.onClick.RemoveListener(OnCraftButtonClicked);
        }
    }
}