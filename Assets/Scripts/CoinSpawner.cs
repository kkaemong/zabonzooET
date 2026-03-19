using UnityEngine;
using System.Collections;

public class CoinSpawner : MonoBehaviour
{
    [Header("--- [ 1. Spawn Interval ] ---")]
    public float minSpawnInterval = 2f; 
    public float maxSpawnInterval = 5f; 
    
    [Header("--- [ 2. Height Randomization ] ---")]
    public float minYOffset = -1f; 
    public float maxYOffset = 1f;  
    public float maxYLimit = 3.5f;   // 코인이 생성될 수 있는 최대 높이 (점프 가능 범위)

    [Header("--- [ 3. Burst Mode (Cookie Run Style) ] ---")]
    public int burstCount = 10;      // Number of coins in a sequence
    public float burstDelay = 0.15f; // 코인 사이의 시간 간격을 늘려 거리를 벌립니다. (기존 0.08f)
    public float yStep = 0f;         // Vertical offset for staircase patterns
    public float waveAmplitude = 1.5f; // Amplitude for wavy patterns

    [Header("--- [ 4. Coin Prefabs ] ---")]
    public GameObject[] coinPrefabs; 

    [Header("--- [ 5. Overlap Check (Obstacles) ] ---")]
    public LayerMask obstacleLayer; // 방해물 레이어 (예: Disturb, Obstacle 등)
    public Vector2 checkBoxSize = new Vector2(2f, 2f); // 감지 상자 크기 (가로, 세로)

    private float timer = 0f;
    private float currentInterval = 0f;
    public static bool isSpawningCoinSequence = false; // 현재 코인이 쏟아지고 있는지 확인 (disturb 스포너와 통신용)
    public static float lastCoinSequenceEndTime = 0f; // 코인 생성이 끝난 직후 시간 저장용

    void Start()
    {
        SetRandomInterval();
    }

    void Update()
    {
        // 게임이 멈춰있으면 코인 생성 타이머 멈춤
        if (GameManager.IsGamePaused) return;

        // 💡 [자연스러운 간격 유지] 방해물이 방금 막(예: 1.5초 이내) 스폰되었으면 코인 타이머를 일시정지 (시간 째로 대기)
        if (Time.time - spawner.lastObstacleSpawnTime < 1.5f)
        {
            return; 
        }

        timer += Time.deltaTime;

        if (timer >= currentInterval && !isSpawningCoinSequence)
        {
            SpawnCoinSequence();
            timer = 0f; 
            SetRandomInterval(); 
        }
    }

    void SetRandomInterval()
    {
        currentInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
    }

    void SpawnCoinSequence()
    {
        if (coinPrefabs == null || coinPrefabs.Length == 0) return;
        StartCoroutine(ExecuteSequence());
    }

    IEnumerator ExecuteSequence()
    {
        isSpawningCoinSequence = true; // 생성 시작
        
        // 0: 일직선(바닥)
        // 1: 계단 위로(공중)
        // 2: 계단 아래로(공중)
        // 3: S자 곡선(공중)
        // 4: 바닥 고정
        // 5: 포물선/V자 (공중)
        // 6: 산/Λ자 모양 (공중)
        int patternType = Random.Range(0, 7); 
        
        // 일직선 패턴(0, 4)은 무조건 바닥 높이로 설정
        bool isGroundPattern = (patternType == 0 || patternType == 4);
        
        // 공중 패턴은 바닥보다 약간 높은 곳에서 시작하도록 랜덤 범위 조절
        float startY = isGroundPattern ? 0 : Random.Range(minYOffset + 1.5f, maxYOffset);
        
        // 바닥 패턴들은 개수를 좀 더 넉넉하게
        int localBurstCount = isGroundPattern ? burstCount * 2 : burstCount; 
        
        for (int i = 0; i < localBurstCount; i++)
        {
            if (coinPrefabs == null || coinPrefabs.Length == 0) break;
            
            int randomIndex = Random.Range(0, coinPrefabs.Length);
            GameObject selectedPrefab = coinPrefabs[randomIndex];

            float currentY = 0;

            switch (patternType)
            {
                case 1: // 계단 오르기 (Staircase Up)
                    currentY = startY + (i * yStep); 
                    break;
                case 2: // 계단 내려가기 (Staircase Down)
                    currentY = startY - (i * yStep);
                    break;
                case 3: // S자 곡선 (크고 시원하게)
                    currentY = startY + Mathf.Sin(i * 0.5f) * waveAmplitude * 1.5f;
                    break;
                case 5: // V자 모양 (포물선)
                    float mid = localBurstCount / 2f;
                    currentY = startY + Mathf.Abs(i - mid) * 0.4f;
                    break;
                case 6: // 산(Λ) 모양
                    float mid2 = localBurstCount / 2f;
                    currentY = startY + (mid2 - Mathf.Abs(i - mid2)) * 0.5f;
                    break;
                case 4: // 바닥 고정 (Floor Pattern)
                case 0: // 일직선 (바닥에 딱 붙어서)
                default:
                    currentY = minYOffset - transform.position.y; 
                    break;
            }
            
            float finalY = (isGroundPattern) ? minYOffset : (transform.position.y + currentY);
            
            // --- [ 지면 관통 및 높이 제한 방지 ] ---
            finalY = Mathf.Clamp(finalY, minYOffset, maxYLimit);

            Vector3 spawnPosition = new Vector3(transform.position.x, finalY, transform.position.z);

            // --- [ 방해물과의 겹침 방지 체크 ] ---
            // 물리 엔진 동기화 (생성 직후 감지 확률 높임)
            Physics2D.SyncTransforms();

            // 생성할 위치에 무언가 있는지 이름과 태그로도 꼼꼼히 확인합니다 (Layer 설정 누락 대비)
            Collider2D[] hits = Physics2D.OverlapBoxAll(spawnPosition, checkBoxSize, 0f);
            bool isOverlap = false;
            string overlapName = "";

            foreach (var h in hits)
            {
                if (h.gameObject == this.gameObject) continue;

                string objName = h.gameObject.name.ToLower();
                string objTag = h.gameObject.tag.ToLower();
                // 겹치면 안 되는 녀석들 (장애물이나 생겨난 다른 코인: 이름 또는 태그로 검사)
                if (objName.Contains("disturb") || objName.Contains("obstacle") || objName.Contains("coin") ||
                    objTag.Contains("disturb") || objTag.Contains("obstacle") || objTag.Contains("coin"))
                {
                    isOverlap = true;
                    overlapName = h.gameObject.name;
                    break;
                }
            }
            
            if (!isOverlap) // 방해물이 없을 때만 생성
            {
                Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
            }
            else
            {
                Debug.Log($"<color=yellow>[CoinSpawner]</color> 겹침 감지! ({overlapName}) 위치: {spawnPosition}. 코인 생성을 건너뜁니다.");
            }

            yield return new WaitForSeconds(burstDelay);
            
            // 만약 코인을 쏟아내는 도중에 몬스터를 만나 모달 창이 떴다면 끝날 때까지 대기
            yield return new WaitUntil(() => !GameManager.IsGamePaused);
        }
        isSpawningCoinSequence = false; // 생성 완료
        lastCoinSequenceEndTime = Time.time; // 💡 쏟아내기가 끝난 시간 기록
    }

    // 💡 [새 패턴] 장애물을 자연스럽게 뛰어넘도록 유도하는 포물선 코인 스폰
    public void SpawnCoinArcOver(Vector3 obstaclePos)
    {
        if (coinPrefabs == null || coinPrefabs.Length == 0) return;

        int coinCount = 5;       // 아치를 이룰 코인 개수
        float gapX = 1.6f;       // 코인 사이의 X축 간격 (이동 속도를 고려해 넓게)
        
        // y 축: 포물선 방정식 y = -a(x)^2 + h
        float peakY = obstaclePos.y + 4.0f; // 점프 최고점 근처 (보통 캐릭터 점프 높이)
        float floorY = obstaclePos.y + 1.0f; // 장애물 살짝 위
        
        for (int i = 0; i < coinCount; i++)
        {
            int randomIndex = Random.Range(0, coinPrefabs.Length);
            GameObject prefab = coinPrefabs[randomIndex];
            
            // i=0: 왼쪽, i=2: 중앙(최고점), i=4: 오른쪽
            float offsetX = (i - 2) * gapX; 
            
            // i - 2 값: -2, -1, 0, 1, 2
            // Y 위치 계산: 정점에서 아래로 얼마나 떨어질지 (곡률 조절)
            float dropAmount = Mathf.Pow(Mathf.Abs(i - 2), 2) * 0.7f; 
            float offsetY = peakY - dropAmount; 
            
            // 지면(혹은 장애물 ниж부) 아래로 꺼지지 않게 하한선 설정
            if (offsetY < floorY) offsetY = floorY;
            offsetY = Mathf.Clamp(offsetY, minYOffset, maxYLimit); // 기존 제한 시스템도 준수
            
            Vector3 coinPos = new Vector3(obstaclePos.x + offsetX, offsetY, obstaclePos.z);
            
            // 코인이 생성되면서 mover.cs에 의해 장애물과 같은 속도로 왼쪽으로 이동합니다.
            Instantiate(prefab, coinPos, Quaternion.identity);
        }
    }

    // 에디터 씬 뷰에서 감지 범위를 시각적으로 표시 (설정 도움용)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        // 생성될 것으로 예상되는 위치(현재 스패너 X, 각 패턴 Y)에 상자를 그립니다.
        // 여기서는 대표로 스패너 위치에 하나만 표시합니다.
        Gizmos.DrawWireCube(transform.position, checkBoxSize);
    }
}
