using UnityEngine;

public class backgroundscroll : MonoBehaviour
{
    // 💡 글로벌 속도를 기준으로 텍스처를 밀어낼 비율 (초기값은 아주 작게 시작해서 눈으로 맞추세요)
    public float scrollRatio = 0.05f; 
    
    private Renderer targetRenderer;
    private Vector2 savedOffset; // 현재 위치를 기억할 변수

    void Start()
    {
        targetRenderer = GetComponent<Renderer>();
        if (targetRenderer != null)
        {
            savedOffset = targetRenderer.material.mainTextureOffset;
        }
    }

    void Update()
    {
        // 💡 핵심: Time.time을 버리고, GameManager의 속도에 맞춰 이동량을 '더해줍니다'
        float moveAmount = GameManager.globalSpeed * scrollRatio * Time.deltaTime;
        
        savedOffset.x += moveAmount;
        
        if (targetRenderer != null)
        {
            targetRenderer.material.mainTextureOffset = savedOffset;
        }
    }
}