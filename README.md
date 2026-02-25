
# 🎮 Dig from Survive the World (DSW)

**Unity / C# 기반 1인 개발 생존 디펜스 게임**

플레이어는 낮 동안 자원을 파밍하고, 밤에는 점점 증가하는 적으로부터 생존해야 합니다.  
단순 기능 구현을 넘어 시스템 구조 설계와 문제 해결 과정에 집중하여 개발했습니다.

---

# ✔ 기술 스택

- Unity (URP)
- C#
- NavMesh AI
- ScriptableObject 기반 데이터 관리
- Raycasting
- Global Volume (Post Processing)
- Inventory / Crafting System
- Equipment Architecture

---

# 🔥 주요 기술 구현

## ✅ 상태 기반 화면 피드백 시스템

### 문제  
기존 생존 게임은 체력과 허기를 UI 숫자로만 표현해 플레이어가 위험 상태를 즉각 인지하기 어려웠습니다.

### 해결  
URP Global Volume을 활용하여 상태값과 Post Processing 효과를 연동했습니다.

- 체력 감소 → 화면 붉은 색조 증가  
- 허기 감소 → 채도 감소 및 화면 흐림  

```csharp
volume.weight = Mathf.Lerp(volume.weight, targetWeight, Time.deltaTime * speed);
```

### 결과  
- UI 의존도 감소  
- 플레이 몰입도 상승  
- 위기 상황을 직관적으로 전달  

---

## ✅ 장비 시스템 아키텍처 설계

### 문제  
WeaponHold와 ShovelHold가 동시에 실행되어 장비 상태 충돌이 발생했습니다.

### 해결  
중앙 제어 역할을 하는 **EquipmentManager**를 설계하여 단일 장비만 활성화되도록 상태를 관리했습니다.

```
Player
 └ EquipmentManager
     ├ WeaponHold
     └ ShovelHold
```

### 결과  
- 장비 충돌 제거  
- 확장 가능한 구조 확보  
- 신규 장비 추가 비용 감소  

---

## ✅ 아이템 즉시 재획득 버그 해결

### 문제  
무기를 버린 직후 상호작용 키를 연타하면 즉시 다시 획득되는 버그가 발생했습니다.

### 원인  
Drop 이후 Trigger가 즉시 활성화되며 입력 타이밍이 충돌했습니다.

### 해결  
Coroutine 기반 Pickup Delay 로직을 적용했습니다.

```csharp
yield return new WaitForSeconds(pickupDelay);
canPickup = true;
```

### 결과  
- 입력 악용 방지  
- 상호작용 안정성 확보  

---

## ✅ 부분 지형 파괴 시스템

### 문제  
Diggable 오브젝트가 전체 단위로 파괴되는 문제가 있었습니다.

### 해결  
- 3중 for문을 활용해 voxel 좌표 순회  
- Raycast 기반 충돌 감지  
- Instantiate로 지형을 동적으로 재구성  

### 결과  
- 부분 파괴 가능한 환경 구현  
- 게임 플레이 전략 다양성 증가  

---

## ✅ NavMesh 실시간 업데이트 AI

### 문제  
복잡한 지형에서 적이 경로를 찾지 못하는 문제가 발생했습니다.

### 해결  
NavMesh를 주기적으로 재Bake하여 동적으로 변하는 지형에도 대응하도록 설계했습니다.

### 결과  
- AI 추적 안정성 향상  
- 플레이 긴장감 강화  

---

# 🎥 Gameplay

👉 플레이 영상 

https://youtu.be/7cmiArroW48

구성 : 
- 전투 장면  
- 지형 파괴  
- 아이템 파밍  
- 낮과 밤 생존
- 날씨 구성
- 아이템 보급 
- 인벤토리 & 조합 창 
- 마우스 감도 조절
- 여러 적 구성
- 여러 무기 구성 

---

# 📌 프로젝트 정보

- 개발 형태 : 1인 개발  
- 개발 기간 : 약 3개월  
- 장르 : 생존 / 디펜스  
- 플랫폼 : PC  

---

# 💡 개발 중점

이 프로젝트는 단순한 기능 구현이 아닌 아래 역량을 증명하는 것을 목표로 했습니다.

- 시스템 설계 능력  
- 구조적인 문제 해결 능력  
- 확장 가능한 코드 작성  
- 게임 플레이 경험 개선  

---

# 🚀 향후 개선 예정

- 사운드 및 BGM 개선  
- 게임 방법 안내 UI 개선   
- AI 행동 패턴 개선  
- 최적화 작업

---

👉 **GitHub Repository:**  
https://github.com/SeungBaeCha/Project_-Dig-from-Survive-the-World-DSW-
