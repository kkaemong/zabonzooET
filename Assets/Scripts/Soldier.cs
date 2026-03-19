using UnityEngine;

public class Soldier : MonoBehaviour
{
    [Header("--- [ Soldier Settings ] ---")]
    public float sprintSpeed = 5f; // 플레이어를 향해 달려오는 추가 속도
    public bool faceLeft = true; 
    public float stopDistance = 3f; // 플레이어 앞에서 멈출 거리

    private SpriteRenderer spriteRenderer;
    private Animator anim;
    private Transform playerTransform;
    private bool hasStopped = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        // 왼쪽을 바라봐야 한다면 (스프라이트 기본 방향에 따라 flipX 값을 반대로 줘야 할 수 있음)
        if (spriteRenderer != null)
        {
            // 만약 원본 이미지가 '오른쪽'을 보고 있다면, 왼쪽을 보게 하려면 flipX를 true로 해야 합니다.
            // 반대로 사용자가 보기에 거꾸로 나온다면 이 bool 값을 반전시키면 됩니다.
            spriteRenderer.flipX = !faceLeft; 
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }

        // 스폰 시 달리기 애니메이션 실행 (애니메이터 파라미터가 있다면)
        // anim.SetBool("isRunning", true); // 필요 시 활성화
    }

    void Update()
    {
        if (playerTransform == null) return;

        // 플레이어와의 가로(X축) 거리 계산
        float distanceToPlayer = transform.position.x - playerTransform.position.x;

        if (!hasStopped && distanceToPlayer > stopDistance)
        {
            // 플레이어를 향해 뛰어옵니다 (배경 스크롤 속도 + 군인 달리기 속도)
            float totalSpeed = GameManager.globalSpeed + sprintSpeed;
            transform.Translate(Vector3.left * totalSpeed * Time.deltaTime);
            
            // 💡 이동 속도에 맞춰 애니메이션 속도(다리 움직임)를 비례해서 빠르게 만듭니다. (미끄러짐 방지)
            if (anim != null) anim.speed = totalSpeed / 5f; 
        }
        else if (!hasStopped && distanceToPlayer <= stopDistance)
        {
            hasStopped = true;
            
            // 💡 플레이어가 점프를 하든 말든 거리가 맞춰지면 무조건 퀴즈 강제 실행!
            if (QuizManager.Instance != null && !GameManager.isGameOver)
            {
                Animator playerAnim = playerTransform.GetComponent<Animator>();
                if (playerAnim != null) playerAnim.speed = 0f;

                QuizManager.Instance.ShowQuiz();
            }
            
            // 할 일을 다 한 군인은 화면에서 즉시 사라집니다.
            Destroy(gameObject);
        }
    }
}
