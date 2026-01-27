using UnityEngine;

// 모든 총기 클래스가 상속받을 추상 클래스
public abstract class Gun : MonoBehaviour
{
    [SerializeField]
    public GunData gunData; // 이 총이 사용할 데이터(외부에서 isAutomatic을 확인해야 하므로 public으로 변경)

    [SerializeField]
    protected Transform firePoint; // 총알이 발사될 위치. 실제 총마다 가지게 됨.

    // --- 총알 관리 변수 추가 ---
    public int currentAmmo; // 현재 남은 총알 수 (UI 표시 등을 위해 public으로 설정)
    // --------------------------

    protected Collider playerCollider; // 플레이어의 콜라이더 참조
    public Inventory playerInventory; // 플레이어의 인벤토리 참조
    protected Camera playerCamera;   // 플레이어 카메라 참조

    // 발사 속도 제어를 위한 변수
    protected float nextFireTime;
    
    // 사운드 재생을 위한 변수
    protected AudioSource audioSource;

    // 컴포넌트가 활성화될 때 또는 게임 시작 시 호출
    // 자식 클래스에서 재정의(override)할 수 있도록 virtual로 선언
    protected virtual void Awake()
    {
        // 총이 처음 생성될 때 장탄 수를 0으로 초기화하여 재장전을 유도한다.
        currentAmmo = 0;
        
        // 사운드 컴포넌트 초기화
        // AudioSource 컴포넌트를 찾거나, 없으면 새로 추가한다.
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // 외부에서 플레이어 콜라이더를 설정하기 위한 메서드
    public void SetPlayerCollider(Collider collider)
    {
        playerCollider = collider;
        // 콜라이더를 통해 인벤토리 컴포넌트도 함께 찾아 저장해 둔다.
        playerInventory = collider.GetComponent<Inventory>();
    }

    // 외부에서 플레이어 카메라를 설정하기 위한 메서드
    public void SetPlayerCamera(Camera camera)
    {
        playerCamera = camera;
    }

    // 외부(Player)에서 발사를 시도할 때 호출될 메서드
    public void TryFire()
    {
        // --- 총알 수 체크 추가 ---
        // 현재 총알이 0발 이하면 발사 시도를 막는다.
        if (currentAmmo <= 0)
        {
            // "총알 없음" UI 알림을 띄우도록 신호를 보낸다.
            WeaponUI.OnFireWithEmptyAmmo?.Invoke();
            // (추후 추가) "딸깍" 하는 소리를 내거나 재장전 UI를 띄우는 등의 처리를 할 수 있다.
            return; 
        }
        // --------------------------

        // 현재 시간이 다음 발사 가능 시간보다 크거나 같으면 발사
        if (Time.time >= nextFireTime)
        {
            // 발사 사운드 재생. PlayOneShot을 사용해 여러 발의 사운드가 겹쳐서 재생될 수 있게 한다.
            if (gunData.shotSound != null)
            {
                audioSource.PlayOneShot(gunData.shotSound);
            }

            // 다음 발사 시간 계산 (1 / 초당 발사 수)
            nextFireTime = Time.time + 1f / gunData.fireRate;
            
            // --- 총알 감소 로직 추가 ---
            currentAmmo--; // 총알을 1발 소모한다.
            // --------------------------

            Fire(); // 실제 발사 로직 실행
        }
    }

    /// <summary>
    /// 인벤토리의 총알을 사용하여 총을 재장전한다.
    /// </summary>
    public void Reload()
    {
        // 이 총이 사용되는 총알 종류가 지정되지 않았다면 재장전 불가
        if (gunData.ammoType == null)
        {
            Debug.LogWarning($"'{gunData.name}'에 사용할 총알 종류(Ammo Type)가 지정되지 않았다.");
            return;
        }

        // 이미 총알이 가득 차 있다면 재장전할 필요가 없다.
        if (currentAmmo >= gunData.maxAmmo)
        {
            return;
        }

        // 플레이어 인벤토리를 찾을 수 없으면 재장전 불가
        if (playerInventory == null)
        {
            Debug.LogError("플레이어의 인벤토리를 찾을 수 없어 재장전할 수 없다.");
            return;
        }
        
        // 1. 채워야 할 총알의 양을 계산
        int ammoNeeded = gunData.maxAmmo - currentAmmo;

        // 2. 인벤토리에 있는 필요한 총알의 양을 확인
        int ammoInInventory = playerInventory.GetItemQuantity(gunData.ammoType);

        // 3. 실제로 재장전할 총알의 양을 결정 (채워야 할 양과 가진 양 중 더 적은 값)
        int ammoToReload = Mathf.Min(ammoNeeded, ammoInInventory);

        // 재장전할 총알이 없으면 함수 종료
        if (ammoToReload <= 0)
        {
            UIManager.Instance.ShowNotification("총알이 없다!", 1.5f);
            return;
        }

        // 4. 총의 현재 총알 수를 늘리고 인벤토리에서 사용한 만큼 총알 제거
        currentAmmo += ammoToReload;
        playerInventory.RemoveItem(gunData.ammoType, ammoToReload);

        UIManager.Instance.ShowNotification("재장전 완료!");
        
        // 재장전 사운드 재생
        if (gunData.reloadSound != null)
        {
            audioSource.PlayOneShot(gunData.reloadSound);
        }
    }

    // 각 총의 종류마다 다르게 구현될 실제 발사 로직 (추상 메서드)
    protected abstract void Fire();

    // 카메라 중앙을 기준으로 실제 발사 방향을 계산하는 메서드
    protected virtual Vector3 GetFireDirection()
    {
        // 카메라와 플레이어가 설정되었는지 확인
        if (playerCamera == null || playerCollider == null)
        {
            Debug.LogError("Gun에 플레이어 정보가 설정되지 않았다.");
            return transform.forward; // 기본값으로 오브젝트의 앞쪽 방향 반환
        }

        Vector3 direction;
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // 레이캐스트 실행 (플레이어 자신은 제외)
        float maxDistance = 300f; // 최대 사정거리

        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            // 레이가 무언가에 맞았다면, 총구에서 맞은 지점을 향하는 방향을 계산
            direction = (hit.point - firePoint.position).normalized;
        }
        else
        {
            // 레이가 아무것도 맞추지 못했다면, 카메라가 바라보는 방향으로 설정
            direction = ray.direction;
        }

        return direction;
    }
}