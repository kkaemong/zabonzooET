using UnityEngine;
using System.Collections;

public class player : MonoBehaviour
{
    public float firstJumpForce = 14f; // 1단 점프력 (높게)
    public float secondJumpForce = 10f; // 2단 점프력 (낮게)
    public int maxJumps = 2;
    public float groundCheckDistance = 1.2f; 
    public int health = 3; 

    private Rigidbody2D rb;
    private int jumpCount = 0; 
    private Animator anim; 
    private SpriteRenderer spriteRenderer; 
    private bool isInvincible = false; // 무적 상태 확인용 플래그
    private Coroutine landingCoroutine; // 💡 착지 코루틴 추적용 변수 추가

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 캐릭터가 물리 충돌로 인해 회전(꼬꾸라짐)하지 않도록 고정
        if (rb != null)
        {
            rb.freezeRotation = true;
        }
    }

    void Update()
    {
        // 💡 3번째 누르면 점프는 안 하지만, 카운트는 올라가야 "이미 다 뛰었다"라는 정보를 확실하게 처리할 수 있습니다.
        // maxJumps를 넘어가는 입력이 와도 카운트만 늘리고, 실제 점프 실행(Jump)은 안 하도록 변경
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (jumpCount < maxJumps)
            {
                Jump();
            }
            else
            {
                jumpCount++; // 점프 이펙트는 없지만 카운트를 올려 입력이 방금 들어왔음을 기록
            }
        }

        if (jumpCount > 0 && rb != null && rb.linearVelocity.y < 0f && anim != null)
        {
            // 💡 단일 Raycast가 아닌, 광선을 쏘아 닿는 '모든' 물체를 검사하도록 변경 (동전에 광선이 막히는 현상 방지)
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, Vector2.down, groundCheckDistance);

            foreach (var hit in hits)
            {
                if (hit.collider != null && hit.collider.CompareTag("Ground"))
                {
                    if (anim.GetInteger("state") != 2)
                    {
                        anim.SetInteger("state", 2);
                    }
                    break; // 바닥을 찾았으므로 더 이상 검사할 필요 없음
                }
            }
        }
    }

    void Jump()
    {
        if (rb != null)
        {
            // 점프 횟수에 따라 점프력을 다르게 적용 (1단 점프는 높게, 2단 점프는 낮게)
            float force = (jumpCount == 0) ? firstJumpForce : secondJumpForce;
            
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
            jumpCount++; 
            
            if (anim != null)
            {
                // 💡 만약 점프를 했는데 아직 착지 코루틴이 돌고 있다면 꺼버림
                if (landingCoroutine != null)
                {
                    StopCoroutine(landingCoroutine);
                    landingCoroutine = null;
                }

                if (jumpCount == 1)
                {
                    anim.SetInteger("state", 1);
                }
                else if (jumpCount == 2)
                {
                    anim.SetInteger("state", 3);
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            jumpCount = 0; 
            
            if (anim != null)
            {
                // 💡 이전 코루틴이 돌고 있으면 종료하고 새 코루틴 시작
                if (landingCoroutine != null) StopCoroutine(landingCoroutine);
                landingCoroutine = StartCoroutine(PlayLandingAnimation());
            }
        }
    }

    IEnumerator PlayLandingAnimation()
    {
        anim.SetInteger("state", 2); 
        yield return new WaitForSeconds(0.2f); 
        
        // 💡 0.2초 뒤에 캐릭터가 여전히 바닥에 있을 때만 달리기(0)로 바꿉니다.
        // 그 사이에 점프를 눌러서 jumpCount가 올라갔다면 0으로 바꾸지 않습니다!
        if (jumpCount == 0)
        {
            anim.SetInteger("state", 0); 
        }
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return; // 무적 상태라면 데미지를 받지 않음

        health -= damage; 
        Debug.Log("Health: " + health);

        if (health <= 0)
        {
            Debug.Log("Game Over!");
        }

        if (spriteRenderer != null)
        {
            StartCoroutine(OnHitEffect());
        }
    }

    IEnumerator OnHitEffect()
    {
        isInvincible = true; // 무적 시작
        
        // 빨간색이고 반투명한 상태로 변경
        spriteRenderer.color = new Color(1f, 0.2f, 0.2f, 0.5f);
        
        yield return new WaitForSeconds(1.5f); // 1.5초 동안 지속
        
        // 원래 색상(흰색, 불투명)으로 복구
        spriteRenderer.color = Color.white;
        
        isInvincible = false; // 무적 종료
    }
}