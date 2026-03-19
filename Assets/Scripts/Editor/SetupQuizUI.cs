#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class SetupQuizUI : EditorWindow
{
    [MenuItem("Custom Tools/설정 마법사: 경제 퀴즈 UI 자동 생성")]
    public static void CreateQuizUI()
    {
        // 1. 점수 캔버스 가져오기 (이전에 만든 ScoreCanvas 재사용)
        GameObject parentCanvas = GameObject.Find("ScoreCanvas");
        Canvas c;
        CanvasScaler scaler;
        if (parentCanvas == null)
        {
            parentCanvas = new GameObject("ScoreCanvas");
            c = parentCanvas.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            scaler = parentCanvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); // 기준 해상도 명시
            parentCanvas.AddComponent<GraphicRaycaster>();
            c.sortingOrder = 100; // 가장 위에 보이도록
        }
        else
        {
            // 기존 캔버스에 CanvasScaler가 없다면 추가해줍니다
            scaler = parentCanvas.GetComponent<CanvasScaler>();
            if (scaler == null) 
            {
                scaler = parentCanvas.AddComponent<CanvasScaler>();
            }
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }

        // EventSystem 추가 및 찌꺼기 방지 완벽 초기화
        UnityEngine.EventSystems.EventSystem oldEs = GameObject.FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (oldEs != null)
        {
            DestroyImmediate(oldEs.gameObject);
        }
        
        GameObject eventSystemObj = new GameObject("EventSystem");
        UnityEngine.EventSystems.EventSystem es = eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        
        // 💡 100% 깨끗한 상태에서 스페이스바 간섭 끄기 적용
        es.sendNavigationEvents = false; 

        // 기존에 잘못 생성된 QuizPanel이나 DistanceText가 있다면 깨끗하게 지우기
        Transform oldPanel = parentCanvas.transform.Find("QuizPanel");
        if (oldPanel != null) DestroyImmediate(oldPanel.gameObject);
        
        Transform oldDist = parentCanvas.transform.Find("DistanceText");
        if (oldDist != null) DestroyImmediate(oldDist.gameObject);

        // 2. 퀴즈 패널 (배경) 생성
        GameObject quizPanel = new GameObject("QuizPanel");
        quizPanel.transform.SetParent(parentCanvas.transform, false);
        Image panelImage = quizPanel.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.2f, 0.95f); // 진한 남색 반투명 까리한 배경
        RectTransform panelRect = quizPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero; // 위치 중앙으로 초기화
        panelRect.localPosition = Vector3.zero;

        // 💡 2.5 속보(Breaking News) 타이틀 추가
        GameObject titleObj = new GameObject("BreakingNewsTitle");
        titleObj.transform.SetParent(quizPanel.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "🚨 BREAKING NEWS 🚨";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 60;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(1f, 0.2f, 0.2f); // 강렬한 빨간색
        
        Outline titleOutline = titleObj.AddComponent<Outline>();
        titleOutline.effectColor = Color.white;
        titleOutline.effectDistance = new Vector2(2, -2);
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.sizeDelta = new Vector2(800, 100);
        titleRect.anchoredPosition = new Vector2(0, 420); // 문제 텍스트보다 더 위에 배치

        // 3. 문제 텍스트 생성
        GameObject questionObj = new GameObject("QuestionText");
        questionObj.transform.SetParent(quizPanel.transform, false);
        Text qText = questionObj.AddComponent<Text>();
        qText.text = "여기에 퀴즈 문제가 나옵니다.";
        qText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        qText.fontSize = 80; 
        qText.fontStyle = FontStyle.Bold;
        qText.alignment = TextAnchor.MiddleCenter;
        qText.color = new Color(1f, 0.9f, 0.2f); // 쿠키런 스타일 노란색 텍스트 포인트
        
        // 문제 텍스트에 두꺼운 외곽선 추가
        Outline qOutline = questionObj.AddComponent<Outline>();
        qOutline.effectColor = new Color(0.2f, 0.1f, 0f); // 진갈색 외곽선
        qOutline.effectDistance = new Vector2(4, -4);
        
        // 문제 텍스트 그림자
        Shadow qShadow = questionObj.AddComponent<Shadow>();
        qShadow.effectColor = new Color(0, 0, 0, 0.5f);
        qShadow.effectDistance = new Vector2(5, -10);

        RectTransform qRect = questionObj.GetComponent<RectTransform>();
        qRect.sizeDelta = new Vector2(1600, 400); 
        qRect.anchoredPosition = new Vector2(0, 150); // 타이틀이 위에 있으므로 문제 공간을 살짝만 내림

        // 4. 정답 버튼들 (2x2 카드형상 그리드 정렬용 그룹)
        GameObject btnGroup = new GameObject("ButtonGroup");
        btnGroup.transform.SetParent(quizPanel.transform, false);
        GridLayoutGroup gGroup = btnGroup.AddComponent<GridLayoutGroup>();
        gGroup.childAlignment = TextAnchor.MiddleCenter;
        gGroup.cellSize = new Vector2(500, 160); // 각 버튼 타일 크기 
        gGroup.spacing = new Vector2(50, 50);    // 버튼 간 가로세로 간격 
        RectTransform groupRect = btnGroup.GetComponent<RectTransform>();
        groupRect.sizeDelta = new Vector2(1100, 400); // 2x2 그리드를 감싸는 넉넉한 크기
        groupRect.anchoredPosition = new Vector2(0, -150);

        // 버튼 A, B, C, D 생성 (색상을 알록달록하게)
        Color colBtn1 = new Color(1f, 0.4f, 0.4f); // 핑크/레드
        Color colBtn2 = new Color(0.4f, 0.7f, 1f); // 스카이블루
        Color colBtn3 = new Color(0.4f, 0.9f, 0.4f); // 밝은 그린
        Color colBtn4 = new Color(1f, 0.7f, 0.2f); // 오렌지

        GameObject btnA = CreateButton("ButtonA", "A", btnGroup.transform, colBtn1);
        Button buttonAComponent = btnA.GetComponent<Button>();
        Text textAComponent = btnA.GetComponentInChildren<Text>();

        GameObject btnB = CreateButton("ButtonB", "B", btnGroup.transform, colBtn2);
        Button buttonBComponent = btnB.GetComponent<Button>();
        Text textBComponent = btnB.GetComponentInChildren<Text>();

        GameObject btnC = CreateButton("ButtonC", "C", btnGroup.transform, colBtn3);
        Button buttonCComponent = btnC.GetComponent<Button>();
        Text textCComponent = btnC.GetComponentInChildren<Text>();

        GameObject btnD = CreateButton("ButtonD", "D", btnGroup.transform, colBtn4);
        Button buttonDComponent = btnD.GetComponent<Button>();
        Text textDComponent = btnD.GetComponentInChildren<Text>();

        // 5. 게임매니저 & 퀴즈매니저 연결
        GameObject gmObj = GameObject.Find("GameManager");
        if (gmObj == null) gmObj = new GameObject("GameManager");
        
        QuizManager qm = gmObj.GetComponent<QuizManager>();
        if (qm == null) qm = gmObj.AddComponent<QuizManager>();

        qm.quizPanel = quizPanel;
        qm.questionTextObj = qText.gameObject;
        qm.buttonAObj = buttonAComponent.gameObject;
        qm.buttonBObj = buttonBComponent.gameObject;
        qm.buttonCObj = buttonCComponent.gameObject;
        qm.buttonDObj = buttonDComponent.gameObject;
        qm.btnTextAObj = textAComponent.gameObject;
        qm.btnTextBObj = textBComponent.gameObject;
        qm.btnTextCObj = textCComponent.gameObject;
        qm.btnTextDObj = textDComponent.gameObject;

        // 6. 달린 거리(Distance) UI 추가
        GameObject distObj = GameObject.Find("DistanceText");
        if (distObj == null)
        {
            distObj = new GameObject("DistanceText");
            distObj.transform.SetParent(parentCanvas.transform, false);
            Text dText = distObj.AddComponent<Text>();
            dText.text = "0m";
            dText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            dText.fontSize = 60; // (거리 폰트도 조금 더 키움)
            dText.fontStyle = FontStyle.Bold;
            dText.alignment = TextAnchor.MiddleRight;
            dText.color = Color.white;
            
            Outline outline = distObj.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(2, -2);

            RectTransform dRect = distObj.GetComponent<RectTransform>();
            dRect.anchorMin = new Vector2(1, 1);
            dRect.anchorMax = new Vector2(1, 1);
            dRect.pivot = new Vector2(1, 1);
            // 코인 점수가 우측 상단에 있을 테니, 그 조금 아래쪽에 배치
            dRect.anchoredPosition = new Vector2(-50, -150); 
            dRect.sizeDelta = new Vector2(300, 100);

            GameManager gm = gmObj.GetComponent<GameManager>();
            if (gm != null)
            {
                gm.distanceText = dText;
            }
        }
        
        // 7. Speed UP! 텍스트 (정답 시 애니메이션용) 추가
        GameObject levelUpObj = GameObject.Find("LevelUpText");
        if (levelUpObj == null)
        {
            levelUpObj = new GameObject("LevelUpText");
            levelUpObj.transform.SetParent(parentCanvas.transform, false);
            Text lvlText = levelUpObj.AddComponent<Text>();
            lvlText.text = "Speed UP!";
            lvlText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            lvlText.fontSize = 120;
            lvlText.fontStyle = FontStyle.Bold;
            lvlText.alignment = TextAnchor.MiddleCenter;
            lvlText.color = new Color(1f, 0.8f, 0.2f, 0f); // 투명한 노란색(안보임)
            
            Outline lvlOutline = levelUpObj.AddComponent<Outline>();
            lvlOutline.effectColor = new Color(0, 0, 0, 1f);
            lvlOutline.effectDistance = new Vector2(5, -5);

            RectTransform lvlRect = levelUpObj.GetComponent<RectTransform>();
            lvlRect.anchorMin = new Vector2(0.5f, 0.5f);
            lvlRect.anchorMax = new Vector2(0.5f, 0.5f);
            lvlRect.pivot = new Vector2(0.5f, 0.5f);
            lvlRect.anchoredPosition = new Vector2(0, 300); // 화면 위쪽 중앙
            lvlRect.sizeDelta = new Vector2(800, 200);

            GameManager gm = gmObj.GetComponent<GameManager>();
            if (gm != null)
            {
                // GameManager 컴포넌트에 levelUpText 연결
                gm.levelUpText = lvlText;
            }
        }

        // 끄기 (평소에는 안보이다가 몬스터 부딪히면 켜짐)
        quizPanel.SetActive(false);

        Debug.Log("<color=green>[완료]</color> 경제 퀴즈 UI 및 달린 거리창 생성이 자동 완료되었습니다!");
    }

    private static GameObject CreateButton(string name, string textStr, Transform parent, Color bgColor)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = bgColor; 
        
        // 버튼 외곽선 효과 (테두리 느낌)
        Outline btnOutline = btnObj.AddComponent<Outline>();
        btnOutline.effectColor = new Color(1f, 1f, 1f, 0.8f); // 하얀색 테두리
        btnOutline.effectDistance = new Vector2(4, -4);
        
        // 버튼 그림자
        Shadow btnShadow = btnObj.AddComponent<Shadow>();
        btnShadow.effectColor = new Color(0, 0, 0, 0.4f);
        btnShadow.effectDistance = new Vector2(6, -8);

        Button btnComp = btnObj.AddComponent<Button>();
        
        RectTransform rect = btnObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(500, 160); // 버튼 더 큼직하게 

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        Text txt = textObj.AddComponent<Text>();
        txt.text = textStr;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = 75; 
        txt.fontStyle = FontStyle.Bold;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        
        // 버튼 글자 까리한 검은 테두리
        Outline txtOutline = textObj.AddComponent<Outline>();
        txtOutline.effectColor = new Color(0f, 0f, 0f, 0.8f);
        txtOutline.effectDistance = new Vector2(3, -3);

        RectTransform txtRect = textObj.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.sizeDelta = Vector2.zero;

        return btnObj;
    }
}
#endif
