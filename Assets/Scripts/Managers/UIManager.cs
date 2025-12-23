using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;

/// <summary>
/// 게임의 전반적인 UI (인벤토리, 제작 창 등)를 관리하고 입력을 처리하는 싱글턴 매니저.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("관리할 UI 창")]
    [Tooltip("인벤토리 UI를 관리하는 InventoryUI 스크립트")]
    [SerializeField] private InventoryUI inventoryUI;
    [Tooltip("제작 창 UI를 관리하는 CraftingWindow 스크립트")]
    [SerializeField] private CraftingWindow craftingWindow;

    [Header("알림 메시지")]
    [Tooltip("화면에 잠시 표시될 알림 메시지 TextMeshPro UI")]
    [SerializeField] private TextMeshProUGUI notificationText;
    
    /// <summary>
    /// UI 창(인벤토리, 제작 창 등)이 하나라도 열려있는지 여부를 나타냅니다.
    /// 다른 스크립트에서 플레이어 입력 등을 막는 데 사용할 수 있습니다.
    /// </summary>
    public static bool IsUIOpen { get; private set; }

    private PlayerInputActions inputActions;
    private Coroutine notificationCoroutine; // 현재 실행중인 알림 코루틴 참조

    private void Awake()
    {
        // 싱글턴 패턴
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        inputActions = new PlayerInputActions();
        inputActions.Player.Inventory.performed += _ => ToggleInventory();
        inputActions.Player.Crafting.performed += _ => ToggleCrafting();
    }

    private void Start()
    {
        // 게임 시작 시 알림 텍스트를 비활성화합니다.
        if (notificationText != null)
        {
            notificationText.gameObject.SetActive(false);
        }
        // 게임 시작 시 커서를 숨기고 잠급니다.
        UpdateCursorAndGameState();
    }
    
    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }
    
    /// <summary>
    /// 화면에 짧은 알림 메시지를 표시합니다.
    /// </summary>
    /// <param name="message">표시할 메시지</param>
    /// <param name="duration">메시지 표시 시간(초)</param>
    public void ShowNotification(string message, float duration = 2f)
    {
        if (notificationText == null) return;

        // 이전에 실행중인 알림 코루틴이 있다면 중지
        if (notificationCoroutine != null)
        {
            StopCoroutine(notificationCoroutine);
        }
        
        notificationText.text = message;
        notificationText.gameObject.SetActive(true);

        // 새 알림 코루틴 시작
        notificationCoroutine = StartCoroutine(NotificationCoroutine(duration));
    }

    /// <summary>
    /// 알림 메시지를 잠시 보여주고 서서히 사라지게 하는 코루틴
    /// </summary>
    private IEnumerator NotificationCoroutine(float duration)
    {
        // 텍스트를 완전히 보이게 설정
        notificationText.color = new Color(notificationText.color.r, notificationText.color.g, notificationText.color.b, 1);
        
        // 지속 시간의 절반만큼 대기
        yield return new WaitForSeconds(duration / 2);

        // 서서히 사라지는 효과
        float fadeDuration = duration / 2;
        float timer = 0;
        Color startColor = notificationText.color;

        while (timer < fadeDuration)
        {
            // 게임이 멈췄을 때도 UI가 작동하도록 unscaledDeltaTime 사용
            timer += Time.unscaledDeltaTime; 
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            notificationText.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        notificationText.gameObject.SetActive(false);
    }

    private void ToggleInventory()
    {
        // Tab 키는 인벤토리만 토글한다.
        inventoryUI.ToggleWindow(!inventoryUI.IsOpen());
        UpdateCursorAndGameState();
    }

    private void ToggleCrafting()
    {
        // C 키는 제작창만 토글한다.
        craftingWindow.ToggleWindow(!craftingWindow.IsOpen());
        UpdateCursorAndGameState();
    }

    private void Update()
    {
        // 'ESC' 키를 누르면 모든 창을 닫는다.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (inventoryUI.IsOpen())
            {
                inventoryUI.ToggleWindow(false);
            }
            if (craftingWindow.IsOpen())
            {
                craftingWindow.ToggleWindow(false);
            }
            UpdateCursorAndGameState();
        }
    }

    /// <summary>
    /// UI 창의 열림 상태에 따라 게임 시간, 마우스 커서 상태를 업데이트합니다.
    /// </summary>
    private void UpdateCursorAndGameState()
    {
        IsUIOpen = inventoryUI.IsOpen() || craftingWindow.IsOpen();

        if (IsUIOpen)
        {
            // 창이 하나라도 열려있으면: 게임 시간을 멈추고, 커서를 보이게 합니다.
            Time.timeScale = 0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            // 모든 창이 닫혀있으면: 게임 시간을 원래대로 돌리고, 커서를 숨깁니다.
            Time.timeScale = 1f;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}