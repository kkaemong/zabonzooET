using UnityEngine;

public class disturb : MonoBehaviour
{
    public int damageAmount = 1; 

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 💡 게임 정지 상태(퀴즈 푸는 중)일 때는 방해물에 맞아도 무적 판정!
        if (GameManager.IsGamePaused) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            player playerScript = collision.gameObject.GetComponent<player>();
            
            if (playerScript != null)
            {
                playerScript.TakeDamage(damageAmount);
            }
            
            Destroy(gameObject);
        }
    }
}
