using UnityEngine;

public class disturb : MonoBehaviour
{
    public int damageAmount = 1; 

    private void OnTriggerEnter2D(Collider2D collision)
    {
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
