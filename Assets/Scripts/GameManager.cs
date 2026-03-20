using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    // --- [ 1. 속도 관리 (글로벌 스크롤) ] ---
    public static float globalSpeed = 5f; 
    
    [Header("난이도/속도 설정")]
    public float baseSpeed = 5f;          // 게임 시작 시 기본 속도
    public float maxSpeed = 15f;          // 도달할 수 있는 최대 속도 한계치
    public float accelerationRate = 0.05f; // 초당 증가하는 속도량 (난이도 곡선)
    
    private float currentDifficultySpeed;  // 시간에 따라 서서히 오르는 목표 속도
    public static bool IsGamePaused = false; // 외부(스포너 등)에서 멈춤 상태 확인용
    public static bool isGameOver = false;   // 플레이어 사망 시 영구 정지용
    private Coroutine boostCoroutine;        // 부스트(가속) 코루틴 추적
    private float lastNotifiedSpeed;
    private Color originalSpeedTextColor = Color.white;

    // --- [ 2. 점수 로직 (코인 & 거리) ] ---
    public static int coinCount = 0;
    public Text scoreText; 
    
    public static float distanceTraveled = 0f;
    public Text distanceText;
    
    [Header("Distance Progress UI")]
    private UnityEngine.UI.Slider distanceProgressBar;
    private RectTransform playerIconRect;
    
    
    [Header("Level Up UI")]
    public Text levelUpText; // 퀴즈 정답 시 띄울 레벨업 텍스트
    public Text speedText;   // 현재 체감 속도 표시 텍스트

    [Header("거리 기반 속보 퀴즈 설정")]
    public float quizDistanceInterval = 333f; // 1000m 기준 딱 2번(333m, 666m) 팝업
    private float nextQuizDistance;           // 다음 퀴즈가 나올 목표 거리
    private int quizCount = 0;                // 퀴즈 등장 횟수 추적

    void Start()
    {
        IsGamePaused = false; // 💡 씬 재시작 시 정지 상태 초기화 (매우 중요)
        isGameOver = false;

        coinCount = 0; 
        distanceTraveled = 0f;
        currentDifficultySpeed = baseSpeed;
        globalSpeed = baseSpeed;
        
        quizCount = 0;
        nextQuizDistance = quizDistanceInterval; // 첫 퀴즈 목표(333m) 설정

        if (speedText == null)
        {
            GameObject stObj = GameObject.Find("SpeedText") ?? GameObject.Find("DistanceText (1)");
            if (stObj != null)
            {
                stObj.name = "SpeedText";
                speedText = stObj.GetComponent<Text>();
                speedText.horizontalOverflow = HorizontalWrapMode.Overflow;
                speedText.verticalOverflow = VerticalWrapMode.Overflow;
                speedText.raycastTarget = false;
                speedText.fontStyle = FontStyle.Bold; // 글씨 굵게
                originalSpeedTextColor = speedText.color;
                
                // 화면 왼쪽 상단 하트(Heart) UI 아래쪽에 안전하게 고정
                RectTransform rect = stObj.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0, 1);
                    rect.anchorMax = new Vector2(0, 1);
                    rect.pivot = new Vector2(0, 1);
                    // x는 50으로 살짝 띄우고, y는 -180으로 하트보다 좀 더 아래로 내려줍니다.
                    rect.anchoredPosition = new Vector2(50f, -180f); 
                    speedText.alignment = TextAnchor.UpperLeft;
                }
            }
        }

        UpdateScoreUI();
        UpdateDistanceUI();
        UpdateSpeedUI();

        // 💡 자동 생성된 달리기 게이지 바(Distance ProgressBar) 연결
        GameObject barObj = GameObject.Find("DistanceProgressBarUI");
        if (barObj != null) distanceProgressBar = barObj.GetComponent<UnityEngine.UI.Slider>();
        
        GameObject pIcon = GameObject.Find("PlayerHandle");
        if (pIcon != null) playerIconRect = pIcon.GetComponent<RectTransform>();
    }

    void Update()
    {
        if (isGameOver)
        {
            globalSpeed = 0f;
            return;
        }

        // 게임이 멈춰있지 않고, 부스트 스킬 사용 중이 아닐 때만 난이도(속도)를 점진적으로 올림
        if (!IsGamePaused && boostCoroutine == null)
        {
            if (currentDifficultySpeed < maxSpeed)
            {
                currentDifficultySpeed += accelerationRate * Time.deltaTime;
            }
            
            // 현재 속도를 서서히 올라가는 난이도 속도에 맞춥니다.
            globalSpeed = currentDifficultySpeed;
        }

        // 게임이 진행 중일 때만 거리가 늘어남
        if (!IsGamePaused && globalSpeed > 0)
        {
            distanceTraveled += globalSpeed * Time.deltaTime;

            // 목적지 1000m에 도달하면 게임 클리어 처리 (끝남)
            if (distanceTraveled >= 1000f)
            {
                distanceTraveled = 1000f;
                isGameOver = true;
                globalSpeed = 0f;
                Debug.Log("<color=green>[Game Clear]</color> 1000m 돌파! 게임 종료!");
            }

            // 💡 프로그레스 바(게이지 바) 및 플레이어 아이콘 UI 실시간 업데이트
            if (distanceProgressBar != null)
            {
                float progress = Mathf.Clamp01(distanceTraveled / 1000f);
                distanceProgressBar.value = progress;
                
                if (playerIconRect != null)
                {
                    playerIconRect.anchorMin = new Vector2(progress, 0.5f);
                    playerIconRect.anchorMax = new Vector2(progress, 0.5f);
                    playerIconRect.anchoredPosition = Vector2.zero;
                }
            }

            UpdateDistanceUI();
            UpdateSpeedUI();

            // 💡 원래 기획대로 1000m 완주 중 딱 2번만 나오도록 제한 (quizCount < 2)
            if (quizCount < 2 && distanceTraveled >= nextQuizDistance)
            {
                TriggerBreakingNews();
            }
        }
    }

    private void TriggerBreakingNews()
    {
        quizCount++; // 퀴즈 발생 횟수 증가
        // 다음 퀴즈 돌파 거리 지정 (예: 666m)
        nextQuizDistance += quizDistanceInterval; 

        // 💡 기존처럼 퀴즈 팝업을 바로 띄우지 않고, 화면 밖에서 군인을 하나 스폰합니다.
        // 플레이어가 이 군인과 부딪히면 퀴즈가 시작됩니다!
        SoldierSpawner spawner = FindObjectOfType<SoldierSpawner>();
        if (spawner != null)
        {
            spawner.SpawnQuizSoldier();
        }
        else
        {
            // 혹시 씬에 SoldierSpawner가 없다면 예외 대비책으로 기존 방식(바로 띄우기) 동작
            if (QuizManager.Instance != null)
            {
                CameraShake.Shake(0.5f, 0.2f);
                player p = FindObjectOfType<player>();
                if (p != null)
                {
                    Animator pAnim = p.GetComponent<Animator>();
                    if (pAnim != null) pAnim.speed = 0f;
                }
                QuizManager.Instance.ShowQuiz();
            }
        }
    }

    public void AddCoin(int amount)
    {
        coinCount += amount;
        UpdateScoreUI();
    }

    public void AddLife(int amount)
    {
        player p = FindObjectOfType<player>();
        if (p != null) p.AddLife(amount);
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = coinCount.ToString();
        }
    }

    void UpdateDistanceUI()
    {
        if (distanceText != null)
        {
            // 거리(m) 단위로 정수형태 출력
            distanceText.text = Mathf.FloorToInt(distanceTraveled).ToString() + "m";
        }
    }

    void UpdateSpeedUI()
    {
        if (speedText != null)
        {
            speedText.text = "속도: " + globalSpeed.ToString("F1") + " km/h";
        }
    }

    // --- [ 3. 퀴즈 파트 속도 제어 함수 ] ---
    
    private GameObject heartUI; // 하트 UI 전체 (숨기기 용)
    private GameObject scoreUI; // 점수 UI 전체 (숨기기 용)

    void Awake()
    {
        heartUI = GameObject.Find("heart");
        scoreUI = GameObject.Find("Score");
    }

    // 모달 창이 떴을 때 게임 정지
    public void PauseGame()
    {
        IsGamePaused = true;
        globalSpeed = 0f;

        // 💡 퀴즈 중 방해되는 수치들 숨기기
        if (heartUI != null) heartUI.SetActive(false);
        if (scoreUI != null) scoreUI.SetActive(false);
        if (distanceText != null) distanceText.gameObject.SetActive(false);
        if (speedText != null) speedText.gameObject.SetActive(false);
    }

    // 모달 창이 꺼졌을 때 기본 속도로 복귀
    public void ResumeGame()
    {
        IsGamePaused = false;
        globalSpeed = currentDifficultySpeed; // 멈추기 직전의 난이도 속도로 복귀
        
        // 💡 수치들 다시 켜기
        if (heartUI != null) heartUI.SetActive(true);
        if (scoreUI != null) scoreUI.SetActive(true);
        if (distanceText != null) distanceText.gameObject.SetActive(true);
        if (speedText != null) speedText.gameObject.SetActive(true);

        // 멈춰있던 플레이어 애니메이션 복구
        player p = FindObjectOfType<player>();
        if (p != null)
        {
            Animator pAnim = p.GetComponent<Animator>();
            if (pAnim != null) pAnim.speed = 1f;
        }
    }

    // 정답을 맞췄을 때 3초간 속도 부스트!
    public void ApplyTemporarySpeedBoost(float multiplier = 2f, float duration = 3f)
    {
        if (boostCoroutine != null) StopCoroutine(boostCoroutine);
        boostCoroutine = StartCoroutine(SpeedBoostCoroutine(multiplier, duration));
    }

    IEnumerator SpeedBoostCoroutine(float multiplier, float duration)
    {
        IsGamePaused = false; // 혹시 멈춰있었다면 풀기
        
        // 멈춰있던 플레이어 애니메이션 복구
        player p = FindObjectOfType<player>();
        if (p != null)
        {
            Animator pAnim = p.GetComponent<Animator>();
            if (pAnim != null) pAnim.speed = 1f;
        }
        
        // Level UP! 텍스트 애니메이션 시작
        if (levelUpText != null)
        {
            StartCoroutine(ShowLevelUpText());
        }

        // 목표 난이도 속도보다 n배 빠르게 달림
        globalSpeed = currentDifficultySpeed * multiplier;
        
        // duration(예: 3초) 만큼 대기
        yield return new WaitForSeconds(duration);

        // 시간이 끝나면 원래 난이도로 복귀
        globalSpeed = currentDifficultySpeed;
        boostCoroutine = null;
    }

    IEnumerator ShowLevelUpText()
    {
        if (levelUpText == null) yield break;

        RectTransform rect = levelUpText.GetComponent<RectTransform>();
        Vector2 startPos = new Vector2(0, 50); // 시작 위치
        Vector2 endPos = new Vector2(0, 250);  // 끝 위치 (위로 떠오름)
        rect.anchoredPosition = startPos;

        Color startColor = levelUpText.color;
        startColor.a = 0f;
        levelUpText.color = startColor;

        float fadeDuration = 0.5f;
        float stayDuration = 1.0f;
        float fadeOutDuration = 0.5f;

        // 1. Fade In & Move Up
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            startColor.a = Mathf.Lerp(0f, 1f, t);
            levelUpText.color = startColor;
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        // 2. Stay
        yield return new WaitForSeconds(stayDuration);

        // 3. Fade Out & Move further up
        elapsed = 0f;
        Vector2 currentPos = rect.anchoredPosition;
        Vector2 finalPos = currentPos + new Vector2(0, 100);
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;
            startColor.a = Mathf.Lerp(1f, 0f, t);
            levelUpText.color = startColor;
            rect.anchoredPosition = Vector2.Lerp(currentPos, finalPos, t);
            yield return null;
        }
    }
}