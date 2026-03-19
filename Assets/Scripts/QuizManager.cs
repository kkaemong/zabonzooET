using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class QuizData
{
    public string question;
    public string answerA;
    public string answerB;
    public string answerC;
    public string answerD;
    public int correctAnswerIndex; // 0:A, 1:B, 2:C, 3:D
    public string explanation;     // 💡 정답 해설 텍스트
}

public class QuizManager : MonoBehaviour
{
    public static QuizManager Instance { get; private set; }

    [Header("UI Reference")]
    public GameObject quizPanel; 
    
    [Header("Any Text / TextMeshPro / Button GameObject allowed")]
    public GameObject questionTextObj;
    public GameObject btnTextAObj;
    public GameObject btnTextBObj;
    public GameObject btnTextCObj;
    public GameObject btnTextDObj;
    public GameObject buttonAObj;
    public GameObject buttonBObj;
    public GameObject buttonCObj;
    public GameObject buttonDObj;

    [Header("Boost Setting")]
    public float boostMultiplier = 2f;
    public float boostDuration = 3f;

    private List<QuizData> quizDatabase;
    private QuizData currentQuiz;

    private GameManager gm;
    private GameObject buttonGroupObj; // 버튼들 숨기기 용도

    void Awake()
    {
        // 씬 재시작 시에도 현재 씬의 QuizManager가 무조건 Instance가 되도록 덮어쓰기 (파괴 방지)
        Instance = this;
        InitializeQuizData(); 
    }

    void Start()
    {
        gm = FindObjectOfType<GameManager>();

        // 만약 UI가 연결되어 있지 않다면 강제로 직접 찾아오기 (방어 로직)
        if (quizPanel == null)
        {
            Transform panelT = GameObject.Find("ScoreCanvas")?.transform.Find("QuizPanel");
            if (panelT != null) quizPanel = panelT.gameObject;
        }
        
        // 확실하게 시작할 때 퀴즈 패널 끄기
        if (quizPanel != null)
        {
            quizPanel.SetActive(false);
        }

        if (quizPanel != null)
        {
            if (questionTextObj == null) questionTextObj = quizPanel.transform.Find("QuestionText")?.gameObject;
            
            Transform btnGroup = quizPanel.transform.Find("ButtonGroup");
            if (btnGroup != null)
            {
                if (buttonAObj == null) buttonAObj = btnGroup.Find("ButtonA")?.gameObject;
                if (buttonBObj == null) buttonBObj = btnGroup.Find("ButtonB")?.gameObject;
                if (buttonCObj == null) buttonCObj = btnGroup.Find("ButtonC")?.gameObject;
                if (buttonDObj == null) buttonDObj = btnGroup.Find("ButtonD")?.gameObject;
                
                if (btnTextAObj == null && buttonAObj != null) btnTextAObj = buttonAObj.transform.Find("Text")?.gameObject;
                if (btnTextBObj == null && buttonBObj != null) btnTextBObj = buttonBObj.transform.Find("Text")?.gameObject;
                if (btnTextCObj == null && buttonCObj != null) btnTextCObj = buttonCObj.transform.Find("Text")?.gameObject;
                if (btnTextDObj == null && buttonDObj != null) btnTextDObj = buttonDObj.transform.Find("Text")?.gameObject;
            }
        }
        
        // 클릭 리스너 연결은 ShowQuiz()에서 동적으로 매번 재확인하도록 변경하여
        // 플레이 도중에 마법사를 돌려 껍데기가 바뀌어도 추적할 수 있도록 했습니다.
    }

    void InitializeQuizData()
    {
        if (quizDatabase != null) return;
        
        quizDatabase = new List<QuizData>
        {
            new QuizData { question = "물가가 지속적으로 오르는 현상은?", answerA = "디플레이션", answerB = "스테그플레이션", answerC = "인플레이션", answerD = "리플레이션", correctAnswerIndex = 2,
                explanation = "인플레이션(Inflation)은 화폐가치가 하락하여 일반 물가 수준이 지속적으로 오르는 현상을 말합니다." },
            new QuizData { question = "은행에 돈을 빌릴 때 내야 하는 돈은?", answerA = "세금", answerB = "이자", answerC = "배당금", answerD = "보험금", correctAnswerIndex = 1,
                explanation = "돈을 빌려 쓴 대가로 은행(채권자)에게 지급해야 하는 일정한 비율의 돈을 이자(Interest)라고 부릅니다." },
            new QuizData { question = "회사 수익의 일부를 주주들에게 나눠주는 것은?", answerA = "배당금", answerB = "보험금", answerC = "세금", answerD = "원금", correctAnswerIndex = 0,
                explanation = "주식회사가 이익을 내면, 투자한 주주들에게 지분에 따라 나누어 주는 돈을 배당금(Dividend)이라고 합니다." },
            new QuizData { question = "수요가 공급보다 많으면 가격은 어떻게 될까?", answerA = "오른다", answerB = "내린다", answerC = "변함없다", answerD = "사라진다", correctAnswerIndex = 0,
                explanation = "사려는 사람(수요)이 파는 물건(공급)보다 많으면 서로 사려고 경쟁하기 때문에 가격은 자연스럽게 오릅니다." },
            new QuizData { question = "수입보다 지출이 많은 상태를 뜻하는 단어는?", answerA = "흑자", answerB = "적자", answerC = "동결", answerD = "파산", correctAnswerIndex = 1,
                explanation = "벌어들인 돈(수입)보다 쓴 돈(지출)이 더 많아 손실이 난 상태를 적자(Deficit)라고 합니다. 반대는 흑자입니다." }
        };
    }

    public void ShowQuiz()
    {
        InitializeQuizData(); // 만약을 대비해 한번 더 확인

        if (gm == null) gm = FindObjectOfType<GameManager>();
        if (gm != null) gm.PauseGame(); 

        if (quizDatabase == null || quizDatabase.Count == 0) return;

        // 랜덤 문제 출제
        int r = Random.Range(0, quizDatabase.Count);
        currentQuiz = quizDatabase[r];

        SetText(questionTextObj, currentQuiz.question);
        SetText(btnTextAObj, currentQuiz.answerA);
        SetText(btnTextBObj, currentQuiz.answerB);
        SetText(btnTextCObj, currentQuiz.answerC);
        SetText(btnTextDObj, currentQuiz.answerD);

        // 동적으로 리스너 갱신
        AddButtonListener(buttonAObj, () => OnAnswerSelected(0));
        AddButtonListener(buttonBObj, () => OnAnswerSelected(1));
        AddButtonListener(buttonCObj, () => OnAnswerSelected(2));
        AddButtonListener(buttonDObj, () => OnAnswerSelected(3));

        // 버튼 그룹 객체 캐싱
        if (buttonGroupObj == null && quizPanel != null)
        {
            Transform bg = quizPanel.transform.Find("ButtonGroup");
            if (bg != null) buttonGroupObj = bg.gameObject;
        }

        if (buttonGroupObj != null) buttonGroupObj.SetActive(true); // 새 문제 시 버튼 다시 표시

        if (quizPanel != null) quizPanel.SetActive(true);
        else Debug.LogError("QuizManager에 quizPanel이 연결되지 않았습니다.");
    }

    void SetText(GameObject obj, string textData)
    {
        if (obj == null) return;
        Text t1 = obj.GetComponent<Text>();
        if (t1 != null) { t1.text = textData; return; }
        
        // TextMeshPro 호환 추가
        var t2 = obj.GetComponent<TMPro.TextMeshProUGUI>();
        if (t2 != null) { t2.text = textData; return; }
    }

    void AddButtonListener(GameObject obj, UnityEngine.Events.UnityAction action)
    {
        if (obj == null) return;
        Button b = obj.GetComponent<Button>();
        if (b != null)
        {
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(action);
        }
    }

    void OnAnswerSelected(int selectedIndex)
    {
        // 중복 클릭 방지
        if (buttonGroupObj != null) buttonGroupObj.SetActive(false);
        bool isCorrect = (selectedIndex == currentQuiz.correctAnswerIndex);

        StartCoroutine(ShowExplanationCoroutine(isCorrect));
    }

    System.Collections.IEnumerator ShowExplanationCoroutine(bool isCorrect)
    {
        // 💡 1. 텍스트를 해설 모드로 변경
        string resultWord = isCorrect ? "<color=#00FF00>정답입니다!</color>" : "<color=#FF0000>오답입니다!</color>";
        SetText(questionTextObj, $"{resultWord}\n\n<size=50>{currentQuiz.explanation}</size>");

        // 💡 2. 3초 대기 (유저가 해설을 읽을 시간)
        // 참고: GameManager에서 PauseGame시 Time.timeScale=0 을 안 쓰므로 WaitForSeconds 정상 작동
        yield return new WaitForSecondsRealtime(3f);

        // 💡 3. 보상/패널티 및 게임 재개 로직
        if (isCorrect)
        {
            if (gm != null)
            {
                gm.AddCoin(5);
                gm.AddLife(1); // 💡 목숨 1개 추가
                gm.ResumeGame();
            }
            
            // 💡 정답 시 3초 무적 효과 부여
            player p = FindObjectOfType<player>();
            if (p != null) p.TriggerQuizInvincibility(3f);
        }
        else
        {
            player p = FindObjectOfType<player>();
            if (p != null) p.TakeDamage(1); 
            if (gm != null) gm.ResumeGame(); 
        }

        if (quizPanel != null) quizPanel.SetActive(false);
    }
}
