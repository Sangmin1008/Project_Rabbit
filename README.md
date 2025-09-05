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


## **기믹**

| 스크립트 | 내용 | 기여자 |
|--------------------------------------------------------------------------------------------------------------|------------|-----|
| [FallDetector.cs](https://github.com/RanKa110/Rabbit/blob/main/Assets/02.%20Scripts/Gimmick/FallDetector.cs) | 맵 경계 추락 감지 | 박상민 |
---

<br>
<br>

# 3. 사용 에셋

- [UI User Interface Pack - Cyber by ToffeeCraft](https://toffeecraft.itch.io/ui-user-interface-pack-cyber)
- [Smoke n Dust 04 by pimen](https://pimen.itch.io/smoke-n-dust-04)
- [Battle VFX: Hit Spark by pimen](https://pimen.itch.io/battle-vfx-hit-spark)
- [Smoke FX by jasontomlee](https://jasontomlee.itch.io/smoke-fx)
- [Residential Area Bosses Pixel Art Download - CraftPix.net](https://craftpix.net/product/residential-area-bosses-pixel-art/CraftPix.net)
- [Bombs and Explosions Pixel Art Set Download - CraftPix.net](https://craftpix.net/product/bombs-and-explosions-pixel-art-set/)
- [Residential Area Tileset Pixel Art Download - CraftPix.net](https://craftpix.net/product/residential-area-tileset-pixel-art/)
