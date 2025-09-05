# TSEROF_Code

# 만든 사람들
- ### 이종민
- ### 권기원
- ### 박상민
- ### 박진우
- ### 박재윤

# [🤍 팀 노션](https://www.notion.so/teamsparta/RABBIT-2172dc3ef5148061810fcc91a54ea1ed)

# [🎞 시연 영상](https://youtu.be/8UbreDtBYWE)

# 목차

### 1. 게임 개요 및 개발 기간
### 2. 구현 기능
### 3. 기능 명세서
### 4. 사용 에셋

---
<br>
<br>

# 게임 미리보기
![TitleScene](https://github.com/RanKa110/Rabbit/blob/main/Assets/99.%20Externals/image.png)
![TutorialScene](https://github.com/RanKa110/Rabbit/blob/main/Assets/99.%20Externals/image%20(1).png)
![vsBossGIF](https://github.com/RanKa110/Rabbit/blob/main/Assets/99.%20Externals/3.gif)
![vsEnemy](https://github.com/RanKa110/Rabbit/blob/main/Assets/99.%20Externals/image%20(2).png)
---
<br>
<br>

# 1. 게임 개요 및 개발 기간

- **게임명** : `Rabbit`
- **설명** : 2D 픽셀 횡스크롤 액션 어드벤쳐 게임
- **개요** : 캐릭터를 조작해 적들을 물리치고 나아가 스테이지를 클리어하자.
- **게임 방법**
    - [AD] or [⬅️➡️] : 이동
    - [SPACE] : 점프
    - [SPACE] + [SPACE] : 2단 점프
    - [Left Mouse] : 공격
    - [Right Mouse] : 방어
    - [E] : 상호작용
    - [Esc] : 옵션
- #### **타겟 플랫폼** : Microsoft Windows
- #### **개발 기간** `2025.06.20 ~ 2025.08.12`
- #### **개발 환경** :<img width="30" height="30" alt="VS_Logo" src="https://github.com/dlghdwns97/TSEROF_Code/assets/73785455/dd8e3f5f-a671-4153-881b-6e39e445a80f"> <img width="30" height="30" alt="VS_Logo" src="https://github.com/dlghdwns97/TSEROF_Code/assets/73785455/a2070391-f97f-4a51-81c2-1c2728eb233c">
- #### **개발 엔진** : <img width="70" height="30" alt="Unity_Logo" src="https://github.com/dlghdwns97/TSEROF_Code/assets/73785455/a53ce756-54c2-44d7-870c-71637721bb2f"> 6000.1.8f1
- #### **버전 관리** : <img width="30" height="30" alt="VS_Logo" src="https://github.com/dlghdwns97/TSEROF_Code/assets/73785455/fb87bd07-fa87-4965-a582-468f3122bf41">


---
<br>
<br>

# 2. 구현 기능

## **시작 화면**
- 게임 시작시 실행되는 씬
- 새 게임, 불러오기, 설정, 게임 종료 선택지가 있다.

## **새 게임 선택 화면**
- 튜토리얼을 진행할지 선택하는 씬.
- "아니오" 선택 시 튜토리얼과 함께 게임이 시작되며 "예" 선택 시 튜토리얼이 스킵되고 MapScene이 재생되며 게임이 시작된다.
## **불러오기 선택 화면**
- 인 게임내 저장한 시점을 선택하는 씬.
- 총 10개의 선택지가 존재하며 해당 선택지 마다 저장한 시점을 불러올 수 있다.

## **게임 화면**
- 게임의 최종 목표 (클리어 조건) : 스테이지 끝에 있는 Boss와 대결하여 승리하기
- 정해진 구역마다 출현하는 적들을 처치하며 해당 구역에 맞는 상황조치가 게임의 재미요소를 결정한다.

---
<br>
<br>

# 3. 기능 명세서

<details>
<summary>UML, 기능 정리</summary>

#### 보스 구조
![Untitled (1)](https://github.com/RanKa110/Rabbit/blob/main/Assets/99.%20Externals/%EC%B5%9C%EC%A2%85%ED%94%84%EB%A1%9C%EC%A0%9D%ED%8A%B8_%EB%B3%B4%EC%8A%A4%ED%81%B4%EB%9E%98%EC%8A%A4%EB%8B%A4%EC%9D%B4%EC%96%B4%EA%B7%B8%EB%9E%A8.png)

#### 적 소환
![Untitled (1)](https://github.com/RanKa110/Rabbit/blob/main/Assets/99.%20Externals/%EC%B5%9C%EC%A2%85%ED%94%84%EB%A1%9C%EC%A0%9D%ED%8A%B8_%EC%A0%81%EC%86%8C%ED%99%98%ED%81%B4%EB%9E%98%EC%8A%A4.png)

#### 투사체
![Untitled (2)](https://github.com/RanKa110/Rabbit/blob/main/Assets/99.%20Externals/%EC%B5%9C%EC%A2%85%ED%94%84%EB%A1%9C%EC%A0%9D%ED%8A%B8_%ED%88%AC%EC%82%AC%EC%B2%B4%ED%81%B4%EB%9E%98%EC%8A%A4%EA%B5%AC%EC%A1%B0%EB%8F%84.png)

#### 적(Elite,Nomal)
![Untitled (5)](https://github.com/RanKa110/Rabbit/blob/main/Assets/99.%20Externals/%EC%B5%9C%EC%A2%85%ED%94%84%EB%A1%9C%EC%A0%9D%ED%8A%B8_%EC%A0%81%ED%81%B4%EB%9E%98%EC%8A%A4%EB%8B%A4%EC%9D%B4%EC%96%B4%EA%B7%B8%EB%9E%A8.png)
</details>

## **역할 분담**

| 내용 | 기여자 |<br>
| 플레이어 스프라트 아트, PM, 맵 디자인, 기획 총괄 | 이종민 |<br>
| 맵 디자인 및 제작 보조, 카메라 연출, 연출 총괄, QA, 시연 영상 촬영 및 제작 | 권기원 |<br><br>
| 플레이어 FSM, 패럴랙스 스크롤링 구현, 카메라 연출 및 이펙트, 카메라 스위칭 시스템, 상호작용 시스템,<br>| 대화 창 시스템, 세이브&로드 시스템, VFX&파티클 이펙트,<br>| UI/UX, UI 애니메이션, 튜토리얼 및 맵 제작, 액션 연출, 리팩토링 총괄  | 박상민 |<br><br>
| 보스 FSM, 정예 등급 몬스터 3종 (근거리, 원거리, 쉴드몹) FSM 구현, 적 전체 리팩토링 | 박진우 |<br>
| 일반 등급 몬스터 4종(근거리, 원거리, 멀티샷, 유도미사일) FSM 구현 | 박재윤 |


## **데이터 저장(JSON)**

| 스크립트 | 내용 | 기여자 |
|------------------------------------|-----------|-----|
| [SaveManager.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Manager/SaveManager.cs)                 | 세이브 저장/로드 | 박상민 |

## **캐릭터**

| 스크립트 | 내용 | 기여자 |
|------------------------------------|------------| -- |
| [Assets>Player](https://github.com/RanKa110/Rabbit/tree/main/Assets/02.%20Scripts/Player)                  | 플레이어 기능 구현 | 박상민 |

## **카메라**

| 스크립트 | 내용 | 기여자 |
|------------------------------------|-------------|-----|
| [Assets>Camera](https://github.com/RanKa110/Rabbit/tree/main/Assets/02.%20Scripts/Camera)                  | 카메라 이펙트/스위칭 | 박상민 |

## **UI / UI Animation**

| 스크립트 | 내용 | 기여자 |
|------------------------------------|---------------|-----|
| [Assets>UI](https://github.com/RanKa110/Rabbit/tree/main/Assets/02.%20Scripts/UI)                      | UI 및 UI 애니메이션 | 박상민 |


## **보스**

### 1) Attack Patterns
| 스크립트 | 내용 | 기여자 |
|---|---|---|
| [HitData.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Boss/Attack/HitData.cs) | 공격 시 데미지, 넉백, 공격 타입 등의 정보를 담는 데이터 구조체/클래스. | 박진우  |
| [BaseAttackPattern.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Boss/Attack/BossPatterns/BaseAttackPattern.cs) | 모든 보스 공격 패턴의 기본 동작과 인터페이스를 정의하는 추상 클래스 | 박진우  |
| [PatternContext.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Boss/Attack/BossPatterns/PatternContext.cs) | 패턴 실행에 필요한 보스, 타겟, 풀, 데이터 등을 담아 전달하는 컨텍스트 클래스 | 박진우  |
| [ChargePattern.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Boss/Attack/BossPatterns/ChargePattern.cs) | 돌진 패턴: 윈드업 후 지정 방향으로 빠르게 이동하며 충돌 시 피해를 주는 패턴 | 박진우  |
| [ShellingPattern.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Boss/Attack/BossPatterns/ShellingPattern.cs) | 공중 포격 패턴: 상공에서 포탄/미사일을 낙하시켜 범위 피해를 주는 포격 패턴 | 박진우  |
| [ShootPattern.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Boss/Attack/BossPatterns/ShootPattern.cs) | 연사 패턴: 투사체를 일정 간격으로 연사하는 패턴 | 박진우  |
| [SummonPattern.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Boss/Attack/BossPatterns/SummonPattern.cs) | 소환 패턴: 일정 위치에 적 또는 오브젝트를 소환하는 패턴 | 박진우  |
| [TNTPattern.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Boss/Attack/BossPatterns/TNTPattern.cs) | TNT 투척 패턴: TNT 폭탄을 투척하여 지연 폭발 및 범위 피해를 주는 패턴 | 박진우  |


### 2) Projectiles
| 스크립트 | 내용 | 기여자 |
|---|---|---|
| [BossProjectile.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Boss/Projectiles/BossProjectile.cs) | 보스 전용 탄환. 속도/방향/최대 이동거리, 충돌 시 풀 반환 및 데미지 적용. | 박진우  |
| [FirebombProjectile.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Boss/Projectiles/FirebombProjectile.cs) | 던지는 폭탄형 투사체. 착지/타이머 기반 폭발, 충돌 시 풀 반환 및 데미지 적용. | 박진우 |
| [ShellingProjectile.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Boss/Projectiles/ShellingProjectile.cs) | 포격 패턴에서 소환되는 탄 종류. 지정 지점에 스폰되어 낙하·폭발 처리. 충돌 시 풀 반환 및 데미지 적용 | 박진우 |
| [ProjectilePool.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Boss/Projectiles/ProjectilePool.cs) | 투사체 전용 오브젝트 풀. 투사체 스폰/반납·초기화, 풀링 최적화. | 박진우 |

### 3) Effects
| 스크립트 | 내용 | 기여자 |
|---|---|---|
| [ExplosionEffect.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Boss/Effects/ExplosionEffect.cs) | 폭발 비주얼·사운드 트리거(폭탄·미사일 공용). | 박진우 |
| [Phase2TransitionEffects.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Boss/Effects/Phase2TransitionEffects.cs) | 페이즈 전환 시 실행되는 연출용 이펙트 | 박진우 |

### 4) Handlers
| 스크립트 | 내용 | 기여자 |
|---|---|---|
| [AnimationHandler.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Boss/Handlers/AnimationHandler.cs) | 보스의 애니메이션 상태 제어 및 애니메이션 관련 로직 처리 | 박진우 |
| [AttackHandler.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Boss/Handlers/AttackHandler.cs) | 공격 데이터 적용, 히트 이벤트 처리, 사운드 재생 트리거 | 박진우 |
| [BossAudioHandler.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Boss/Handlers/BossAudioHandler.cs) | 보스 공격, 돌진, 폭발 등 사운드 재생 및 AudioManager 연동 | 박진우 |
| [BossEffectHandler.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Boss/Handlers/BossEffectHandler.cs) | 보스의 이펙트(잔상, 폭발, 먼지 등) 실행 및 관리 | 박진우 |
| [BossPhaseTransitionHandler.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Boss/Handlers/BossPhaseTransitionHandler.cs) | 보스 페이즈 전환 시 연출 및 관련 로직 처리 | 박진우 |
| [BossStatHandler.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Boss/Handlers/BossStatHandler.cs) | 보스의 체력, 게이지 등 스탯 관리 | 박진우 |
| [BossVisualHandler.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Boss/Handlers/BossVisualHandler.cs) | 보스 시각 효과(색상 변화, 강조 효과 등) 관리 | 박진우 |
| [ColliderFlipHandler.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Boss/Handlers/ColliderFlipHandler.cs) | 보스 방향 전환 시 콜라이더 회전/위치 보정 처리 | 박진우 |
| [FacingHandler.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Boss/Handlers/FacingHandler.cs) | 타겟 탐색/추적, 방향 전환 등 보스 타겟팅 로직 | 박진우 |
| [MovementHandler.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Boss/Handlers/MovementHandler.cs) | 보스 이동 처리(속도, 회전, 이동 제한 등) | 박진우 |
| [SummonHandler.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Boss/Handlers/SummonHandler.cs) | 보스가 적을 소환하는 로직 관리 | 박진우 |
| [TargetingHandler.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Boss/Handlers/TargetingHandler.cs) | 타겟 탐색/추적, 방향 전환 등 보스 타겟팅 로직 | 박진우 |

### 5) Roots
| 스크립트 | 내용 | 기여자  |
|---|---|---|
| [BossController.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Controller/BossController.cs) | 보스의 메인 컨트롤러 스크립트. FSM(상태 기계) 구동, 각종 핸들러(공격, 이동, 페이즈 전환 등) 초기화 및 참조, 패턴 실행 제어를 담당. 보스의 생명주기(대기 → 전투 → 사망) 전반을 관리하는 핵심 클래스 | 박진우 |
| [BossStates.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/State/Boss/BossStates.cs) | 보스 FSM에 대한 상태 전이 조건, 상태 전이 실행을 관리하는 클래스들을 묶은 namespace | 박진우  |
| [AnimationEventRelay.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Boss/AnimationEventRelay.cs) | 보스의 공격 애니메이션 이벤트를 다른 시스템으로 전달하는 역할 | 박진우  |
| [DamageReceiver.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Boss/DamageReceiver.cs) | 보스가 받는 피해를 처리하고 HP 감소 및 관련 반응을 실행 | 박진우  |


## **적**

### 1) Roots
| 스크립트 | 내용 | 기여자 |
|---|---|---|
| [BaseEnemyController.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Controller/Enemies/BaseEnemyController.cs) | 모든 적 캐릭터의 기본 동작을 정의하는 베이스 클래스. 이동, 공격, 피격 처리 등 공통 로직 포함 | 박진우 |
| [BaseEnemyState.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Controller/Enemies/BaseEnemyController.cs) | 적 AI의 상태를 정의하는 열거형 | 박진우 |
| [BaseEnemyStates.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Controller/Enemies/BaseEnemyController.cs) | 적 FSM 클래스 묶음. Idle, Chasing, Attack, Cooldown, Die 상태를 포함 | 박진우 |


### 2) Elite Enemy
| 스크립트 | 내용 | 기여자 |
|---|---|---|
| [EliteMeleeEnemyController.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Controller/Enemies/Elite/EliteMeleeEnemyController.cs) | 정예 근접 적 AI 제어. 근접 공격, 타이밍 맞춘 타격 효과 처리 | 박진우 |
| [EliteRangedEnemyController.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Controller/Enemies/Elite/EliteRangedEnemyController.cs) | 정예 원거리 적 AI 제어. 원거리 발사 애니메이션 및 투사체 발사 처리 | 박진우 |
| [EliteShieldEnemyController.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Controller/Enemies/Elite/EliteShieldEnemyController.cs) | 방패를 가진 정예 적. 근접 방어 및 특수 패턴 처리 | 박진우 |

### 3) Normal Enemy
| 스크립트 | 내용 | 기여자 |
|---|---|---|
| [NormalMeleeController.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Controller/Enemies/Normal/NormalMeleeController.cs) | 일반 근접 적 AI 제어. 근접 타격, 공격 모션 및 효과 처리 | 박진우 |
| [NormalRangedController.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Controller/Enemies/Normal/NormalRangedController.cs) | 일반 원거리 적 AI 제어. 투사체 발사 및 애니메이션 동기화 처리 | 박진우 |

### 4) Enemy Pool & Wave

| 스크립트 | 내용 | 기여자 |
|---|---|---|
| [EliteEnemyPool.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Enemy/EliteEnemy/Pooling/EliteEnemyPool.cs) | 적들 전용 오브젝트 풀. 트리거 방식을 통해 적 생성, 재사용, 반환 로직 관리 | 박진우 |
| [WaveManager.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Enemy/EliteEnemy/Pooling/WaveManager.cs) | 웨이브 기반 적 스폰 시스템. 웨이브 순서, 소환 간격, 스폰 위치 제어 | 박진우 |

### 5) Attacks
| 스크립트 | 내용 | 기여자 |
|---|---|---|
| [OnAttackEventHandler.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Enemy/EliteEnemy/AnimationEvent/OnAttackEventHandler.cs) | 애니메이션 이벤트를 활용하여 플레이어 공격 시 데미지 입히기 및 공격 사운드 재생을 처리하는 핸들러 | 박진우 |
| [EliteMeleeHitbox.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Enemy/EliteEnemy/Attack/EliteMeleeHitbox.cs) | 정예 근접 적의 히트박스를 관리하며, `HitData`를 활용해 충돌 시 데미지 계산 처리 | 박진우 |
| [EliteProjectile.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Enemy/EliteEnemy/Attack/Projectile/EliteProjectile.cs) | 정예 원거리 적이 사용하는 투사체 동작을 관리하며, 발사 속도 및 데미지 처리 로직 포함 | 박진우 |

---

## **기믹**

| 스크립트 | 내용 | 기여자 |
|--------------------------------------------------------------------------------------------------------------|------------|-----|
| [FallDetector.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Gimmick/FallDetector.cs) | 맵 경계 추락 감지 | 박상민 |
---

<br>
<br>

# 4. 사용 에셋

- [UI User Interface Pack - Cyber by ToffeeCraft](https://toffeecraft.itch.io/ui-user-interface-pack-cyber)
- [Smoke n Dust 04 by pimen](https://pimen.itch.io/smoke-n-dust-04)
- [Battle VFX: Hit Spark by pimen](https://pimen.itch.io/battle-vfx-hit-spark)
- [Smoke FX by jasontomlee](https://jasontomlee.itch.io/smoke-fx)
- [Residential Area Bosses Pixel Art Download - CraftPix.net](https://craftpix.net/product/residential-area-bosses-pixel-art/CraftPix.net)
- [Bombs and Explosions Pixel Art Set Download - CraftPix.net](https://craftpix.net/product/bombs-and-explosions-pixel-art-set/)
- [Residential Area Tileset Pixel Art Download - CraftPix.net](https://craftpix.net/product/residential-area-tileset-pixel-art/)
