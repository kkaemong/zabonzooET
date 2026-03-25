using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 서버(백엔드)와의 HTTP 통신만을 전담하는 매니저 스크립트입니다.
/// </summary>
public class APIManager : MonoBehaviour
{
    // 어디서든 APIManager.Instance.함수명() 으로 쉽게 부르기 위한 싱글톤
    public static APIManager Instance { get; private set; }

    [Header("서버 설정")]
    [Tooltip("백엔드 서버의 기본 주소 (예: http://3.33.22.11:8080)")]
    public string baseUrl = "http://j14a507.p.ssafy.io"; // TODO: 실제 배포된 서버 주소로 변경하세요!

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 넘어가도 매니저가 파괴되지 않음
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ====================================================
    // 1. [게임 결과 및 획득 코인 전송] (POST /api/game/run-result)
    // 👉 사용법: GameManager에서 플레이어 사망 시 APIManager.Instance.SendGameResult(코인개수); 호출
    // ====================================================
    
    // (보낼 데이터를 JSON 형식에 맞게 묶어주는 클래스)
    [System.Serializable]
    public class RunResultRequest
    {
        public int earnedCoins;
        public string stageId;
        // 백엔드 명세서에 따라 userId 등 필요한 항목을 여기에 더 추가하면 됩니다.
    }

    public void SendGameResult(int coins, string currentStage = "2000s")
    {
        RunResultRequest data = new RunResultRequest 
        { 
            earnedCoins = coins,
            stageId = currentStage
        };
        
        // C# 객체를 JSON 글자(String)로 예쁘게 변환
        string jsonString = JsonUtility.ToJson(data); 

        // 코루틴으로 서버에 전송 시작
        StartCoroutine(PostRequest(baseUrl + "/api/game/run-result", jsonString));
    }

    // ====================================================
    // 2. [스테이지 정보 (퀴즈 등) 조회] (GET /api/game/stage/{stageId})
    // 👉 사용법: 게임 시작 전 또는 씬 로드 시 APIManager.Instance.GetStageInfo("스테이지번호"); 호출
    // ====================================================
    public void GetStageInfo(string stageId)
    {
        StartCoroutine(GetRequest(baseUrl + $"/api/game/stage/{stageId}"));
    }


    // ====================================================
    // 3. [금융 이벤트 선택지 조회] (GET /api/game/finance-options)
    // 👉 사용법: 게임 클리어 후 금융상품 선택 버튼 누를 때 호출
    // ====================================================
    public void GetFinanceOptions()
    {
        StartCoroutine(GetRequest(baseUrl + "/api/game/finance-options"));
    }

    // ====================================================
    // [공통 HTTP 통신 내부 로직] (이 아래는 수정할 필요가 거의 없습니다)
    // ====================================================

    // POST 방식 (내 데이터를 서버에 저장해 달라고 보낼 때)
    private IEnumerator PostRequest(string url, string jsonBody)
    {
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            // JSON 글자를 컴퓨터가 읽을 수 있는 바이트(Byte)로 변환
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            
            // "나 지금 JSON 보낸다!" 라고 서버에 미리 알려주는 헤더
            request.SetRequestHeader("Content-Type", "application/json"); 

            // 💡 만약 백엔드에서 로그인 토큰(JWT)이 필요하다고 하면 아래 주석을 풀고 추가하세요.
            // request.SetRequestHeader("Authorization", "Bearer " + "여기에유저토큰값");

            Debug.Log($"<color=cyan>[API POST 요청]</color> 주소: {url}\n보낸 데이터: {jsonBody}");

            // 통신 끝날 때까지 대기
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"<color=red>[API POST 에러]</color> {request.error}");
            }
            else
            {
                Debug.Log($"<color=green>[API POST 성공!]</color> 서버 응답: {request.downloadHandler.text}");
            }
        }
    }

    // GET 방식 (서버에서 데이터를 가져오라고 시킬 때)
    private IEnumerator GetRequest(string url)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            // 💡 만약 백엔드에서 로그인 토큰(JWT)이 필요하다고 하면 아래 주석을 풀고 추가하세요.
            // request.SetRequestHeader("Authorization", "Bearer " + "여기에유저토큰값");

            Debug.Log($"<color=cyan>[API GET 요청]</color> 주소: {url}");

            // 통신 끝날 때까지 대기
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"<color=red>[API GET 에러]</color> {request.error}");
            }
            else
            {
                Debug.Log($"<color=green>[API GET 성공!]</color> 서버 데이터: {request.downloadHandler.text}");
                
                // TODO: 받아온 JSON 텍스트(request.downloadHandler.text)를 파싱해서
                // 퀴즈 매니저 등에 넘겨주는 코드를 나중에 여기에 한 줄 추가하시면 됩니다!
            }
        }
    }
}
