using UnityEngine;
using UnityEngine.EventSystems; // IPointerClickHandler 사용을 위해 추가

/// <summary>
/// 튜토리얼 패널이 마우스 클릭을 감지하면 게임 플레이를 시작하는 스크립트.
/// 이 스크립트는 튜토리얼 패널 UI 오브젝트에 부착되어야 한다.
/// </summary>
public class TutorialPanelClickDismiss : MonoBehaviour, IPointerClickHandler
{
    /// <summary>
    /// 마우스 클릭 이벤트가 발생했을 때 호출된다.
    /// </summary>
    /// <param name="eventData">클릭 이벤트 데이터</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        // GameManager의 BeginGameplay() 함수를 호출하여 실제 게임 플레이를 시작한다.
        // 이 스크립트가 붙은 오브젝트는 UIManager에 의해 자동으로 비활성화된다.
        GameManager.Instance.BeginGameplay();
    }
}
