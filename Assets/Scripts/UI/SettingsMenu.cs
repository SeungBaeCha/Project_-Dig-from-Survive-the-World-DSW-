using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsMenu : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    [Tooltip("설정 메뉴 전체를 담고 있는 Panel 오브젝트")]
    [SerializeField] private GameObject settingsPanel;
    [Tooltip("마우스 감도 조절용 Slider")]
    [SerializeField] private Slider sensitivitySlider;
    [Tooltip("현재 감도 값을 표시할 TextMeshProUGUI")]
    [SerializeField] private TextMeshProUGUI sensitivityValueText;

    [Header("참조 스크립트")]
    [Tooltip("플레이어의 PlayerAim 스크립트")]
    [SerializeField] private PlayerAim playerAim;
    
    // 이 메뉴에서 사용할 기본 감도 값. PlayerAim의 초기값과 맞춰주는 것이 좋음.
    private const float DEFAULT_SENSITIVITY = 0.05f; 

    void Start()
    {
        // UI 리스너 연결
        if (sensitivitySlider != null)
        {
            // 슬라이더의 범위를 미세 조절에 맞게 설정 (예: 0.01 ~ 0.2)
            sensitivitySlider.minValue = 0.01f;
            sensitivitySlider.maxValue = 0.2f;
            sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        }

        // 항상 기본 감도 값으로 초기화
        if(sensitivitySlider != null) sensitivitySlider.value = DEFAULT_SENSITIVITY;
        OnSensitivityChanged(DEFAULT_SENSITIVITY);

        // 시작할 때 패널을 닫아 둠
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    private void OnSensitivityChanged(float value)
    {
        if (playerAim != null)
        {
            playerAim.SetSensitivity(value);
        }

        if (sensitivityValueText != null)
        {
            // 소수점 세 자리까지 표시하여 미세한 값 확인
            sensitivityValueText.text = value.ToString("F3");
        }
    }

    public void TogglePanel()
    {
        if (settingsPanel == null) return;

        bool isActive = !settingsPanel.activeSelf;
        settingsPanel.SetActive(isActive);

        // 패널이 열릴 때, 현재 감도 값으로 슬라이더를 다시 동기화
        if (isActive && playerAim != null)
        {
            sensitivitySlider.value = playerAim.GetSensitivity();
        }

        UIManager.Instance.UpdateCursorAndGameState();
    }

    public bool IsOpen()
    {
        return settingsPanel != null && settingsPanel.activeSelf;
    }
}
