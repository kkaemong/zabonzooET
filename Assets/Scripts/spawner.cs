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
    

    private float timer = 0f;
    private float currentInterval = 0f;

    void Start()
    {
        SetRandomInterval();
    }

    void Update()
    {
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

        int randomIndex = Random.Range(0, obstaclePrefabs.Length);
        GameObject selectedPrefab = obstaclePrefabs[randomIndex];

        Vector3 spawnPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);

        // --- [ 중복 생성 방지 체크 ] ---
        // 지정된 범위(checkBoxSize) 안에 targetLayer에 해당하는 콜라이더가 있는지 확인합니다.
        Collider2D hit = Physics2D.OverlapBox(spawnPosition, checkBoxSize, 0f, targetLayer);
        
        if (hit != null)
        {
            // 이미 무언가 있다면 생성을 하지 않고 종료합니다.
            // Debug.Log("Overlap detected! Skipping spawn.");
            return;
        }

        Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
    }
}
