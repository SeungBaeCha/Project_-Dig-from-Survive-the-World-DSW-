
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 삽(Shovel) 장비의 기능을 관리하는 스크립트.
/// WeaponHold에 의해 활성화/비활성화 상태가 제어된다.
/// 레이캐스트를 이용해 'Diggable' 태그가 붙은 땅을 감지하고,
/// 입력에 따라 해당 땅을 파괴하는 역할을 한다.
/// </summary>
public class ShovelHold : MonoBehaviour
{
    [Header("삽 설정")]
    [Tooltip("땅을 팔 수 있는 최대 사정거리")]
    [SerializeField] private float digDistance = 3f;
    [Tooltip("파기 가능한 땅을 식별하는 데 사용될 태그")]
    [SerializeField] private string diggableTag = "Diggable";

    [Header("UI 설정")]
    [Tooltip("크로스헤어 GameObject")]
    public GameObject crosshairGameObject;
    [Tooltip("파기 가능한 대상을 조준했을 때의 크로스헤어 색상")]
    public Color diggableCrosshairColor = Color.green;

    [Header("이펙트 설정")]
    [Tooltip("땅을 팔 때 생성될 파티클 이펙트")]
    public ParticleSystem digEffectPrefab;

    // 메인 카메라 참조
    private Camera mainCamera;
    // 현재 조준하고 있는 오브젝트가 파기 가능한지 여부
    private bool isTargetDiggable = false;
    // 현재 조준하고 있는 파기 가능한 오브젝트
    private GameObject diggableTarget;
    
    // 크로스헤어의 그래픽 컴포넌트와 원래 색상을 저장하기 위한 리스트
    private List<Graphic> crosshairGraphics;
    private List<Color> originalCrosshairColors;
    
    void Start()
    {
        // 성능을 위해 메인 카메라 참조를 미리 찾아 저장
        mainCamera = Camera.main;

        if (crosshairGameObject == null)
        {
            Debug.LogError("Crosshair GameObject가 할당되지 않았습니다!");
        }
        else
        {
            // 크로스헤어 및 그 자식들로부터 모든 Graphic 컴포넌트를 찾아 리스트에 저장
            crosshairGraphics = new List<Graphic>(crosshairGameObject.GetComponentsInChildren<Graphic>());
            // 원래 색상들을 저장
            originalCrosshairColors = new List<Color>();
            foreach (var graphic in crosshairGraphics)
            {
                originalCrosshairColors.Add(graphic.color);
            }
        }
    }

    void Update()
    {
        // 카메라나 크로스헤어가 없으면 로직을 실행하지 않음
        if (mainCamera == null || crosshairGameObject == null) return;
        
        // 화면 정중앙에서 카메라 앞 방향으로 레이 생성
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // 레이캐스트를 실행하여 충돌하는 오브젝트가 있는지 확인
        if (Physics.Raycast(ray, out hit, digDistance))
        {
            // 충돌한 오브젝트에 'Diggable' 태그가 있는지 확인
            if (hit.collider.CompareTag(diggableTag))
            {
                // 파기 가능한 대상이므로 크로스헤어 색상을 변경하고 상태를 저장
                SetCrosshairColor(diggableCrosshairColor);
                isTargetDiggable = true;
                diggableTarget = hit.collider.gameObject;
            }
            else
            {
                // 파기 불가능한 대상이므로 원래 색상으로 복원
                ResetCrosshairColor();
            }
        }
        else
        {
            // 레이에 아무것도 맞지 않았을 경우에도 원래 색상으로 복원
            ResetCrosshairColor();
        }
    }
    
    /// <summary>
    /// 플레이어 입력에 따라 호출되는 '사용' 함수. (Unity Input System의 Player Input 컴포넌트에서 연결 필요)
    /// </summary>
    public void OnUse(InputAction.CallbackContext context)
    {
        // 키를 눌렀다 떼는 순간에만 작동하도록 설정
        if (!context.performed) return;

        // 조준한 대상이 파기 가능한 상태일 때만 파기 로직 실행
        if (isTargetDiggable && diggableTarget != null)
        {
            Dig();
        }
    }
    
    /// <summary>
    /// 땅 파기 로직을 수행하는 함수
    /// </summary>
    private void Dig()
    {
        Debug.Log(diggableTarget.name + "을(를) 팠습니다!");

        // 파티클 이펙트가 할당되었다면, 땅을 판 위치에 생성
        if (digEffectPrefab != null)
        {
            // 이펙트를 생성하고, 잠시 후 자동으로 파괴되도록 처리
            ParticleSystem effectInstance = Instantiate(digEffectPrefab, diggableTarget.transform.position, Quaternion.identity);
            Destroy(effectInstance.gameObject, effectInstance.main.duration);
        }

        // 대상 오브젝트 파괴
        Destroy(diggableTarget);

        // 파괴 후 상태 초기화
        isTargetDiggable = false;
        diggableTarget = null;
        ResetCrosshairColor();
    }

    /// <summary>
    /// 크로스헤어의 모든 그래픽 요소 색상을 지정된 색으로 변경
    /// </summary>
    private void SetCrosshairColor(Color color)
    {
        if (crosshairGraphics == null) return;
        foreach (var graphic in crosshairGraphics)
        {
            graphic.color = color;
        }
    }

    /// <summary>
    /// 크로스헤어 색상을 원래 색상으로 되돌리는 함수
    /// </summary>
    private void ResetCrosshairColor()
    {
        if (crosshairGraphics == null || originalCrosshairColors == null) return;

        for (int i = 0; i < crosshairGraphics.Count; i++)
        {
            // 저장해둔 원래 색상으로 복원
            if (crosshairGraphics[i].color != originalCrosshairColors[i])
            {
                crosshairGraphics[i].color = originalCrosshairColors[i];
            }
        }
    }

    // 삽을 장착/해제할 때 크로스헤어를 켜고 끄는 기능
    // 이 함수는 WeaponHold와 같은 장비 관리 스크립트에서 호출해주어야 함
    public void SetActive(bool isActive)
    {
        // 크로스헤어 게임 오브젝트 자체를 활성/비활성화
        if (crosshairGameObject != null)
        {
            crosshairGameObject.SetActive(isActive);
        }
        
        // 이 스크립트 자체도 활성/비활성화하여 Update 로직 제어
        this.enabled = isActive;

        // 비활성화될 때는 크로스헤어 상태를 리셋
        if (!isActive)
        {
            ResetCrosshairColor();
        }
    }
}
