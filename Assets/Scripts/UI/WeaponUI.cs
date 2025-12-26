using UnityEngine;
using TMPro; // TextMeshPro를 사용하기 위해 네임스페이스 추가
using System; // Action 이벤트를 사용하기 위해 추가
using System.Collections; // Coroutine을 사용하기 위해 추가

/// <summary>
/// 현재 장착된 무기의 UI (이름, 총알 수 및 재장전 피드백, 총알 부족 등)를 화면에 표시하는 역할을 한다.
/// WeaponHold 스크립트와 연동하여 작동한다.
/// </summary>
public class WeaponUI : MonoBehaviour
{
    // --- 이벤트 정의 ---
    // Gun 스크립트에서 총알이 없을 때 발사를 시도하면 이 이벤트를 발생시킨다.
    public static Action OnFireWithEmptyAmmo;

    [Header("참조 설정")]
    [Tooltip("플레이어에 붙어있는 WeaponHold 스크립트를 연결하세요.")]
    public WeaponHold weaponHold;
    [Tooltip("무기 이름을 표시할 TextMeshPro UI 오브젝트를 연결하세요.")]
    public TextMeshProUGUI weaponNameText;
    [Tooltip("총알 정보를 표시할 TextMeshPro UI 오브젝트를 연결하세요.")]
    public TextMeshProUGUI ammoText;
    [Tooltip("총알 부족 메시지를 표시할 TextMeshPro UI 오브젝트를 연결하세요.")]
    public TextMeshProUGUI noAmmoText; // 총알 부족 텍스트 참조 추가

    // 현재 UI가 추적하고 있는 Gun 스크립트의 캐시
    private Gun currentGun;
    // 총알 부족 메시지 코루틴이 이미 실행 중인지 확인하기 위한 변수
    private Coroutine noAmmoCoroutine;

    // 스크립트가 활성화될 때 호출
    private void OnEnable()
    {
        // Gun에서 보낸 "총알 없음" 신호(이벤트)를 받았을 때 ShowNoAmmoMessage 함수를 실행하도록 등록
        OnFireWithEmptyAmmo += ShowNoAmmoMessage;
    }

    // 스크립트가 비활성화될 때 호출
    private void OnDisable()
    {
        // 등록했던 이벤트 리스너를 해제 (메모리 누수 방지)
        OnFireWithEmptyAmmo -= ShowNoAmmoMessage;
    }

    void Start()
    {
        // 필수 참조들이 모두 연결되었는지 확인
        if (weaponHold == null || weaponNameText == null || ammoText == null || noAmmoText == null)
        {
            Debug.LogError("WeaponUI: 필요한 모든 참조가 인스펙터에 연결되지 않았다.");
            enabled = false; 
            return;
        }
        
        // 게임 시작 시 UI를 비어있는 상태로 초기화
        UpdateUI(null);
        // 총알 부족 텍스트는 처음에 보이지 않도록 비활성화
        noAmmoText.gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        GameObject equippedWeaponObject = weaponHold.equippedWeapon;
        Gun equippedGun = null;

        if (equippedWeaponObject != null)
        {
            equippedGun = equippedWeaponObject.GetComponent<Gun>();
        }

        if (currentGun != equippedGun)
        {
            UpdateUI(equippedGun);
        }

        if (currentGun != null && ammoText.enabled)
        {
            UpdateAmmoText();
        }
    }

    private void UpdateUI(Gun newGun)
    {
        currentGun = newGun;

        if (currentGun != null)
        {
            weaponNameText.gameObject.SetActive(true);
            ammoText.gameObject.SetActive(true);
            weaponNameText.text = currentGun.gunData.itemName;
        }
        else
        {
            weaponNameText.gameObject.SetActive(false);
            ammoText.gameObject.SetActive(false);
        }
    }
    
    private void UpdateAmmoText()
    {
        // 총이 사용되는 총알 종류가 지정되어 있고, 인벤토리 참조가 유효한 경우
        if (currentGun.gunData.ammoType != null && currentGun.playerInventory != null)
        {
            // 인벤토리에서 해당 총알의 총 개수를 가져온다.
            int totalCarriedAmmo = currentGun.playerInventory.GetItemQuantity(currentGun.gunData.ammoType);
            ammoText.text = $"{currentGun.currentAmmo} / {totalCarriedAmmo}";
        }
        else
        {
            // 총알 아이템을 사용하지 않는 경우, 기존 방식으로 표시한다.
            ammoText.text = $"{currentGun.currentAmmo} / {currentGun.gunData.maxAmmo}";
        }
    }

    // --- 총알 부족 메시지 관리 로직 ---

    /// <summary>
    /// "총알 없음" 신호를 받았을 때 호출되는 함수
    /// </summary>
    private void ShowNoAmmoMessage()
    {
        // 코루틴이 이미 실행 중이라면 중복 실행을 막는다 (연달아 클릭 시 메시지가 여러 번 겹치는 것 방지)
        if (noAmmoCoroutine != null)
        {
            // 기존 코루틴을 중지
            StopCoroutine(noAmmoCoroutine);
        }
        // 새로운 코루틴을 시작하고, 나중에 중지할 수 있도록 변수에 저장
        noAmmoCoroutine = StartCoroutine(ShowNoAmmoCoroutine());
    }

    /// <summary>
    /// 총알 부족 텍스트를 일시 보여주다가 사라지게 하는 코루틴
    /// </summary>
    private IEnumerator ShowNoAmmoCoroutine()
    {
        // 1. 총알 부족 텍스트를 활성
        noAmmoText.gameObject.SetActive(true);
        
        // 2. 1.5초 동안 기다린다.
        yield return new WaitForSeconds(1.5f);
        
        // 3. 텍스트를 다시 비활성화
        noAmmoText.gameObject.SetActive(false);

        // 4. 코루틴이 끝났으므로 변수를 null로 초기화
        noAmmoCoroutine = null;
    }
}