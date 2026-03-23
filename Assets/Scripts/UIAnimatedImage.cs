using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIAnimatedImage : MonoBehaviour
{
    public Sprite[] frames;
    public float framesPerSecond = 12f;
    
    private Image image;
    private int currentFrame;
    private float timer;

    void Awake()
    {
        image = GetComponent<Image>();
    }

    void Update()
    {
        if (frames == null || frames.Length == 0) return;

        // Use unscaled time so the animation plays even when the game is paused (Time.timeScale = 0)
        timer += Time.unscaledDeltaTime; 
        if (timer >= 1f / framesPerSecond)
        {
            timer -= 1f / framesPerSecond;
            currentFrame = (currentFrame + 1) % frames.Length;
            image.sprite = frames[currentFrame];
        }
    }
}
