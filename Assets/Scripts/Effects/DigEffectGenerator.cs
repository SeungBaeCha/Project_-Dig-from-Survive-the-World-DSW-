using UnityEngine;

/// <summary>
/// 이 스크립트를 ParticleSystem이 있는 게임 오브젝트에 추가해 봐.
/// 땅 파기 효과에 어울리는 파티클 속성을 자동으로 설정해 줄 거야.
/// 다 만든 건 프리팹으로 저장해서 Shovel 스크립트에 연결하면 돼.
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class DigEffectGenerator : MonoBehaviour
{
    // 인스펙터에서 파티클 색을 편하게 바꿀 수 있도록 public으로 만들었어.
    [Tooltip("파티클의 기본 색상을 설정해 봐.")]
    public Color digColor = new Color(0.55f, 0.4f, 0.25f); // 흙 느낌 나는 갈색

    void Awake()
    {
        // 이 게임 오브젝트의 파티클 시스템 컴포넌트를 가져올게.
        ParticleSystem ps = GetComponent<ParticleSystem>();
        
        // 혹시 모르니 파티클을 멈추고 초기화.
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // --- 1. 기본 설정 (Main Module) ---
        var main = ps.main;
        main.duration = 0.6f;                                   // 이펙트 전체 지속 시간
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);  // 파티클 생존 시간 (무작위)
        main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);       // 파티클 시작 속도 (무작위)
        main.startSize = new ParticleSystem.MinMaxCurve(0.005f, 0.015f);   // 파티클 시작 크기 (무작위). '티끌'처럼 보이게 매우 작게 수정
        main.startColor = digColor;                             // 파티클 시작 색상
        main.maxParticles = 50;                                 // 최대 파티클 개수
        main.playOnAwake = true;                                // 생성되면 바로 자동 재생
        main.stopAction = ParticleSystemStopAction.Destroy;     // 재생 끝나면 알아서 파괴

        // --- 2. 방출 설정 (Emission Module) ---
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0;                              // 시간에 따른 방출은 안 써.
        // 한 번에 터져나오는 파티클 설정
        emission.SetBursts(new ParticleSystem.Burst[] 
        {
            new ParticleSystem.Burst(0.0f, 25)                  // 시작하자마자 25개 방출
        });

        // --- 3. 형태 설정 (Shape Module) ---
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;   // 반구 형태 (땅에서 솟아나는 느낌)
        shape.radius = 0.3f;                                    // 방출 반경
        shape.randomDirectionAmount = 1f;                           // 방향을 무작위로 (1f는 완전 무작위)

        // --- 4. 시간에 따른 색상 변화 (Color over Lifetime Module) ---
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        // 끝으로 갈수록 점점 투명해지게 설정할 거야.
        GradientColorKey[] colorKeys = { new GradientColorKey(digColor, 0.0f) };
        GradientAlphaKey[] alphaKeys = { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.5f, 0.5f), new GradientAlphaKey(0.0f, 1.0f) };
        grad.SetKeys(colorKeys, alphaKeys);
        colorOverLifetime.color = grad;

        // --- 5. 시간에 따른 크기 변화 (Size over Lifetime Module) ---
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        // 끝으로 갈수록 점점 작아지게.
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1.0f, 0.0f);
        
        // --- 6. 중력 설정 (Gravity Modifier) ---
        main.gravityModifier = 0.5f; // 중력을 살짝 줘서 자연스럽게 떨어지게.

        // --- 7. 물리 충돌 설정 (Collision Module) ---
        var collision = ps.collision;
        collision.enabled = true;
        collision.type = ParticleSystemCollisionType.World;   // 월드의 다른 콜라이더랑 충돌하게.
        // collision.mode = ParticleSystemCollisionMode.3D;      // 3D 충돌 모드 사용 (오류 발생으로 임시 주석 처리)
        collision.dampen = 0.3f;                                // 충돌하면 속도가 줄어드는 정도
        collision.bounce = 0.1f;                                // 살짝 튕기는 정도
        collision.lifetimeLoss = 0.2f;                          // 충돌하면 생존 시간 감소

        // --- 8. 렌더러 설정 (Renderer Module) ---
        var renderer = ps.GetComponent<ParticleSystemRenderer>();

        // 핑크색으로 보이는 현상은 URP(Universal Render Pipeline)에 맞는 머티리얼이 설정되지 않아서야.
        // URP용 기본 파티클 머티리얼을 찾아서 할당하거나, 없으면 새로 만들어서 해결할 수 있어.
        Shader particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");

        if (particleShader != null)
        {
            // 셰이더를 찾았으면 새 머티리얼을 만들고 할당할게.
            Material newMaterial = new Material(particleShader);
            // 이 머티리얼은 파티클의 색상(startColor)을 그대로 사용하게 될 거야.
            renderer.material = newMaterial;
        }
        else
        {
            // 만약 셰이더를 못 찾으면 경고 메시지를 표시할게.
            // 이 경우엔 URP 패키지가 프로젝트에 제대로 설치되었는지 확인해봐야 해.
            Debug.LogWarning("URP 파티클 셰이더를 찾을 수 없어. 'Universal RP' 패키지가 설치되어 있는지 확인해 봐!");
        }

        // 파티클 모양은 카메라를 항상 바라보는 '빌보드'로 설정할게.
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
    }
}
