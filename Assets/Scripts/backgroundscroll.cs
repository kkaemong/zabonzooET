using UnityEngine;

public class backgroundscroll : MonoBehaviour
{
    public float scrollSpeed = 0.2f;
    public Renderer targetRenderer;

    void Start()
    {
        // Inspector에서 할당하지 않았다면 현재 오브젝트에서 찾아옵니다.
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }
    }

    void Update()
    {
        // x축으로 이미지를 밀어줍니다.
        Vector2 offset = new Vector2(Time.time * scrollSpeed, 0);
        
        // 가져온 렌더러의 머티리얼 오프셋을 조절합니다.
        targetRenderer.material.mainTextureOffset = offset;
    }
}