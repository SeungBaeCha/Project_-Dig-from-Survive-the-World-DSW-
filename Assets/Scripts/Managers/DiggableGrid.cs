using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation; // NavMeshSurface를 사용하기 위해 추가

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
    [Tooltip("NavMesh 업데이트 주기. 이 시간(초)마다 한 번씩만 업데이트를 실행하여 부하를 줄인다.")]
    public float navMeshRebuildInterval = 2.0f;

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

    // NavMesh 업데이트가 필요한지 여부를 나타내는 플래그
    private bool navMeshNeedsRebuild = false;

    // 생성된(또는 파괴되어 생긴) 입구(구멍) 위치들 관리
    private List<Vector3> entrances = new List<Vector3>();

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

        // 초기 격자 생성 후 NavMesh 전체를 한 번 빌드
        surface.BuildNavMesh();

        // NavMesh를 주기적으로 체크하고 필요시 재빌드하는 코루틴을 시작
        StartCoroutine(NavMeshRebuildRoutine());
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
                    // 각 청크의 위치를 계산. 이제 gridOrigin을 기준으로 함
                    Vector3 position = new Vector3(x * chunkSize, y * chunkSize, z * chunkSize) + gridOrigin;

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
    /// Chunk가 파괴될 때 NavMesh 업데이트가 필요하다고 플래그를 설정한다.
    /// </summary>
    public void RequestNavMeshUpdate()
    {
        navMeshNeedsRebuild = true;
    }

    /// <summary>
    /// 일정 주기로 NavMesh 업데이트가 필요한지 확인하고, 필요하다면 재빌드한다.
    /// </summary>
    private IEnumerator NavMeshRebuildRoutine()
    {
        // 게임이 실행되는 동안 계속 반복
        while (true)
        {
            // navMeshNeedsRebuild 플래그가 true일 때만 업데이트를 진행
            if (navMeshNeedsRebuild)
            {
                // 플래그를 리셋하고 NavMesh를 다시 빌드
                navMeshNeedsRebuild = false;
                surface.BuildNavMesh();
            }

            // 설정된 시간(navMeshRebuildInterval)만큼 대기
            yield return new WaitForSeconds(navMeshRebuildInterval);
        }
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
