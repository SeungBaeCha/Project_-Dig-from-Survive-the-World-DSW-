using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation; // NavMeshSurface를 사용하기 위해 추가
using UnityEngine.AI; // NavMesh, NavMeshBuildSettings, NavMeshBuilder 등을 사용하기 위해 추가

/// <summary>
/// 작은 '청크' 프리팹을 이용하여 팔 수 있는 지형을 격자 형태로 생성하고,
/// 지형 변화 시 NavMesh를 효율적으로 업데이트하는 역할을 담당한다.
/// </summary>
public class DiggableGrid : MonoBehaviour
{
    [Header("Grid Dimensions")]
    [Tooltip("격자의 너비 (X-axis) 가로축")]
    public int width = 10;
    [Tooltip("격자의 높이 (Y-axis) 세로축")]
    public int height = 5;
    [Tooltip("격자의 깊이 (Z-axis) 앞뒤축")]
    public int depth = 10;

    [Header("Chunk Settings")]
    [Tooltip("지형을 구성할 기본 블록(청크) 프리팹")]
    public GameObject chunkPrefab;

    [Tooltip("각 청크의 크기. 프리팹의 실제 크기와 일치해야 함")]
    public float chunkSize = 1.0f;

    [Header("NavMesh Settings")]
    [Tooltip("씬의 유일한 NavMeshSurface 컴포넌트(보통 Ground 오브젝트에 있음)를 여기에 할당해야 한다.")]
    public NavMeshSurface surface;
    
    // NavMesh 업데이트가 중복 실행되지 않도록 관리하는 코루틴 참조
    private Coroutine _rebuildCoroutine;

    // NavMesh 업데이트 상태를 나타내는 플래그 추가
    public bool IsNavMeshUpdating { get; private set; } = false; // 외부에서 읽기 전용

    public static DiggableGrid Instance { get; private set; } // 싱글톤 인스턴스

    [Header("Multi-Grid Settings")]
    [Tooltip("생성될 그리드들 사이의 간격. 그리드의 크기를 고려하여 설정해야 겹치지 않습니다.")]
    public float gridSpacing = 5f;
    [Tooltip("중앙 (0,0,0) 위치에 그리드를 생성합니다.")]
    public bool generateInCenter = true;
    [Tooltip("위쪽 (+Z) 방향에 그리드를 생성합니다.")]
    public bool generateUp = true;
    [Tooltip("아래쪽 (-Z) 방향에 그리드를 생성합니다.")]
    public bool generateDown = true;
    [Tooltip("왼쪽 (-X) 방향에 그리드를 생성합니다.")]
    public bool generateLeft = true;
    [Tooltip("오른쪽 (+X) 방향에 그리드를 생성합니다.")]
    public bool generateRight = true;
    
        // 생성된(또는 파괴되어 생긴) 입구(구멍) 위치들 관리
    
        private List<Vector3> entrances = new List<Vector3>();
    
    
    
        // 이펙트 풀링을 위한 딕셔너리
    
        private Dictionary<GameObject, Queue<GameObject>> effectPools = new Dictionary<GameObject, Queue<GameObject>>();
    
        [Tooltip("각 이펙트 프리팹별 초기 풀 크기")]
    
        public int initialEffectPoolSize = 10;
    
    
    
        void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // 씬이 변경되어도 파괴되지 않도록 설정 (루트 오브젝트를 대상으로 함)
            DontDestroyOnLoad(transform.root.gameObject);
        }

        // GameManager의 OnNightStart 이벤트 구독
        GameManager.OnNightStart += OnNightStartEventHandler;
    }

    void Start()
    {
        // surface가 Inspector에서 할당되지 않았다면 에러 메시지를 표시
        if (surface == null)
        {
            Debug.LogError("NavMeshSurface가 할당되지 않았습니다. Ground 오브젝트를 DiggableGrid의 Surface 슬롯에 할당해주세요.");
            return;
        }

        // chunkPrefab이 할당되었는지 확인
        if (chunkPrefab == null)
        {
            Debug.LogError("Chunk Prefab이 할당되지 않았다. DiggableGrid 스크립트의 Inspector에서 프리팹을 설정해줘.");
            return;
        }

        // 설정에 따라 여러 그리드를 생성
        CreateGrids();

        // 게임 시작 시 초기 NavMesh 빌드를 한 번 수행
        _rebuildCoroutine = StartCoroutine(DelayedRebuildAsync(0f));
    }

    /// <summary>
    /// 설정에 따라 여러 위치에 그리드를 생성한다.
    /// 이 메서드는 월드 원점(0,0,0)을 기준으로 동작한다.
    /// </summary>
    void CreateGrids()
    {
        // 그리드 하나의 전체 너비(x)와 깊이(z)를 계산
        float gridTotalWidth = width * chunkSize;
        float gridTotalDepth = depth * chunkSize;

        // 중앙 그리드 생성 (월드 원점 기준)
        if (generateInCenter)
        {
            GenerateGrid(Vector3.zero);
        }

        // 상하좌우 그리드는 '전체 그리드 크기 + 설정된 간격'을 기준으로 월드 원점으로부터의 위치를 계산
        if (generateUp)
        {
            GenerateGrid(new Vector3(0, 0, gridTotalDepth + gridSpacing));
        }
        if (generateDown)
        {
            GenerateGrid(new Vector3(0, 0, -(gridTotalDepth + gridSpacing)));
        }
        if (generateLeft)
        {
            GenerateGrid(new Vector3(-(gridTotalWidth + gridSpacing), 0, 0));
        }
        if (generateRight)
        {
            GenerateGrid(new Vector3(gridTotalWidth + gridSpacing, 0, 0));
        }
    }


    /// <summary>
    /// 지정된 원점(gridOrigin)을 기준으로, 설정된 크기에 맞춰 청크들로 격자를 생성
    /// </summary>
    /// <param name="gridOrigin">이 그리드의 시작 위치(좌측 하단 앞쪽)</param>
    void GenerateGrid(Vector3 gridOrigin)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    // 각 청크의 위치를 계산. 이제 gridOrigin과 이 오브젝트의 월드 위치(transform.position)를 모두 기준으로 함
                    Vector3 position = transform.position + new Vector3(x * chunkSize, y * chunkSize, z * chunkSize) + gridOrigin;

                    // 청크 프리팹을 씬에 생성(인스턴스화)
                    GameObject chunk = Instantiate(chunkPrefab, position, Quaternion.identity);

                    // 생성된 청크들을 이 오브젝트의 자식으로 만들어 관리
                    chunk.transform.SetParent(transform);

                    // 생성된 청크에 Chunk 레이어 할당
                    chunk.layer = LayerMask.NameToLayer("Chunk");
                }
            }
        }
    }



    /// <summary>
    /// 짧은 지연 시간 후에 NavMesh를 재빌드하는 코루틴.
    /// </summary>
    private IEnumerator DelayedRebuildAsync(float delay) // delay 인자는 더 이상 사용되지 않지만, 시그니처 유지를 위해 남겨둔다.
    {
        // StartContinuousNavMeshUpdate()에서 호출될 때는 delay가 0f이므로, 이 부분은 바로 통과한다.
        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }

        IsNavMeshUpdating = true;
        LogDebugMessage("NavMesh 업데이트 시작됨 (비동기, UpdateNavMesh 사용).");

        surface.UpdateNavMesh(surface.navMeshData);

        IsNavMeshUpdating = false;
        LogDebugMessage("NavMesh 업데이트 작업 예약됨 (비동기).");

        _rebuildCoroutine = null;
    }

    // --- Entrance 관리 API ---

    // 부모(또는 Chunk)가 입구를 등록할 때 호출
    public void RegisterEntrance(Vector3 worldPos)
    {
        // 핵심: 동일 위치 중복 등록 방지
        if (!entrances.Contains(worldPos))
        {
            entrances.Add(worldPos);
        }
    }

    // 입구 제거(필요 시 호출 가능)
    public void UnregisterEntrance(Vector3 worldPos)
    {
        entrances.Remove(worldPos);
    }

    // 다른 스크립트(예: Enemy)가 입구 목록을 참조할 수 있도록 제공
    public List<Vector3> GetEntrances()
    {
        return entrances;
    }

    /// <summary>
    /// GameManager의 OnNightStart 이벤트 발생 시 호출. 밤 시작 시 NavMesh를 업데이트한다.
    /// </summary>
    private void OnNightStartEventHandler()
    {
        LogDebugMessage("밤 시작! NavMesh 업데이트를 요청합니다.");
        // 지연 없이 즉시 NavMesh 업데이트 코루틴을 시작한다.
        // _rebuildCoroutine이 이미 진행 중일 경우 StopCoroutine으로 중지 후 다시 시작.
        if (_rebuildCoroutine != null)
        {
            StopCoroutine(_rebuildCoroutine);
        }
        _rebuildCoroutine = StartCoroutine(DelayedRebuildAsync(0f));
    }


    /// <summary>
    /// 오브젝트가 파괴될 때 호출되며, GameManager 이벤트 구독을 해제한다.
    /// </summary>
    void OnDestroy()
    {
        // GameManager의 OnNightStart 이벤트 구독 해제
        if (GameManager.Instance != null)
        {
            GameManager.OnNightStart -= OnNightStartEventHandler;
        }
    }

    // 이펙트 풀에서 사용될 커스텀 컴포넌트
    public class PooledEffect : MonoBehaviour
    {
        public GameObject originalPrefab;
    }

    /// <summary>
    /// 이펙트 풀에서 오브젝트를 가져오거나 새로 생성하여 반환.
    /// </summary>
    public GameObject GetPooledEffect(GameObject effectPrefab, Vector3 position, Quaternion rotation)
    {
        if (!effectPools.ContainsKey(effectPrefab))
        {
            effectPools[effectPrefab] = new Queue<GameObject>();
            // 초기 풀 사이즈만큼 미리 생성
            for (int i = 0; i < initialEffectPoolSize; i++)
            {
                GameObject obj = Instantiate(effectPrefab, transform); // DiggableGrid를 부모로 설정
                obj.SetActive(false);
                PooledEffect pooledEffect = obj.AddComponent<PooledEffect>();
                pooledEffect.originalPrefab = effectPrefab;
                effectPools[effectPrefab].Enqueue(obj);
            }
        }

        GameObject effect;
        if (effectPools[effectPrefab].Count > 0)
        {
            effect = effectPools[effectPrefab].Dequeue();
        }
        else
        {
            effect = Instantiate(effectPrefab, transform); // 풀이 비면 새로 생성
            PooledEffect pooledEffect = effect.AddComponent<PooledEffect>();
            pooledEffect.originalPrefab = effectPrefab;
        }

        effect.transform.position = position;
        effect.transform.rotation = rotation;
        effect.SetActive(true);

        // 파티클 시스템이 끝나면 자동으로 풀로 돌아오도록 설정 (ParticleSystem 컴포넌트 필요)
        ParticleSystem ps = effect.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            // 파티클 시스템의 duration과 startLifetime을 고려하여 총 재생 시간을 계산
            float totalDuration = ps.main.duration;
            if (ps.main.startLifetime.mode == ParticleSystemCurveMode.Constant)
            {
                totalDuration += ps.main.startLifetime.constant;
            }
            else if (ps.main.startLifetime.mode == ParticleSystemCurveMode.TwoConstants)
            {
                totalDuration += ps.main.startLifetime.constantMax;
            }
            
            StartCoroutine(ReturnEffectAfterCompletion(effect, totalDuration));
        } else {
            // 파티클 시스템이 없는 이펙트인 경우, 임시로 1초 뒤에 반환
            StartCoroutine(ReturnEffectAfterCompletion(effect, 1f)); 
        }

        return effect;
    }

    /// <summary>
    /// 사용 완료된 이펙트 오브젝트를 풀로 반환. (GetPooledEffect에서 코루틴으로 호출됨)
    /// </summary>
    public void ReturnPooledEffect(GameObject effect)
    {
        effect.SetActive(false);
        PooledEffect pooledEffect = effect.GetComponent<PooledEffect>();
        if (pooledEffect != null && effectPools.ContainsKey(pooledEffect.originalPrefab))
        {
            effectPools[pooledEffect.originalPrefab].Enqueue(effect);
        }
        else
        {
            // 풀을 찾지 못했거나 오류 발생 시 파괴
            Destroy(effect);
        }
    }

    private IEnumerator ReturnEffectAfterCompletion(GameObject effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        // effect GameObject가 유효한지 먼저 확인한다.
        if (effect != null) // 이펙트가 여전히 존재하면 풀로 반환
        {
            ReturnPooledEffect(effect);
        }
        else
        {
            LogDebugMessage($"파괴된 이펙트 GameObject에 접근하려 했습니다. (원인: 씬 전환 또는 다른 스크립트 파괴)");
        }
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void LogDebugMessage(string message)
    {
        Debug.Log(message);
    }


}
