using UnityEngine;

public class spawner : MonoBehaviour
{
    [Header("--- [ Simple Obstacle Spawner ] ---")]
    public GameObject[] obstaclePrefabs; 
    public float minSpawnInterval = 2f; 
    public float maxSpawnInterval = 5f; 
    
    [Header("--- [ Overlap Check ] ---")]
    public Vector2 checkBoxSize = new Vector2(2f, 2f); 
    public LayerMask targetLayer; 
    
    public static float lastObstacleSpawnTime = 0f; // 코인 스포너와 통신용

    private float timer = 0f;
    private float currentInterval = 0f;

    private bool isObstacle = false;

    void Start()
    {
        SetRandomInterval();
        
        // 이 스포너가 장애물 스포너인지 건물 스포너인지 이름으로 구분 (오타 distrub 포함)
        isObstacle = gameObject.name.ToLower().Contains("disturb") || gameObject.name.ToLower().Contains("distrub") || gameObject.name.ToLower().Contains("obstacle");

        // 장애물 스포너라면 코인 스포너와 X 좌표를 동일하게 맞춤 (겹침 완벽 방지를 위함)
        if (isObstacle)
        {
            CoinSpawner cs = FindObjectOfType<CoinSpawner>();
            if (cs != null)
            {
                Vector3 newPos = transform.position;
                newPos.x = cs.transform.position.x;
                transform.position = newPos;
            }
        }
    }

    void Update()
    {
        // 게임이 멈춰있거나 강제 종료상태(클리어/패배)면 장애물 생성 원천 차단
        if (GameManager.IsGamePaused || GameManager.isGameOver) return;

        // 💡 장애물 스포너일 때만 코인 타이머를 눈치 봄 (건물 스포너는 간섭받지 않음!)
        if (isObstacle)
        {
            // 💡 [자연스러운 간격 유지] 코인이 쏟아지는 중이거나, 끝난 직후(1.5초 이내)면 장애물 타이머 일시정지
            if (CoinSpawner.isSpawningCoinSequence || Time.time - CoinSpawner.lastCoinSequenceEndTime < 1.5f)
            {
                return; 
            }
        }

        timer += Time.deltaTime;

        if (timer >= currentInterval)
        {
            SpawnObstacle();
            timer = 0f; 
            SetRandomInterval(); 
        }
    }

    void SetRandomInterval()
    {
        // 1000m 완주에 다가갈수록 스폰 간격이 짧아짐 (난이도 상승)
        float progress = Mathf.Clamp01(GameManager.distanceTraveled / 1000f);
        float multiplier = Mathf.Lerp(1f, 0.4f, progress); // 후반엔 스폰 간격이 40% 수준으로 짧아짐
        currentInterval = Random.Range(minSpawnInterval, maxSpawnInterval) * multiplier;
    }

    void SpawnObstacle()
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) return;

        int randomIndex = Random.Range(0, obstaclePrefabs.Length);
        GameObject selectedPrefab = obstaclePrefabs[randomIndex];

        Vector3 spawnPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);

        // --- [ 중복 생성 방지 체크 ] ---
        Physics2D.SyncTransforms();
        Collider2D[] hits = Physics2D.OverlapBoxAll(spawnPosition, checkBoxSize, 0f);
        bool isOverlap = false;

        foreach (var h in hits)
        {
            if (h.gameObject == this.gameObject) continue;

            string objName = h.gameObject.name.ToLower();
            string objTag = h.gameObject.tag.ToLower();
            
            if (objName.Contains("disturb") || objName.Contains("obstacle") || objName.Contains("coin") || objName.Contains("soldier") ||
                objTag.Contains("disturb") || objTag.Contains("obstacle") || objTag.Contains("coin") || objTag.Contains("soldier"))
            {
                isOverlap = true;
                break;
            }
        }
        
        if (isOverlap)
        {
            return;
        }

        Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
        
        if (isObstacle)
        {
            // 💡 장애물이 방금 스폰되었음을 코인 스포너에게 알림
            lastObstacleSpawnTime = Time.time;

            // 💡 70% 확률로 점프 궤적을 알려주는 코인 아치를 방해물 위에 생성!
            if (Random.value < 0.7f)
            {
                CoinSpawner cs = FindObjectOfType<CoinSpawner>();
                if (cs != null)
                {
                    cs.SpawnCoinArcOver(spawnPosition);
                }
            }
        }
    }
}
