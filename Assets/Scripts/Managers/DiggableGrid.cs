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

        // 초기 격자 생성 후 NavMesh 전체를 한 번 비동기 빌드
        _rebuildCoroutine = StartCoroutine(DelayedRebuildAsync(0f)); // 0초 지연으로 즉시 비동기 빌드 시작
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
    /// Chunk가 파괴될 때 NavMesh 업데이트를 요청한다. (Debounced)
    /// 여러 번 호출되더라도 마지막 호출 후 짧은 시간 뒤에 한 번만 실행된다.
    /// </summary>
    public void RequestNavMeshUpdate()
    {
        // 이미 실행중인 업데이트 코루틴이 있다면 중지시킨다.
        if (_rebuildCoroutine != null)
        {
            StopCoroutine(_rebuildCoroutine);
        }
        // 새로운 업데이트 코루틴을 시작하고 참조를 저장한다.
        _rebuildCoroutine = StartCoroutine(DelayedRebuildAsync(1.0f)); // 1초 지연 후 비동기 업데이트 시작
    }

    /// <summary>
    /// 짧은 지연 시간 후에 NavMesh를 재빌드하는 코루틴.
    /// </summary>
    private IEnumerator DelayedRebuildAsync(float delay)
    {
        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }

        IsNavMeshUpdating = true;
        Debug.Log("NavMesh update started (synchronously for now).");

        // Unity의 내장된 NavMeshSurface.BuildNavMesh()를 사용한다.
        // 이 메서드는 동기적으로 동작하며, 메인 스레드를 잠시 블록할 수 있지만
        // 현재 Unity 환경에서 런타임에 가장 안정적으로 NavMesh를 업데이트하는 방법이다.
        surface.BuildNavMesh();
        
        IsNavMeshUpdating = false;
        Debug.Log("NavMesh update completed (synchronously for now).");

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
}
