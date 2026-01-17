using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic; // List를 사용하기 위해 추가
using TMPro;

/// <summary>
/// 게임의 전반적인 UI (인벤토리, 제작 창, HUD 등)를 관리하고 입력 처리하는 싱글턴 매니저.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI 창")]
    [SerializeField] public InventoryUI inventoryUI;
    [SerializeField] private CraftingWindow craftingWindow;
    [SerializeField] private SettingsMenu settingsMenu; // 설정 메뉴 참조 추가

    [Header("튜토리얼 (여러 페이지)")]
    [SerializeField] private GameObject tutorialContainer; // 튜토리얼 페이지들의 부모 오브젝트
    [SerializeField] private List<GameObject> tutorialPages; // 튜토리얼 페이지들
    private int currentPageIndex = 0;

    [Header("게임 HUD")]
    [SerializeField] private GameObject gameHUD; // 체력, 배고픔, 크로스헤어 등을 포함하는 HUD

    [Header("알림 메시지")]
    [SerializeField] private TextMeshProUGUI notificationText;
    
    [Header("보급 상자 알림")]
    [SerializeField] private TextMeshProUGUI supplyBoxNotificationText;
    private Coroutine supplyBoxNotificationCoroutine;
    
    public static bool IsUIOpen { get; private set; }
    public static bool IsGamePaused { get; private set; }

    private PlayerInputActions inputActions;
    private Coroutine notificationCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(transform.root.gameObject);
        }

        inputActions = new PlayerInputActions();
        inputActions.Player.Inventory.performed += _ => ToggleInventory();
        inputActions.Player.Crafting.performed += _ => ToggleCrafting();
    }

    private void Start()
    {
        // 게임 시작 시 모든 UI를 꺼둔다.
        if (tutorialContainer != null) tutorialContainer.SetActive(false);
        if (gameHUD != null) gameHUD.SetActive(false);
        if (notificationText != null) notificationText.gameObject.SetActive(false);
        if (supplyBoxNotificationText != null) supplyBoxNotificationText.gameObject.SetActive(false);
    
        // 게임 시작 시에는 커서를 보이지 않게 잠금
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    #region 튜토리얼 관련 메서드

    /// <summary>
    /// 튜토리얼 시퀀스를 시작한다.
    /// </summary>
    public void StartTutorial()
    {
        if (tutorialContainer == null || tutorialPages.Count == 0) return;

        currentPageIndex = 0;
        tutorialContainer.SetActive(true);
        
        // 모든 페이지를 일단 끈 후, 첫 페이지만 활성화
        for (int i = 0; i < tutorialPages.Count; i++)
        {
            tutorialPages[i].SetActive(i == currentPageIndex);
        }
        
        if (gameHUD != null) gameHUD.SetActive(false);
        UpdateCursorAndGameState();
    }

    /// <summary>
    /// '다음' 버튼으로 호출. 다음 튜토리얼 페이지를 보여준다.
    /// </summary>
    public void ShowNextPage()
    {
        if (currentPageIndex < tutorialPages.Count - 1)
        {
            tutorialPages[currentPageIndex].SetActive(false);
            currentPageIndex++;
            tutorialPages[currentPageIndex].SetActive(true);
        }
    }

    /// <summary>
    /// '이전' 버튼으로 호출. 이전 튜토리얼 페이지를 보여준다.
    /// </summary>
    public void ShowPreviousPage()
    {
        if (currentPageIndex > 0)
        {
            tutorialPages[currentPageIndex].SetActive(false);
            currentPageIndex--;
            tutorialPages[currentPageIndex].SetActive(true);
        }
    }

    #endregion

    /// <summary>
    /// 튜토리얼 종료 후 메인 게임 HUD를 표시한다.
    /// </summary>
    public void ShowGameHUD()
    {
        if (tutorialContainer != null) tutorialContainer.SetActive(false);
        if (gameHUD != null) gameHUD.SetActive(true);
    
        UpdateCursorAndGameState();
    }

    public void ShowNotification(string message, float duration = 2f)
    {
        if (notificationText == null || !GameManager.Instance.isGameStarted) return;

        if (notificationCoroutine != null)
        {
            StopCoroutine(notificationCoroutine);
        }
        
        notificationText.text = message;
        notificationText.gameObject.SetActive(true);
        notificationCoroutine = StartCoroutine(NotificationCoroutine(duration));
    }

    private IEnumerator NotificationCoroutine(float duration)
    {
        notificationText.color = new Color(notificationText.color.r, notificationText.color.g, notificationText.color.b, 1);
        yield return new WaitForSeconds(duration / 2);

        float fadeDuration = duration / 2;
        float timer = 0;
        Color startColor = notificationText.color;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime; 
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            notificationText.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        notificationText.gameObject.SetActive(false);
    }

    /// <summary>
    /// 보급 상자 생성 알림을 화면에 표시한다.
    /// </summary>
    /// <param name="message">표시할 메시지</param>
    /// <param name="duration">메시지가 표시될 총 시간 (초)</param>
    /// <param name="resetTimer">true일 경우, 기존 알림 코루틴을 중단하고 타이머를 리셋한다. false일 경우, 텍스트만 업데이트하고 기존 타이머를 유지한다.</param>
    public void ShowSupplyBoxNotification(string message, float duration = 4f, bool resetTimer = true)
    {
        if (supplyBoxNotificationText == null)
        {
            Debug.LogWarning("Supply Box Notification Text가 지정되지 않아 알림을 표시할 수 없습니다.");
            return;
        }

        supplyBoxNotificationText.text = message;
        supplyBoxNotificationText.gameObject.SetActive(true);
        
        // resetTimer가 true이거나 기존 코루틴이 없으면 새로 시작하거나 다시 시작
        if (resetTimer || supplyBoxNotificationCoroutine == null)
        {
            if (supplyBoxNotificationCoroutine != null)
            {
                StopCoroutine(supplyBoxNotificationCoroutine);
            }
            supplyBoxNotificationCoroutine = StartCoroutine(SupplyBoxNotificationCoroutine(duration));
        }
        else
        {
            // resetTimer가 false이고 기존 코루틴이 있으면 텍스트만 업데이트하고 타이머는 유지
            // 이 경우, 코루틴은 이미 실행 중이므로 아무것도 할 필요 없음
        }
    }

    private IEnumerator SupplyBoxNotificationCoroutine(float duration)
    {
        // 메시지 완전 불투명하게 시작
        supplyBoxNotificationText.color = new Color(supplyBoxNotificationText.color.r, supplyBoxNotificationText.color.g, supplyBoxNotificationText.color.b, 1);
        yield return new WaitForSeconds(duration / 2); // 절반 시간 동안은 불투명 유지

        float fadeDuration = duration / 2;
        float timer = 0;
        Color startColor = supplyBoxNotificationText.color;

        // 남은 절반 시간 동안 서서히 사라지게 함
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime; 
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            supplyBoxNotificationText.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        supplyBoxNotificationText.gameObject.SetActive(false);
    }

    private void ToggleInventory()
    {
        if (!GameManager.Instance.isGameStarted) return; // 게임 시작 후에만 가능
        inventoryUI.ToggleWindow();
        UpdateCursorAndGameState();
    }

    private void ToggleCrafting()
    {
        if (!GameManager.Instance.isGameStarted) return; // 게임 시작 후에만 가능
        craftingWindow.Toggle();
        UpdateCursorAndGameState();
    }

    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.isGameStarted) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 다른 UI 창이 열려있으면 그것부터 닫는다.
            if (inventoryUI != null && inventoryUI.IsOpen())
            {
                inventoryUI.ToggleWindow(false);
            }
            else if (craftingWindow != null && craftingWindow.IsOpen())
            {
                craftingWindow.ToggleWindow(false);
            }
            // 닫을 다른 창이 없다면 설정 메뉴를 토글한다.
            else if (settingsMenu != null)
            {
                settingsMenu.TogglePanel();
            }
        }
    }

    public void UpdateCursorAndGameState()
    {
        // --- 상태 정의 ---
        // 1. 게임을 멈춰야 하는 경우: 인벤토리, 제작창, 또는 설정 메뉴가 열렸을 때, 또는 튜토리얼이 활성화되었을 때
        bool shouldPause = (inventoryUI != null && inventoryUI.IsOpen()) ||
                             (craftingWindow != null && craftingWindow.IsOpen()) ||
                             (settingsMenu != null && settingsMenu.IsOpen()) ||
                             (tutorialContainer != null && tutorialContainer.activeSelf);

        // 2. 커서를 보여줘야 하는 경우: 게임을 멈추게 하는 UI가 열렸거나, 튜토리얼 또는 상자 UI가 열렸을 때
        bool shouldShowCursor = shouldPause || 
                                  (tutorialContainer != null && tutorialContainer.activeSelf) ||
                                  (LootBoxUI.Instance != null && LootBoxUI.Instance.lootBoxWindow.activeSelf);

        // IsUIOpen은 이제 커서가 보여야 하는 모든 경우를 의미 (플레이어의 다른 행동을 막기 위함)
        IsUIOpen = shouldShowCursor;
        // IsGamePaused는 실제로 게임 시간이 멈췄는지 여부를 의미
        IsGamePaused = shouldPause;

        // --- 상태 적용 ---
        // 게임 시간 제어 (shouldPause 조건만 사용)
        Time.timeScale = shouldPause ? 0f : 1f;

        // 커서 제어 (shouldShowCursor 조건 사용)
        if (shouldShowCursor)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    /// <summary>
    /// 플레이어의 인벤토리, 제작 등 UI 관련 입력 액션을 비활성화한다.
    /// 게임 오버와 같이 모든 플레이어 입력을 막아야 할 때 사용된다.
    /// </summary>
    public void DisablePlayerInput()
    {
        inputActions.Player.Disable();
    }

    /// <summary>
    /// 플레이어의 인벤토리, 제작 등 UI 관련 입력 액션을 다시 활성화한다.
    /// </summary>
    public void EnablePlayerInput()
    {
        inputActions.Player.Enable();
    }
}
