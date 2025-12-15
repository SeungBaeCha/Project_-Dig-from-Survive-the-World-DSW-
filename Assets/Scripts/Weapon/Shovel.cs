using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 삽(Shovel)의 핵심 기능을 담당하는 스크립트.
/// 레이캐스트로 파기 가능한 대상을 감지하고, 실제 파기 동작을 수행한다.
/// 이 스크립트는 Shovel 게임 오브젝트에 부착되어야 한다.
/// </summary>
public class Shovel : MonoBehaviour
{
    [Header("삽 설정")]
    [Tooltip("땅을 팔 수 있는 최대 사정거리")]
    [SerializeField] private float digDistance = 3f;
    [Tooltip("파기 가능한 땅을 식별하는 데 사용될 태그")]
    [SerializeField] private string diggableTag = "Diggable";

    [Header("이펙트 설정")]
    [Tooltip("땅을 팔 때 생성될 파티클 이펙트")]
    public ParticleSystem digEffectPrefab;

    // 메인 카메라 참조
    private Camera mainCamera;
    // 현재 조준하고 있는 파기 가능한 오브젝트
    private GameObject diggableTarget;
    
    /// <summary>
    /// 현재 조준하고 있는 대상이 파기 가능한지 여부를 나타낸다.
    /// ShovelHold 스크립트가 이 값을 참조하여 크로스헤어 색상을 변경한다.
    /// </summary>
    public bool IsTargetDiggable { get; private set; }

    void Start()
    {
        // Player의 메인 카메라를 찾아 저장한다. 이 스크립트가 활성화될 때 한 번만 호출된다.
        mainCamera = Camera.main;
    }

    void Update()
    {
        // 카메라가 없으면 아무 동작도 하지 않는다.
        if (mainCamera == null) return;
        
        // 화면 정중앙에서 카메라 앞 방향으로 레이를 생성한다.
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // 레이캐스트를 실행하여 'Diggable' 태그를 가진 오브젝트가 감지되었는지 확인한다.
        if (Physics.Raycast(ray, out hit, digDistance) && hit.collider.CompareTag(diggableTag))
        {
            // 감지되었다면, 파기 가능한 상태로 설정한다.
            IsTargetDiggable = true;
            diggableTarget = hit.collider.gameObject;
        }
        else
        {
            // 감지되지 않았다면, 파기 불가능한 상태로 설정한다.
            IsTargetDiggable = false;
            diggableTarget = null;
        }
    }
    
    /// <summary>
    /// Player Input에 의해 호출되는 '발사' 액션.
    /// </summary>
    public void OnFire(InputAction.CallbackContext context)
    {
        // 키를 눌렀다 떼는 순간에만 작동하고, 파기 가능한 대상이 있을 때만 Dig()를 호출한다.
        if (context.performed && IsTargetDiggable && diggableTarget != null)
        {
            Dig();
        }
    }
    
    /// <summary>
    /// 땅 파기 로직을 수행하는 함수
    /// </summary>
    private void Dig()
    {
        // 파티클 이펙트가 할당되었다면, 땅을 판 위치에 생성한다.
        if (digEffectPrefab != null)
        {
            ParticleSystem effectInstance = Instantiate(digEffectPrefab, diggableTarget.transform.position, Quaternion.identity);
            Destroy(effectInstance.gameObject, effectInstance.main.duration);
        }

        // 대상 오브젝트를 파괴한다.
        Destroy(diggableTarget);

        // 파괴 후 상태를 초기화한다.
        diggableTarget = null;
        IsTargetDiggable = false;
    }
}
