using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro 사용을 위해 추가

public class HungerUI : MonoBehaviour
{
    [Header("UI 설정")]
    public Slider hungerSlider; // 배고픔을 표시할 슬라이더
    public TextMeshProUGUI hungerText; // 배고픔 텍스트 UI 참조

    [Header("참조")]
    private PlayerHealth playerHealth; // 플레이어의 체력 및 배고픔 정보

    void Start()
    {
        // 플레이어 객체를 찾아 PlayerHealth 컴포넌트를 가져옴
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerHealth = playerObject.GetComponent<PlayerHealth>();
        }

        if (hungerSlider == null)
        {
            Debug.LogError("Hunger Slider가 연결되지 않았습니다!");
        }
    }

    void Update()
    {
        // playerHealth와 UI 요소들이 모두 유효할 때만 UI 업데이트
        if (playerHealth != null && hungerSlider != null)
        {
            // 슬라이더의 값을 현재 배고픔 비율로 설정
            hungerSlider.value = playerHealth.currentHunger / playerHealth.maxHunger;

            // 배고픔 텍스트 업데이트
            if (hungerText != null)
            {
                hungerText.text = $"Hunger : {Mathf.CeilToInt(playerHealth.currentHunger)}";
            }
        }
    }
}
