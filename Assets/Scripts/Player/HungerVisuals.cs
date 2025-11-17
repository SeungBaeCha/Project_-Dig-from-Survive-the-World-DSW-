using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// 플레이어의 배고픔 수치에 따라 화면 시각 효과를 조절하는 스크립트
public class HungerVisuals : MonoBehaviour
{
    // 포스트 프로세싱 볼륨 참조
    public Volume postProcessVolume;

    // 플레이어 체력 및 배고픔 정보 참조
    private PlayerHealth playerHealth;
    // 색상 보정 효과
    private ColorAdjustments colorAdjustments;

    // 효과의 기본값 저장을 위한 변수
    private float defaultSaturation;
    private Color defaultColorFilter;

    void Start()
    {
        // PlayerHealth 컴포넌트 가져오기
        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.LogError("PlayerHealth 컴포넌트를 찾을 수 없습니다!");
            this.enabled = false; // 스크립트 비활성화
            return;
        }

        // 볼륨 프로파일에서 ColorAdjustments 효과를 찾아보고, 없으면 새로 추가
        if (postProcessVolume != null && postProcessVolume.profile.TryGet(out colorAdjustments))
        {
            // 효과의 기본값 저장
            defaultSaturation = colorAdjustments.saturation.value;
            defaultColorFilter = colorAdjustments.colorFilter.value;
        }
        else
        {
            // 프로파일에 ColorAdjustments가 없으면 새로 추가
            colorAdjustments = postProcessVolume.profile.Add<ColorAdjustments>(true);
            colorAdjustments.saturation.overrideState = true;
            colorAdjustments.colorFilter.overrideState = true;

            // 기본값 설정
            defaultSaturation = 0f;
            defaultColorFilter = Color.white;
            colorAdjustments.saturation.value = defaultSaturation;
            colorAdjustments.colorFilter.value = defaultColorFilter;

            Debug.LogWarning("ColorAdjustments 효과가 없어 새로 추가했습니다. 기본값을 설정합니다.");
        }
    }

    void Update()
    {
        if (playerHealth == null || colorAdjustments == null) return;

        // 현재 배고픔 비율 계산 (0.0 ~ 1.0)
        float hungerPercent = playerHealth.currentHunger / playerHealth.maxHunger;

        if (hungerPercent <= 0)
        {
            // 배고픔이 0일 때: 화면을 붉게 만듦
            colorAdjustments.saturation.value = 0; // 채도를 약간 주어 붉은색이 보이게 함
            colorAdjustments.colorFilter.value = new Color(1f, 0.2f, 0.2f, 1f); // 붉은색 필터
        }
        else if (hungerPercent < 0.4f)
        {
            // 40% 미만일 때: 점차 흑백으로 변경
            // 40%일 때 1, 0%일 때 0이 되는 값을 계산
            float lerpFactor = hungerPercent / 0.4f; 
            // 채도를 0(기본)에서 -100(흑백)으로 보간
            colorAdjustments.saturation.value = Mathf.Lerp(-100f, defaultSaturation, lerpFactor);
            // 색상 필터는 기본값으로 유지
            colorAdjustments.colorFilter.value = defaultColorFilter;
        }
        else
        {
            // 40% 이상일 때: 효과를 원래대로 복구
            colorAdjustments.saturation.value = defaultSaturation;
            colorAdjustments.colorFilter.value = defaultColorFilter;
        }
    }

    // 게임 종료 또는 오브젝트 파괴 시 효과를 원래대로 되돌림
    void OnDestroy()
    {
        if (colorAdjustments != null)
        {
            colorAdjustments.saturation.value = defaultSaturation;
            colorAdjustments.colorFilter.value = defaultColorFilter;
        }
    }
}
