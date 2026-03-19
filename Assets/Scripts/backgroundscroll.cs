using UnityEngine;

public class backgroundscroll : MonoBehaviour
{
    // 💡 글로벌 속도를 기준으로 텍스처를 밀어낼 비율 (초기값은 아주 작게 시작해서 눈으로 맞추세요)
    public float scrollRatio = 0.05f; 
    
    [Header("시대별 배경 틴트 컬러 (80년대 -> 현대로)")]
    public Color startColor = new Color(1f, 0.85f, 0.7f); // 약간 빛바랜 따뜻한 느낌 (1980년대)
    public Color endColor = new Color(1f, 1f, 1f);       // 완전 밝은 원래 색깔 (2020년대)
    public float colorChangeDistance = 1500f;            // 1500m에 도달하면 완전한 현대로 변함

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

            // 💡 시대 변화 (거리에 따른 색조 변화)
            float t = Mathf.Clamp01(GameManager.distanceTraveled / colorChangeDistance);
            targetRenderer.material.color = Color.Lerp(startColor, endColor, t);
        }
    }
}