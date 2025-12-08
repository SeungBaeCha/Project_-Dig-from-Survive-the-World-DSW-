using System.Collections.Generic; // List를 사용하기 위해 추가
using UnityEngine;

public class Shotgun : Gun
{
    // 샷건은 Fire() 메서드를 다르게 구현
    protected override void Fire()
    {
        // 이번 발사에서 생성된 모든 총알의 콜라이더를 저장할 리스트
        List<Collider> bulletColliders = new List<Collider>();
        
        // 1. 카메라 시점 기준으로 실제 발사 방향의 중앙점을 계산
        Vector3 centralDirection = GetFireDirection();

        // gunData에 설정된 펠릿 수만큼 반복
        for (int i = 0; i < gunData.pelletsPerShot; i++)
        {
            // 중앙 방향을 기준으로 랜덤한 편차(spread)를 적용
            Quaternion spreadRotation = Quaternion.Euler(
                Random.Range(-gunData.spreadAngle, gunData.spreadAngle),
                Random.Range(-gunData.spreadAngle, gunData.spreadAngle),
                0);
            
            // 최종 발사 방향 계산
            Vector3 fireDirection = spreadRotation * centralDirection;

            // 계산된 방향으로 총알의 초기 회전값 설정
            Quaternion bulletRotation = Quaternion.LookRotation(fireDirection);

            // 계산된 방향으로 총알 생성
            GameObject bulletObject = Instantiate(gunData.bulletPrefab, firePoint.position, bulletRotation);
            Bullet bullet = bulletObject.GetComponent<Bullet>();

            if (bullet != null)
            {
                // 플레이어와 충돌하지 않도록 설정
                bullet.IgnorePlayerCollision(playerCollider);
                
                // 총알의 콜라이더를 리스트에 추가
                Collider bulletCollider = bulletObject.GetComponent<Collider>();
                if (bulletCollider != null)
                {
                    bulletColliders.Add(bulletCollider);
                }

                // 총알 발사
                bullet.FireBullet();
            }
        }

        // 생성된 모든 총알들이 서로를 무시하도록 설정
        for (int i = 0; i < bulletColliders.Count; i++)
        {
            for (int j = i + 1; j < bulletColliders.Count; j++)
            {
                Physics.IgnoreCollision(bulletColliders[i], bulletColliders[j]);
            }
        }
    }
}