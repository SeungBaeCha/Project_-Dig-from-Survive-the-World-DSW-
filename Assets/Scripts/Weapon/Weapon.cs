using System.Collections;
using UnityEngine;

/// <summary>
/// 무기 개체에 부착되어 줍기/버리기 상태를 관리하는 스크립트
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Weapon : MonoBehaviour
{
    [Header("상호작용 UI")]
    // 무기 위에 표시할 월드 스페이스 UI 오브젝트 
    [SerializeField] private GameObject interactionUI;

    // 무기의 물리적 상태를 제어하기 위한 컴포넌트
    private Rigidbody rb;
    private Collider col;

    // 이 무기를 주울 수 있는지 여부를 나타내는 플래그
    public bool canBePickedUp = true;

    void Awake()
    {
        // 컴포넌트들을 미리 찾아놓는다.
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        // 무기가 이상하게 서 있는 현상을 방지하고 자연스럽게 떨어지게 하기 위해 무게 중심을 조절한다 (y축)
        rb.centerOfMass = new Vector3(0, -0.1f, 0);

        // 시작할 때 상호작용 UI는 꺼둔다.
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
    }

    /// <summary>
    /// 이 무기(의 트리거 콜라이더)에 다른 오브젝트가 들어왔을 때 호출된다.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // 주울 수 있는 상태가 아니면 아무것도 하지 않는다.
        if (!canBePickedUp) return;

        // 들어온 오브젝트가 'Player' 태그를 가지고 있는지 확인
        if (other.CompareTag("Player"))
        {
            // 플레이어에게서 WeaponHold 스크립트를 찾는다.
            WeaponHold weaponHold = other.GetComponent<WeaponHold>();

            // 스크립트를 찾았다면, 이 무기를 '주울 수 있는 무기'로 설정하라고 알려줌.
            if (weaponHold != null)
            {
                weaponHold.SetNearbyWeapon(this.gameObject);

                // 상호작용 UI가 있다면 활성화한다.
                if (interactionUI != null)
                {
                    interactionUI.SetActive(true);
                }
            }
        }
    }

    /// <summary>
    /// 다른 오브젝트가 이 무기의 트리거 콜라이더에서 빠져나갔을 때 호출된다.
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        // 나간 오브젝트가 'Player' 태그를 가지고 있는지 확인
        if (other.CompareTag("Player"))
        {
            // 플레이어에게서 WeaponHold 스크립트를 찾는다.
            WeaponHold weaponHold = other.GetComponent<WeaponHold>();

            // 스크립트를 찾았다면, 이 무기가 더 이상 근처에 없다고 알려줌.
            if (weaponHold != null)
            {
                weaponHold.ClearNearbyWeapon(this.gameObject);

                // 상호작용 UI가 있다면 비활성화한다.
                if (interactionUI != null)
                {
                    interactionUI.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// 무기를 주웠을 때 호출되어 상호작용 UI를 비활성화하는 메서드
    /// </summary>
    public void HandlePickup()
    {
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
    }

    /// <summary>
    /// 무기를 버렸을 때 호출되어 상호작용 UI를 다시 활성화하는 메서드.
    /// </summary>
    public void HandleDrop()
    {
        if (interactionUI != null)
        {
            interactionUI.SetActive(true);
        }
    }

    /// <summary>
    /// 지정된 시간 동안 무기를 주울 수 없도록 만드는 코루틴을 시작
    /// </summary>>
    /// <param name="cooldownTime">줍기 비활성화 시간(초)</param>
    public void StartPickupCooldown(float cooldownTime)
    {
        StartCoroutine(PickupCooldownCoroutine(cooldownTime));
    }

    /// <summary>
    /// 무기를 일시적으로 주울 수 없게 만드는 코루틴
    /// </summary>
    private IEnumerator PickupCooldownCoroutine(float cooldownTime)
    {
        // 1. 줍기 비활성화
        canBePickedUp = false;

        // 2. 지정된 시간만큼 대기
        yield return new WaitForSeconds(cooldownTime);

        // 3. 줍기 활성화
        canBePickedUp = true;
    }
}