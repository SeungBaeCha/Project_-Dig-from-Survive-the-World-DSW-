using UnityEngine;

// AssaultRifle 클래스는 Gun 클래스를 상속받는다.
public class AssaultRifle : Gun
{
    // Gun 클래스의 추상 메서드인 Fire()를 여기서 실제로 구현한다.
    protected override void Fire()
    {
        // 발사에 필요한 모든 항목이 제대로 설정되었는지 확인
        if (gunData == null || gunData.bulletPrefab == null || firePoint == null)
        {
            return; // 하나라도 없으면 발사 로직 중단
        }

        // 1. 카메라 시점 기준으로 실제 발사 방향을 계산
        Vector3 fireDirection = GetFireDirection();

        // 2. 계산된 방향으로 총알의 초기 회전값 설정
        Quaternion bulletRotation = Quaternion.LookRotation(fireDirection);

        // 3. GunData에 설정된 총알 프리팹을 firePoint의 위치와 계산된 방향으로 생성
        GameObject bulletObject = Instantiate(gunData.bulletPrefab, firePoint.position, bulletRotation);
        
        // 방금 생성된 총알에서 Bullet 스크립트 컴포넌트를 가져온다.
        Bullet bullet = bulletObject.GetComponent<Bullet>();

        // Bullet 스크립트가 있는지 확인
        if (bullet != null)
        {
            // 총알이 플레이어와 충돌하지 않도록 설정
            bullet.IgnorePlayerCollision(playerCollider);

            // 총알 발사
            bullet.FireBullet();
        }
    }
}
