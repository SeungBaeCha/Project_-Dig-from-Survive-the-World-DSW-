using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 제작 창 UI를 관리.
/// </summary>
public class CraftingWindow : MonoBehaviour
{
    [Header("UI 요소")]
    [Tooltip("제작 창 Panel 오브젝트")]
    [SerializeField] private GameObject windowPanel;
    
    [Header("레시피 디테일")]
    [Tooltip("재료 아이콘을 표시할 이미지들 (4개)")]
    [SerializeField] private List<Image> materialIcons;
    [Tooltip("재료 개수를 표시할 텍스트들 (4개)")]
    [SerializeField] private List<TextMeshProUGUI> materialQuantities;
    [Tooltip("결과물 아이콘을 표시할 이미지")]
    [SerializeField] private Image resultIcon;
    [Tooltip("제작 실행 버튼")]
    [SerializeField] private Button craftButton;

    private bool isOpen = false;

    private void Start()
    {
        // 시작할 땐 닫아둔다.
        isOpen = false;
        windowPanel.SetActive(false);

        // 버튼 클릭 이벤트에 함수 연결
        if (craftButton != null)
        {
            craftButton.onClick.AddListener(OnCraftButtonClicked);
        }
    }

    private void OnDestroy()
    {
        // 버튼 리스너 해제
        if (craftButton != null)
        {
            craftButton.onClick.RemoveListener(OnCraftButtonClicked);
        }
    }
    
    /// <summary>
    /// 제작 창의 활성화 상태를 토글한다.
    /// </summary>
    public void Toggle()
    {
        // isOpen 상태를 반전시키고, 그 상태에 따라 창을 열거나 닫는다.
        ToggleWindow(!isOpen);
    }

    /// <summary>
    /// 제작 창의 활성화 상태를 직접 제어한다.
    /// </summary>
    public void ToggleWindow(bool state)
    {
        isOpen = state;
        windowPanel.SetActive(isOpen);

        // 창이 열렸을 때만 UI를 새로고침한다.
        if (isOpen)
        {
            RefreshUI();
        }
    }

    /// <summary>
    /// 제작 창이 현재 열려있는지 여부를 반환한다.
    /// </summary>
    public bool IsOpen()
    {
        return isOpen;
    }

    /// <summary>
    /// 활성화된 레시피 정보를 기반으로 UI를 다시 그린다.
    /// </summary>
    private void RefreshUI()
    {
        // CraftingSystem에서 현재 활성화된 레시피를 가져온다.
        CraftingRecipe activeRecipe = CraftingSystem.Instance.ActiveRecipe;

        if (activeRecipe == null)
        {
            // 활성화된 레시피가 없으면 모든 UI를 비운다.
            ClearUI();
            return;
        }

        // --- UI 업데이트 ---

        // 1. 결과물 정보 업데이트
        resultIcon.sprite = activeRecipe.result.icon;
        resultIcon.gameObject.SetActive(true);

        // 2. 재료 정보 업데이트
        for (int i = 0; i < materialIcons.Count; i++)
        {
            if (i < activeRecipe.materials.Count)
            {
                // 표시할 재료가 있는 경우
                RequiredMaterial material = activeRecipe.materials[i];
                materialIcons[i].sprite = material.item.icon;
                materialIcons[i].gameObject.SetActive(true);
                materialQuantities[i].text = material.quantity.ToString();
            }
            else
            {
                // 표시할 재료가 더 이상 없는 경우, 해당 슬롯은 비운다.
                materialIcons[i].gameObject.SetActive(false);
                materialQuantities[i].text = "";
            }
        }
        
        // 3. 제작 버튼 활성화 (이 부분은 재료 충족 여부에 따라 다르게 할 수도 있음)
        craftButton.interactable = true; 
    }

    /// <summary>
    /// 모든 레시피 관련 UI를 비우고 숨긴다.
    /// </summary>
    private void ClearUI()
    {
        resultIcon.gameObject.SetActive(false);

        foreach (var icon in materialIcons)
        {
            icon.gameObject.SetActive(false);
        }
        foreach (var text in materialQuantities)
        {
            text.text = "";
        }

        craftButton.interactable = false;
    }

    /// <summary>
    /// 제작 버튼이 클릭되었을 때 호출될 함수
    /// </summary>
    private void OnCraftButtonClicked()
    {
        CraftingRecipe activeRecipe = CraftingSystem.Instance.ActiveRecipe;
        if (activeRecipe != null)
        {
            // CraftingSystem에 제작 요청
            bool success = CraftingSystem.Instance.CraftItem(activeRecipe);

            if (success)
            {
                Debug.Log("제작 성공!");
                // 성공했으므로 UI를 다시 그려서 바뀐 인벤토리 상태를 반영할 수 있음
                // 하지만 현재는 창을 닫는 것이 더 자연스러울 수 있음.
                // RefreshUI(); 
            }
            else
            {
                Debug.Log("제작 실패! (재료 부족 등)");
            }
        }
    }
}