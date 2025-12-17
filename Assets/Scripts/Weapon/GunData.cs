using UnityEngine;

// 프로젝트 창에서 우클릭 -> Create -> Data -> Gun 으로 생성할 수 있게 메뉴를 추가해주는 어노테이션
[CreateAssetMenu(fileName = "Gun", menuName = "Data/Gun")]
public class GunData : ItemData
{
    [Header("고유 정보")]
    public float damage;   // 데미지
    public float range;    // 사정거리 (지금 당장 쓰진 않아도 나중을 위해)

    [Header("Shooting")]
    public float fireRate; // 연사 속도 (초당 발사 수)
    public bool isAutomatic = false; // 자동 발사 여부
    
    // 샷건 같은 무기를 위한 설정
    [Header("Shotgun Settings")]
    public int pelletsPerShot = 1; // 한 번에 발사되는 총알(펠릿) 수. 일반 총은 1.
    public float spreadAngle = 0f; // 총알이 퍼지는 각도. 일반 총은 0.

    [Header("Ammo")]
    public int maxAmmo = 30; // 최대 장탄 수

    [Header("Prefabs & VFX")]
    public GameObject bulletPrefab; // 총알 프리팹
    // public GameObject muzzleFlash; // 총구 화염 효과 (나중에 추가)
    // public AudioClip shotSound;    // 발사 소리 (나중에 추가)
}