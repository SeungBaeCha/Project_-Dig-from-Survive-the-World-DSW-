using UnityEngine;

/// <summary>
/// 작은 '청크' 프리팹을 이용하여 팔 수 있는 지형을 격자 형태로 생성
/// 이 스크립트를 빈 게임 오브젝트에 추가하고, Inspector에서 격자 크기와 청크 프리팹 설정
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

    void Start()
    {
        // chunkPrefab이 할당되었는지 확인
        if (chunkPrefab == null)
        {
            Debug.LogError("Chunk Prefab이 할당되지 않았다. DiggableGrid 스크립트의 Inspector에서 프리팹을 설정해줘.");
            return;
        }

        // 격자 생성을 시작
        GenerateGrid();
    }

    /// <summary>
    /// 설정된 크기에 맞춰 청크들로 격자를 생성
    /// </summary>
    void GenerateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    // 각 청크의 위치를 계산
                    // 이 오브젝트의 위치를 기준으로 격자가 생성
                    Vector3 position = new Vector3(x * chunkSize, y * chunkSize, z * chunkSize) + transform.position;
                    
                    // 청크 프리팹을 씬에 생성(인스턴스화)
                    GameObject chunk = Instantiate(chunkPrefab, position, Quaternion.identity);
                    
                    // 생성된 청크들을 이 오브젝트의 자식으로 만들어 관리
                    chunk.transform.SetParent(transform);
                }
            }
        }
    }
}