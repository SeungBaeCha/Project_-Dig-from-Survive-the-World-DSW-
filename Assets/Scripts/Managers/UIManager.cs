using UnityEngine;
using TMPro; // TextMeshPro를 사용하기 위해 꼭 추가해야 해!

/// <summary>
/// 게임의 모든 UI 요소를 관리하는 싱글톤 매니저
/// </summary>
public class UIManager : MonoBehaviour
{
    // 싱글톤 인스턴스: 다른 스크립트에서 UIManager.Instance 로 쉽게 접근할 수 있게 해준다.
    public static UIManager Instance { get; private set; }

    [Header("상호작용 UI")]
    // 상호작용 텍스트를 표시할 TextMeshPro UI 요소. Unity 에디터에서 연결해줘야 해.
    [SerializeField] private TextMeshProUGUI interactionText;

    private void Awake()
    {
        // --- 싱글톤 패턴 구현 ---
        // 만약 인스턴스가 아직 없고, 이 UIManager가 첫 번째라면
        if (Instance == null)
        {
            // 이 인스턴스를 static 변수에 할당
            Instance = this;
            // 씬이 바뀌어도 이 오브젝트가 파괴되지 않게 설정 (필요에 따라 주석 처리/해제)
            // DontDestroyOnLoad(gameObject); 
        }
        // 만약 인스턴스가 이미 존재하는데, 또 다른 UIManager가 씬에 있다면
        else
        {
            // 새로 생긴 UIManager를 파괴해서 단 하나의 인스턴스만 존재하도록 보장
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 게임 시작 시에는 상호작용 텍스트를 보이지 않게 초기화
        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 상호작용 텍스트를 화면에 보여주는 함수
    /// </summary>
    /// <param name="textToShow">화면에 표시할 문장</param>
    public void ShowInteractionText(string textToShow)
    {
        if (interactionText != null)
        {
            // 텍스트 내용을 설정하고
            interactionText.text = textToShow;
            // 텍스트 오브젝트를 활성화해서 화면에 보여준다
            interactionText.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 상호작용 텍스트를 화면에서 숨기는 함수
    /// </summary>
    public void HideInteractionText()
    {
        if (interactionText != null)
        {
            // 텍스트 오브젝트를 비활성화해서 화면에서 숨긴다
            interactionText.gameObject.SetActive(false);
        }
    }
}
