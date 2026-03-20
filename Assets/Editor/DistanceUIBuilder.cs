using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

public class DistanceUIBuilder : EditorWindow
{
    [MenuItem("Tools/Build Distance Progress Bar")]
    public static void BuildUI()
    {
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null) 
        {
            Debug.LogError("에러: 씬에 Canvas가 없습니다! Canvas를 먼저 생성해주세요.");
            return;
        }

        GameObject oldBar = GameObject.Find("DistanceProgressBarUI");
        if(oldBar != null) Object.DestroyImmediate(oldBar); // 기존에 있다면 깔끔하게 지우고 다시 만들기

        // 배경 바 생성
        GameObject bgObj = new GameObject("DistanceProgressBarUI");
        bgObj.transform.SetParent(canvas.transform, false);
        RectTransform bgRt = bgObj.AddComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0.25f, 0.85f);
        bgRt.anchorMax = new Vector2(0.75f, 0.88f);
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        // 채워지는 영역
        GameObject fillArea = new GameObject("FillArea");
        fillArea.transform.SetParent(bgObj.transform, false);
        RectTransform faRt = fillArea.AddComponent<RectTransform>();
        faRt.anchorMin = Vector2.zero; faRt.anchorMax = Vector2.one;
        faRt.offsetMin = new Vector2(3, 3); faRt.offsetMax = new Vector2(-3, -3);

        // 채워지는 바(초록색)
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillArea.transform, false);
        RectTransform fRt = fillObj.AddComponent<RectTransform>();
        fRt.anchorMin = Vector2.zero; fRt.anchorMax = new Vector2(0, 1);
        fRt.offsetMin = Vector2.zero; fRt.offsetMax = Vector2.zero;
        Image fImg = fillObj.AddComponent<Image>();
        fImg.color = new Color(0.1f, 0.8f, 0.2f, 1f);

        // 슬라이더 조립
        Slider slider = bgObj.AddComponent<Slider>();
        slider.fillRect = fRt;
        slider.interactable = false;
        slider.minValue = 0; slider.maxValue = 1;

        // 퀴즈 마커 (333m, 666m 지점)
        float[] quizDists = { 0.333f, 0.666f };
        foreach(float d in quizDists) 
        {
            GameObject marker = new GameObject("QuizMarker_" + d);
            marker.transform.SetParent(bgObj.transform, false);
            RectTransform mRt = marker.AddComponent<RectTransform>();
            mRt.anchorMin = new Vector2(d, 0.1f);
            mRt.anchorMax = new Vector2(d, 0.9f);
            mRt.sizeDelta = new Vector2(5, 0);
            mRt.anchoredPosition = Vector2.zero;
            Image mImg = marker.AddComponent<Image>();
            mImg.color = Color.red;
        }

        // 플레이어 캐릭터 아이콘
        Sprite playerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/things/메인캐릭터.png");
        GameObject iconObj = new GameObject("PlayerHandle");
        iconObj.transform.SetParent(bgObj.transform, false);
        RectTransform iconRt = iconObj.AddComponent<RectTransform>();
        iconRt.sizeDelta = new Vector2(80, 80); // 아이콘 크기
        Image iconImg = iconObj.AddComponent<Image>();
        
        if (playerSprite != null) 
            iconImg.sprite = playerSprite;
        else 
            Debug.LogWarning("메인캐릭터.png를 찾을 수 없어서 아이콘 이미지가 비어 있습니다!");

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("<color=green>성공! 화면 상단 중앙에 게이지 바 세팅이 완료되었습니다. 게임을 실행해 보세요!</color>");
    }
}
