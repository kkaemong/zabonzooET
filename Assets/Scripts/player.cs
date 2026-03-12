using UnityEngine;

public class player : MonoBehaviour
{
    public float jumpForce = 12f; // 점프 힘 (중력을 높였을 때를 대비해 기본값을 올림)
    public int maxJumps = 2; // 최대 점프 횟수 (2면 더블 점프)
    public float groundCheckDistance = 1.2f; // 바닥에 닿기 얼마나 전부터 착지 애니메이션을 틀지 (수치 조절 가능)
    private Rigidbody2D rb;
    private int jumpCount = 0; // 현재 점프 횟수 기록
    private Animator anim; // 애니메이터 컴포넌트

    void Start()
    {
        // Rigidbody2D 컴포넌트를 가져옵니다. (3D인 경우 Rigidbody로 변경하세요)
        rb = GetComponent<Rigidbody2D>();
        // 파라미터 제어를 위해 Animator 컴포넌트를 가져옵니다.
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // 스페이스바를 눌렀을 때 && 점프 횟수가 최대 횟수보다 적을 때만 점프
        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumps)
        {
            Jump();
        }

        // 공중에 떠있는 상태(점프 횟수가 0보다 클 때)에서 캐릭터가 아래로 떨어질 때
        // 땅에 닿기 "조금 전"에 착지 애니메이션으로 넘어가도록 바닥과의 거리를 검사합니다.
        if (jumpCount > 0 && rb != null && rb.linearVelocity.y < 0f && anim != null)
        {
            // 발밑 방향으로 가상의 레이저(Raycast)를 쏩니다.
            // 주의: 플레이어 중심점에서 구하는 방식이므로, 캐릭터 크기에 따라 groundCheckDistance 값을 조절해야 합니다.
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance);

            // 레이저가 "Ground" 태그가 달린 바닥에 닿았다면 (즉, 바닥이 가까워졌다면)
            if (hit.collider != null && hit.collider.CompareTag("Ground"))
            {
                // 현재 애니메이션 상태가 이미 착지(2)가 아닐 때만 2로 변경합니다.
                if (anim.GetInteger("state") != 2)
                {
                    anim.SetInteger("state", 2);
                }
            }
        }
    }

    void Jump()
    {
        if (rb != null)
        {
            // 점프 속도를 더 빠르고 즉각적으로 만들기 위해 AddForce 대신 속도(velocity)를 직접 변경합니다.
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpCount++; // 점프할 때마다 횟수 증가
            
            // 애니메이터 파라미터 "state"를 1(점프)로 설정합니다.
            if (anim != null)
            {
                anim.SetInteger("state", 1);
            }
        }
    }

    // 오브젝트가 무언가와 충돌했을 때 호출되는 함수
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 충돌한 오브젝트의 태그가 "Ground"일 때 다시 점프 횟수 초기화
        // (주의: 유니티 에디터에서 바닥 오브젝트의 태그를 "Ground"로 설정해주어야 합니다)
        if (collision.gameObject.CompareTag("Ground"))
        {
            jumpCount = 0; // 바닥에 닿으면 점프 횟수를 초기화하여 다시 점프할 수 있게 설정
            
            // 바닥에 닿았을 때(착지 시) 착지 애니메이션을 재생하는 코루틴을 시작합니다.
            if (anim != null)
            {
                StartCoroutine(PlayLandingAnimation());
            }
        }
    }

    // 착지 애니메이션이 너무 빨리 끝나는 것을 방지하기 위해 
    // 일정 시간(예: 0.2초) 동안 유지했다가 기본 상태(대기 또는 달리기 등)로 되돌리는 코루틴
    System.Collections.IEnumerator PlayLandingAnimation()
    {
        anim.SetInteger("state", 2); // 착지 상태로 변환
        
        // n초 동안 대기합니다. (착지 애니메이션 길이에 맞춰 이 숫자를 조절하세요. 예: 0.15f ~ 0.3f)
        yield return new WaitForSeconds(0.2f); 
        
        // 착지 애니메이션이 끝난 후 다시 기본 상태(예: 0)로 되돌립니다.
        // 현재 게임의 "가만히 서있는 상태"나 "달리는 상태"에 맞는 state 번호로 변경해주세요. (예: idle이 0이라면 0으로 둡니다)
        anim.SetInteger("state", 0); 
    }
}
