using UnityEngine;

/// <summary>
/// 플레이어가 이 트리거에 들어왔을 때 게임의 주요 시스템을 시작시키는 역할을 한다.
/// 게임 시작 지점에 배치된 비어있는 게임 오브젝트에 부착하여 사용한다.
/// </summary>
public class GameStartTrigger : MonoBehaviour
{
    // 트리거가 이미 발동되었는지 확인하는 플래그
    private bool hasBeenTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // 아직 발동되지 않았고, 들어온 오브젝트가 "Player" 태그를 가지고 있을 경우
        if (!hasBeenTriggered && other.CompareTag("Player"))
        {
            // 한 번만 실행되도록 플래그 설정
            hasBeenTriggered = true;

            Debug.Log("게임 시작 트리거가 발동되었습니다!");

            // GameManager의 StartTutorialSequence() 함수를 호출하여 튜토리얼을 시작한다.
            GameManager.Instance.StartTutorialSequence();
            
            // 자신의 역할을 다했으므로, 이 트리거 오브젝트를 파괴한다.
            Destroy(gameObject);
        }
    }

    // Scene 뷰에서 트리거 영역을 쉽게 식별할 수 있도록 기즈모를 그린다.
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f); // 초록색, 반투명
        Gizmos.matrix = transform.localToWorldMatrix;
        BoxCollider col = GetComponent<BoxCollider>();
        if (col != null)
        {
            Gizmos.DrawCube(col.center, col.size);
        }
    }
}
