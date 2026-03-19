using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    private static CameraShake instance;
    private Vector3 originalPos;
    private Coroutine shakeCoroutine;

    public static void Shake(float duration = 0.5f, float magnitude = 0.1f)
    {
        if (Camera.main == null) return;

        if (instance == null)
        {
            instance = Camera.main.gameObject.GetComponent<CameraShake>();
            if (instance == null)
            {
                instance = Camera.main.gameObject.AddComponent<CameraShake>();
                instance.originalPos = Camera.main.transform.localPosition;
            }
        }

        instance.StartShake(duration, magnitude);
    }

    private void StartShake(float duration, float magnitude)
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            transform.localPosition = originalPos; // 이전 흔들기 초기화
        }
        shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);

            elapsed += Time.unscaledDeltaTime; // TimeScale=0 일 때도 작동하도록
            yield return null;
        }

        transform.localPosition = originalPos;
        shakeCoroutine = null;
    }
}
