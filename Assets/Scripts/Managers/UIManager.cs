using UnityEngine;
using UnityEngine.InputSystem;

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

    /// <summary>
    /// UI 창(인벤토리, 제작 창 등)이 하나라도 열려있는지 여부를 나타냅니다.
    /// 다른 스크립트에서 플레이어 입력 등을 막는 데 사용할 수 있습니다.
    /// </summary>
    public static bool IsUIOpen { get; private set; }

    private PlayerInputActions inputActions;

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