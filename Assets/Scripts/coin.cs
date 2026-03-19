using UnityEngine;

public class coin : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 💡 게임 정지 상태(퀴즈 푸는 중)일 때는 코인도 안 먹어지게 처리!
        if (GameManager.IsGamePaused) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            // GameManager에서 점수를 1점 증가시킵니다.
            GameManager gm = FindObjectOfType<GameManager>();
            if (gm != null)
            {
                gm.AddCoin(1);
            }
            
            // 코인은 바로 사라집니다.
            Destroy(gameObject);
        }
    }
}
