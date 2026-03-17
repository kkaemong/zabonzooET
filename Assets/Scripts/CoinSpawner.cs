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
    private bool isSpawning = false; // 현재 코인이 생성 중인지 확인하는 플래그

    void Start()
    {
        SetRandomInterval();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= currentInterval && !isSpawning)
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
        isSpawning = true; // 생성 시작
        // 0: 일직선(바닥), 1: 계단 위로(공중), 2: 계단 아래로(공중), 3: S자 곡선(공중), 4: 바닥 고정
        int patternType = Random.Range(0, 5); 
        
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
                case 3: // S자 곡선 (S-Shape/Sine Wave)
                    currentY = startY + Mathf.Sin(i * 0.4f) * waveAmplitude;
                    break;
                case 4: // 바닥 고정 (Floor Pattern)
                case 0: // 일직선 (바닥에 딱 붙어서)
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

            // 생성할 위치에 이미 방해물이 있는지 확인합니다. (상자 형태 감지)
            Collider2D obstacleHit = Physics2D.OverlapBox(spawnPosition, checkBoxSize, 0f, obstacleLayer);
            
            if (obstacleHit == null) // 방해물이 없을 때만 생성
            {
                Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
            }
            else
            {
                Debug.Log($"<color=yellow>[CoinSpawner]</color> 겹침 감지! ({obstacleHit.gameObject.name}) 위치: {spawnPosition}. 코인 생성을 건너뜁니다.");
            }

            yield return new WaitForSeconds(burstDelay);
        }
        isSpawning = false; // 생성 완료
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
