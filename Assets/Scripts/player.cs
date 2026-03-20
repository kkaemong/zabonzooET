using UnityEngine;
using UnityEngine.UI; 
using System.Collections;

public class player : MonoBehaviour
{
    public float firstJumpForce = 14f;
    public float secondJumpForce = 10f;
    public int maxJumps = 2;
    public float groundCheckDistance = 1.2f; 
    public int health = 3; 

    [Header("Health UI")]
    public Image[] hearts; 
    public Sprite fullHeart;
    public Sprite emptyHeart; 
    [Header("Effects")]
    public GameObject hitEffectPrefab;
    public GameObject shieldEffectPrefab;
    private GameObject activeShield; // 쉴드 유지용

    private Rigidbody2D rb;
    private int jumpCount = 0; 
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private bool isInvincible = false;
    private Coroutine landingCoroutine;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>(); // 깜빡임 연출용

        // 💡 플레이어의 렌더링 순서를 5로 강제 고정하여 배경(0)보다 무조건 앞에 나오도록 설정
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 5;
        }

        if (rb != null)
        {
            rb.freezeRotation = true;
        }

        UpdateHealthUI();
    }

    void Update()
    {
        if (isDead) return;

        // 💡 게임 정지 중(퀴즈 등)일 때는 점프 입력 무시
        if (Input.GetKeyDown(KeyCode.Space) && !GameManager.IsGamePaused)
        {
            if (jumpCount < maxJumps)
            {
                Jump();
            }
            else
            {
                jumpCount++;
            }
        }

        if (jumpCount > 0 && rb != null && rb.linearVelocity.y < 0f && anim != null)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, Vector2.down, groundCheckDistance);

            foreach (var hit in hits)
            {
                if (hit.collider != null && hit.collider.CompareTag("Ground"))
                {
                    if (anim.GetInteger("state") != 2)
                    {
                        anim.SetInteger("state", 2);
                    }
                    break;
                }
            }
        }
    }

    void Jump()
    {
        if (isDead) return;

        if (rb != null)
        {
            float force = (jumpCount == 0) ? firstJumpForce : secondJumpForce;

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
            jumpCount++;

            if (anim != null)
            {
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
        if (isDead) return;

        if (collision.gameObject.CompareTag("Ground"))
        {
            jumpCount = 0;

            if (anim != null)
            {
                if (landingCoroutine != null) StopCoroutine(landingCoroutine);
                landingCoroutine = StartCoroutine(PlayLandingAnimation());
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;


    }

    IEnumerator PlayLandingAnimation()
    {
        anim.SetInteger("state", 2);
        yield return new WaitForSeconds(0.2f);

        if (jumpCount == 0)
        {
            anim.SetInteger("state", 0);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead || isInvincible) return; 

        health -= damage;
        Debug.Log("Health: " + health);

        // 💡 피격 시 카메라 흔들림 연출 (쿠구궁)
        CameraShake.Shake(0.3f, 0.4f);

        UpdateHealthUI();

        if (health <= 0)
        {
            Die();
        }
        else
        {
            // 💡 피격 이펙트 생성
            if (hitEffectPrefab != null)
            {
                // 방해물(disturb) 등 피격 시 이펙트를 더 앞쪽으로 (1.2 -> 1.8)
                Vector3 hitOffset = new Vector3(1.8f, 0f, 0f);

                GameObject hit = Instantiate(hitEffectPrefab, transform.position + hitOffset, Quaternion.identity);
                
                // 크기를 조금 더 줄임 (0.5 -> 0.35)
                hit.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);

                // 💡 투명도 낮추기 (반투명하게)
                SpriteRenderer[] hitSrs = hit.GetComponentsInChildren<SpriteRenderer>();
                foreach (var sr in hitSrs) {
                    Color c = sr.color; c.a = 0.6f; sr.color = c;
                }
                ParticleSystem[] hitPss = hit.GetComponentsInChildren<ParticleSystem>();
                foreach (var ps in hitPss) {
                    var main = ps.main;
                    main.startColor = new Color(1f, 1f, 1f, 0.6f);
                }
                
                // 💡 반복재생 방지: 0.6초 뒤에 강제로 이펙트 삭제
                Destroy(hit, 0.6f);
            }

            if (spriteRenderer != null)
            {
                StartCoroutine(OnHitEffect());
            }
        }
    }

    public void AddLife(int amount)
    {
        if (isDead) return;
        health += amount;
        if (health > hearts.Length) health = hearts.Length; // 최대 체력 초과 방지
        UpdateHealthUI();
    }

    public void TriggerQuizInvincibility(float duration)
    {
        StartCoroutine(InvincibilityCoroutine(duration));
        
        // 💡 쉴드 이펙트 생성 (자식 오브젝트로 생성하여 따라다니게 적용)
        if (shieldEffectPrefab != null && activeShield == null)
        {
            activeShield = Instantiate(shieldEffectPrefab, transform.position, Quaternion.identity, transform);
            
            // 쉴드 크기 더 작게 줄임 (0.5 -> 0.4)
            activeShield.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

            // 💡 쉴드는 4번으로 강제 고정! (배경 0번보다 앞에, 플레이어 5번보다 뒤에 위치하게 함)
            SpriteRenderer[] shieldSrs = activeShield.GetComponentsInChildren<SpriteRenderer>();
            foreach (var sr in shieldSrs) {
                sr.sortingLayerID = spriteRenderer.sortingLayerID; 
                sr.sortingOrder = 4; // 플레이어(5)보다 1칸 뒤
                Color c = sr.color; c.a = 0.6f; sr.color = c; // 반투명
            }
            ParticleSystemRenderer[] shieldPsrs = activeShield.GetComponentsInChildren<ParticleSystemRenderer>();
            foreach (var psr in shieldPsrs) {
                psr.sortingLayerID = spriteRenderer.sortingLayerID;
                psr.sortingOrder = 4;
            }
            ParticleSystem[] shieldPss = activeShield.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in shieldPss) {
                var main = ps.main;
                main.startColor = new Color(1f, 1f, 1f, 0.6f); // 반투명
            }

            Destroy(activeShield, duration); // duration 시간 후 자동 삭제
        }
    }

    IEnumerator InvincibilityCoroutine(float duration)
    {
        isInvincible = true;
        float blinkInterval = 0.15f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (isDead) break;
            spriteRenderer.color = new Color(1f, 1f, 1f, 0.3f);
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;

            if (isDead) break;
            spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }
        if (!isDead) spriteRenderer.color = Color.white;
        isInvincible = false;
    }

    void Die()
    {
        isDead = true;
        Debug.Log("Game Over!");

        // 1. 방금 설정한 상태 번호 4번으로 강제 전환
        if (anim != null)
        {
            anim.SetInteger("state", 4);
        }

        // 2. 물리 운동 제거 (위로 튀지 않고 멈춤)
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // 3. 글로벌 스크롤 속도를 0으로 만들어 배경, 코인, 장애물의 이동 멈춤
        GameManager.globalSpeed = 0f;
        GameManager.isGameOver = true; // 게임 오버 선언 (속도 증가 영구 중단)

        // 💡 4. 화면 크기가 멈췄음에도 스포너가 계속 작동해 물체가 겹치는 현상 방지
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.ToLower().Contains("spawn"))
            {
                obj.SetActive(false); // 이름에 'spawn'이 들어간 스포너들(coin, building, obstacle)을 모두 끕니다
            }
        }
    }

    void UpdateHealthUI()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null) continue;
            if (i < health)
            {
                hearts[i].sprite = fullHeart;
            }
            else
            {
                hearts[i].sprite = emptyHeart;
            }
        }
    }

    IEnumerator OnHitEffect()
    {
        isInvincible = true;
        
        float blinkDuration = 1.5f; // 총 깜빡이는 시간
        float blinkInterval = 0.15f; // 한 번 깜빡이는(켜고 끄기) 주기
        float elapsed = 0f;

        // 깜빡깜빡 (반투명 <-> 원래색)
        while (elapsed < blinkDuration)
        {
            if (isDead) break; // 죽으면 깜빡임 중지

            // 반투명하게 (알파값 0.3)
            spriteRenderer.color = new Color(1f, 1f, 1f, 0.3f);
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;

            if (isDead) break;

            // 다시 원래대로 (알파값 1.0)
            spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        // 혹시 투명 상태로 끝났을 수 있으니 원상복구
        if(!isDead) spriteRenderer.color = Color.white;
        
        isInvincible = false;
    }
}