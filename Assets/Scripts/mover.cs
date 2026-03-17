using UnityEngine;

public class mover : MonoBehaviour
{
    // 💡 글로벌 속도를 기준으로 몇 배 빠르게/느리게 갈 건지 정하는 비율 (기본값 1)
    public float speedMultiplier = 1f; 

    void Update()
    {
        // GameManager의 글로벌 속도에 내 비율을 곱해서 이동합니다.
        transform.Translate(Vector3.left * GameManager.globalSpeed * speedMultiplier * Time.deltaTime);
    }
}