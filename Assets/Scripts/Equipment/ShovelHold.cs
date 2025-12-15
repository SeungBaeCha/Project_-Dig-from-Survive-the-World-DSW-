using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 삽(Shovel)의 크로스헤어 UI를 관리하는 스크립트.
/// 이 스크립트는 Shovel.cs와 함께 Shovel 게임 오브젝트에 부착되어야 한다.
/// </summary>
public class ShovelHold : MonoBehaviour
{
    [Header("UI 설정")]
    [Tooltip("파기 가능한 대상을 조준했을 때의 크로스헤어 색상")]
    [SerializeField] private Color diggableCrosshairColor = Color.green; // 기본값을 초록색으로 변경

    // --- 내부 참조 변수 ---
    private Shovel shovel; // 같은 오브젝트에 있는 Shovel 스크립트
    private WeaponHold weaponHold; // Player에 있는 WeaponHold 스크립트
    private List<Graphic> crosshairGraphics;
    private List<Color> originalCrosshairColors;
    private bool isInitialized = false;

    void Awake()
    {
        // 같은 게임 오브젝트에 있는 Shovel 컴포넌트를 미리 찾아둔다.
        shovel = GetComponent<Shovel>();
    }

    void Update()
    {
        // 초기화가 되지 않았거나, 필요한 참조가 없으면 아무 작업도 하지 않는다.
        if (!isInitialized || shovel == null || weaponHold == null || weaponHold.crosshair == null)
        {
            return;
        }

        // Shovel 스크립트가 대상을 감지했는지 여부에 따라 크로스헤어 색상을 변경한다.
        if (shovel.IsTargetDiggable)
        {
            SetCrosshairColor(diggableCrosshairColor);
        }
        else
        {
            ResetCrosshairColor();
        }
    }

    /// <summary>
    /// WeaponHold에 의해 호출되어 필요한 정보를 설정하고 초기화를 진행한다.
    /// </summary>
    public void Initialize(WeaponHold wh)
    {
        weaponHold = wh;

        if (weaponHold.crosshair != null)
        {
            // 크로스헤어의 모든 그래픽 컴포넌트와 원래 색상을 저장한다.
            crosshairGraphics = new List<Graphic>(weaponHold.crosshair.GetComponentsInChildren<Graphic>());
            originalCrosshairColors = new List<Color>();
            foreach (var graphic in crosshairGraphics)
            {
                originalCrosshairColors.Add(graphic.color);
            }
        }
        
        isInitialized = true;
    }

    /// <summary>
    /// WeaponHold에 의해 호출되어 크로스헤어를 원래 상태로 되돌린다.
    /// </summary>
    public void Deinitialize()
    {
        if (isInitialized)
        {
            ResetCrosshairColor();
        }
        isInitialized = false;
        weaponHold = null;
        crosshairGraphics = null;
        originalCrosshairColors = null;
    }
    
    private void SetCrosshairColor(Color color)
    {
        if (crosshairGraphics == null) return;
        foreach (var graphic in crosshairGraphics)
        {
            graphic.color = color;
        }
    }

    private void ResetCrosshairColor()
    {
        if (crosshairGraphics == null || originalCrosshairColors == null) return;
        for (int i = 0; i < crosshairGraphics.Count; i++)
        {
            if (i < crosshairGraphics.Count && crosshairGraphics[i] != null)
            {
                crosshairGraphics[i].color = originalCrosshairColors[i];
            }
        }
    }
}
