using UnityEngine;

public class SoldierSpawner : MonoBehaviour
{
    [Header("--- [ Spawn Settings ] ---")]
    public GameObject[] soldierPrefabs; // 퀴즈를 내는 몬스터(soldier) 프리팹들
    public float minSpawnInterval = 10f; // 몬스터 등장 최소 간격 (좀 길게)
    public float maxSpawnInterval = 20f; // 몬스터 등장 최대 간격

    [Header("--- [ Overlap Check ] ---")]
    public LayerMask avoidLayer; // 피하고 싶은 레이어 (장애물, 코인 등)
    public Vector2 checkBoxSize = new Vector2(3f, 4f); // 감지 상자 크기 (가로, 세로)

    void Start()
    {
        // 이제 타이머로 소환하지 않고 GameManager가 특정 거리에 도달했을 때만 1마리씩 소환합니다.
    }

    // GameManager에서 거리(333m, 666m)를 채웠을 때 콜하는 퀴즈 캐릭터 소환 함수
    public void SpawnQuizSoldier()
    {
        if (soldierPrefabs == null || soldierPrefabs.Length == 0) return;

        // 생성 위치 결정 (스패너의 위치)
        Vector3 spawnPosition = transform.position;

        // 물리 엔진 동기화 (생성 직후 감지 확률 높임)
        Physics2D.SyncTransforms();

        // 폭넓게 감지하여 겹침을 철저하게 방지합니다
        Collider2D[] hits = Physics2D.OverlapBoxAll(spawnPosition, checkBoxSize, 0f);
        bool isOverlap = false;
        foreach (var h in hits)
        {
            if (h.gameObject == this.gameObject) continue;
            string objName = h.gameObject.name.ToLower();
            if (objName.Contains("disturb") || objName.Contains("obstacle") || objName.Contains("coin") || objName.Contains("soldier"))
            {
                isOverlap = true;
                break;
            }
        }
        
        if (!isOverlap) // 아무것도 겹치지 않을 때만 생성
        {
            int randomIndex = Random.Range(0, soldierPrefabs.Length);
            GameObject selectedPrefab = soldierPrefabs[randomIndex];
            
            if (selectedPrefab != null)
            {
                Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
            }
        }
    }

    // 에디터 씬 뷰에서 감지 범위를 시각적으로 표시
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(transform.position, checkBoxSize);
    }
}
