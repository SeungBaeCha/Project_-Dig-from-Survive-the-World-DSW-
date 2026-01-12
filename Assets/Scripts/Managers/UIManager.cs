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
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private CraftingWindow craftingWindow;

    [Header("튜토리얼 (여러 페이지)")]
    [SerializeField] private GameObject tutorialContainer; // 튜토리얼 페이지들의 부모 오브젝트
    [SerializeField] private List<GameObject> tutorialPages; // 튜토리얼 페이지들
    private int currentPageIndex = 0;

    [Header("게임 HUD")]
    [SerializeField] private GameObject gameHUD; // 체력, 배고픔, 크로스헤어 등을 포함하는 HUD

    [Header("알림 메시지")]
    [SerializeField] private TextMeshProUGUI notificationText;
    
    public static bool IsUIOpen { get; private set; }

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
        if (!GameManager.Instance.isGameStarted) return; // 게임 시작 후에만 가능

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 튜토리얼 패널은 ESC로 닫지 않는다.
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

    private void UpdateCursorAndGameState()
    {
        // 튜토리얼 컨테이너가 활성화되어 있는지도 UI 열림 상태에 포함시킨다.
        IsUIOpen = inventoryUI.IsOpen() || craftingWindow.IsOpen() || (tutorialContainer != null && tutorialContainer.activeSelf);

        if (IsUIOpen)
        {
            Time.timeScale = 0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
