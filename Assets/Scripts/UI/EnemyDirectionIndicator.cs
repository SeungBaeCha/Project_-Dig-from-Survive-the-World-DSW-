using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EnemyDirectionIndicator : MonoBehaviour
{
    [Header("UI 설정 (8-방향)")]
    public Image indicatorUp;
    public Image indicatorDown;
    public Image indicatorLeft;
    public Image indicatorRight;
    public Image indicatorUpLeft;
    public Image indicatorUpRight;
    public Image indicatorDownLeft;
    public Image indicatorDownRight;

    [Header("탐지 설정")]
    public float detectionDistance = 20f;
    
    [Header("효과 설정")]
    public float maxAlpha = 0.8f;
    public float fadeSpeed = 3f;

    private Camera mainCamera;
    private List<Image> allIndicators;
    private Dictionary<Image, float> targetAlphas;

    void Start()
    {
        mainCamera = Camera.main;

        // 8방향 인디케이터를 모두 리스트에 추가
        allIndicators = new List<Image> { 
            indicatorUp, indicatorDown, indicatorLeft, indicatorRight,
            indicatorUpLeft, indicatorUpRight, indicatorDownLeft, indicatorDownRight
        };
        targetAlphas = new Dictionary<Image, float>();

        foreach (var indicator in allIndicators)
        {
            if (indicator != null)
            {
                // 시작 시 모든 인디케이터를 투명하게 초기화
                Color color = indicator.color;
                color.a = 0;
                indicator.color = color;
                targetAlphas[indicator] = 0f;
            }
        }
    }

    void Update()
    {
        if (EnemyManager.Instance == null) return;
        
        // 매 프레임 목표 알파값을 0으로 초기화
        foreach (var key in new List<Image>(targetAlphas.Keys))
        {
            if(key != null) targetAlphas[key] = 0f;
        }

        List<Enemy> enemies = EnemyManager.Instance.GetActiveEnemies();
        if (enemies == null) return;

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null) continue;

            // 적과의 거리 및 방향 계산
            Vector3 toEnemy = enemy.transform.position - mainCamera.transform.position;
            float distance = toEnemy.magnitude;

            // 탐지 거리 밖에 있으면 무시
            if (distance > detectionDistance) continue;
            
            // 화면 안/밖 여부 판단
            Vector3 viewportPoint = mainCamera.WorldToViewportPoint(enemy.transform.position);
            bool onScreen = viewportPoint.z > 0 && viewportPoint.x > 0 && viewportPoint.x < 1 && viewportPoint.y > 0 && viewportPoint.y < 1;
            
            // 화면 안에 있으면 무시
            if (onScreen) continue;

            // 근접도에 따른 알파값 계산 (가까울수록 진해짐)
            float proximityAlpha = Mathf.InverseLerp(detectionDistance, 0, distance);

            Vector3 direction;
            if (viewportPoint.z < 0) // Case 1: 적이 카메라 뒤에 있을 때
            {
                // 카메라 로컬 좌표 기준으로 실제 방향 벡터 계산
                direction = mainCamera.transform.InverseTransformDirection(toEnemy);
            }
            else // Case 2: 적이 카메라 앞이지만 화면 밖에 있을 때
            {
                // 뷰포트 좌표를 기반으로 가상 방향 벡터 생성
                direction = viewportPoint - new Vector3(0.5f, 0.5f, 0.5f);
            }
            
            // 계산된 방향에 따라 8방향 인디케이터의 알파값 업데이트
            UpdateDirectionalAlphas(direction, proximityAlpha);
        }

        // 계산된 목표 알파값으로 각 인디케이터의 색상을 부드럽게 변경
        foreach (var indicator in allIndicators)
        {
            if (indicator != null)
            {
                Color color = indicator.color;
                float currentAlpha = color.a;
                float targetAlpha = targetAlphas[indicator] * maxAlpha;

                color.a = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);
                indicator.color = color;
            }
        }
    }

    /// <summary>
    /// 방향 벡터를 기반으로 8방향 인디케이터의 목표 알파값을 갱신합니다.
    /// </summary>
    void UpdateDirectionalAlphas(Vector3 direction, float proximity)
    {
        // z축은 방향 계산에 필요 없으므로 0으로 만들고 정규화
        direction.z = 0;
        direction.Normalize();

        // 4방향(상하좌우) 가중치 계산
        float right = Mathf.Clamp01(direction.x);
        float left = Mathf.Clamp01(-direction.x);
        float up = Mathf.Clamp01(direction.y);
        float down = Mathf.Clamp01(-direction.y);

        // 대각선 방향 가중치 계산
        float upRight = up * right;
        float upLeft = up * left;
        float downRight = down * right;
        float downLeft = down * left;
        
        // 주 방향(상하좌우)이 대각선과 겹치지 않도록 가중치 보정
        float cardinalUp = up * (1 - left) * (1 - right);
        float cardinalDown = down * (1 - left) * (1 - right);
        float cardinalLeft = left * (1 - up) * (1 - down);
        float cardinalRight = right * (1 - up) * (1 - down);

        // 계산된 가중치에 따라 최종 알파값 갱신 (여러 적 신호 중 가장 강한 신호 사용)
        if (indicatorUp) targetAlphas[indicatorUp] = Mathf.Max(targetAlphas[indicatorUp], cardinalUp * proximity);
        if (indicatorDown) targetAlphas[indicatorDown] = Mathf.Max(targetAlphas[indicatorDown], cardinalDown * proximity);
        if (indicatorLeft) targetAlphas[indicatorLeft] = Mathf.Max(targetAlphas[indicatorLeft], cardinalLeft * proximity);
        if (indicatorRight) targetAlphas[indicatorRight] = Mathf.Max(targetAlphas[indicatorRight], cardinalRight * proximity);
        
        if (indicatorUpRight) targetAlphas[indicatorUpRight] = Mathf.Max(targetAlphas[indicatorUpRight], upRight * proximity);
        if (indicatorUpLeft) targetAlphas[indicatorUpLeft] = Mathf.Max(targetAlphas[indicatorUpLeft], upLeft * proximity);
        if (indicatorDownRight) targetAlphas[indicatorDownRight] = Mathf.Max(targetAlphas[indicatorDownRight], downRight * proximity);
        if (indicatorDownLeft) targetAlphas[indicatorDownLeft] = Mathf.Max(targetAlphas[indicatorDownLeft], downLeft * proximity);
    }
}
