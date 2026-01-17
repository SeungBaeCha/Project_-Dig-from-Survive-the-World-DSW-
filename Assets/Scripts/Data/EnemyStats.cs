using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Stats", menuName = "Stats/Enemy Stats")]
public class EnemyStats : ScriptableObject
{
    [Header("능력치")]
    public float maxHealth = 100f;
    public float chaseSpeed = 6f;
    public float attackDamage = 10f;

    [Header("시각적 요소")]
    [Tooltip("적의 몸 색깔")]
    public Color bodyColor = Color.white;
    [Tooltip("적의 크기 배율 (X, Y, Z축)")]
    public Vector3 scale = Vector3.one;

    [Header("스폰 설정")]
    [Tooltip("이 적이 처음으로 등장할 수 있는 날짜")]
    public int startDay = 1;
    [Tooltip("스폰 가중치. 높을수록 등장할 확률이 높아짐.")]
    [Range(0.1f, 100f)]
    public float spawnChanceWeight = 1.0f;
}
