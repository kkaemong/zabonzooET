using UnityEngine;

public class distrubspwaner : MonoBehaviour
{
    [Header("--- [ Spawn Settings ] ---")]
    public GameObject[] obstaclePrefabs; 
    public float minSpawnInterval = 3f; 
    public float maxSpawnInterval = 6f; 

    [Header("--- [ Overlap Check (Coins) ] ---")]
    public LayerMask coinLayer; // 피하고 싶은 코인 레이어
    public Vector2 checkBoxSize = new Vector2(3f, 2f); // 감지 상자 크기 (가로, 세로)

    private float timer = 0f;
    private float currentInterval = 0f;

    void Start()
    {
        SetRandomInterval();
    }

    void Update()
    {
        // 게임이 멈춰있으면 추가 소환도 멈춤
        if (GameManager.IsGamePaused) return;

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
        currentInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
    }

    void SpawnObstacle()
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) return;

        // 생성 위치 결정 (스패너의 위치)
        Vector3 spawnPosition = transform.position;

        // --- [ 코인과의 겹침 방지 체크 ] ---
        // 물리 엔진 동기화 (생성 직후 감지 확률 높임)
        Physics2D.SyncTransforms();

        // 생성 위치에 이미 코인 레이어의 물체가 있는지 확인합니다. (상자 형태 감지)
        Collider2D coinHit = Physics2D.OverlapBox(spawnPosition, checkBoxSize, 0f, coinLayer);
        
        if (coinHit == null) // 코인이 없을 때만 생성
        {
            int randomIndex = Random.Range(0, obstaclePrefabs.Length);
            GameObject selectedPrefab = obstaclePrefabs[randomIndex];
            Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
        }
        else
        {
            Debug.Log($"<color=cyan>[DisturbSpawner]</color> 겹침 감지! ({coinHit.gameObject.name}) 위치: {spawnPosition}. 방해물 생성을 건너뜁니다.");
        }
    }

    // 에디터 씬 뷰에서 감지 범위를 시각적으로 표시
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, checkBoxSize);
    }
}
