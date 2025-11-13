using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    // 인스펙터창에 보이도록 설정
    public TextMeshProUGUI timerText;

    void Update()
    {
        // 게임매니저가 실행될때
        if (GameManager.Instance != null)
        {
            // remainingTime 과 isNight 의 변수를 Gamemanager안 함수를 가져온다
            float remainingTime = GameManager.Instance.GetRemainingTime();
            bool isNight = GameManager.Instance.IsNight;

            // dayNightStatus 변수는 밤과 낮을 확인하는 변수
            string dayNightStatus = isNight ? "밤" : "낮";
            
            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);

            // 타이머의 작동
            timerText.text = $"{minutes:00}:{seconds:00} {dayNightStatus}";
        }
    }
}