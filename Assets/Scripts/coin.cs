using UnityEngine;

public class coin : MonoBehaviour
{
    // 코인 전용 스크립트 (현재는 설정할 변수가 없습니다)

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}
