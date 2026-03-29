using UnityEngine;

public sealed class SharedSceneBgm : MonoBehaviour
{
    private static SharedSceneBgm _instance;

    private AudioSource _audioSource;
    private string _currentResourcePath = string.Empty;

    public static void Play(string resourcePath, float volume, bool restartIfSame = false)
    {
        if (string.IsNullOrWhiteSpace(resourcePath))
        {
            return;
        }

        SharedSceneBgm instance = EnsureInstance();
        instance.PlayInternal(resourcePath, volume, restartIfSame);
    }

    public static void Stop()
    {
        if (_instance == null || _instance._audioSource == null)
        {
            return;
        }

        _instance._audioSource.Stop();
        _instance._audioSource.time = 0f;
    }

    public static bool IsPlaying(string resourcePath)
    {
        return _instance != null &&
               _instance._audioSource != null &&
               _instance._audioSource.isPlaying &&
               string.Equals(_instance._currentResourcePath, resourcePath, System.StringComparison.Ordinal);
    }

    private static SharedSceneBgm EnsureInstance()
    {
        if (_instance != null)
        {
            return _instance;
        }

        GameObject root = new GameObject("SharedSceneBgm");
        DontDestroyOnLoad(root);
        _instance = root.AddComponent<SharedSceneBgm>();
        return _instance;
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }

        _audioSource.playOnAwake = false;
        _audioSource.loop = true;
    }

    private void PlayInternal(string resourcePath, float volume, bool restartIfSame)
    {
        AudioClip clip = Resources.Load<AudioClip>(resourcePath);
        if (clip == null)
        {
            Debug.LogWarning($"SharedSceneBgm: AudioClip resource '{resourcePath}' was not found.", this);
            return;
        }

        bool isSameClip = string.Equals(_currentResourcePath, resourcePath, System.StringComparison.Ordinal) &&
                          _audioSource.clip == clip;

        _audioSource.volume = volume;

        if (isSameClip && _audioSource.isPlaying && !restartIfSame)
        {
            return;
        }

        if (!isSameClip)
        {
            _audioSource.Stop();
            _audioSource.clip = clip;
            _currentResourcePath = resourcePath;
        }

        if (restartIfSame || !_audioSource.isPlaying)
        {
            _audioSource.time = 0f;
            _audioSource.Play();
        }
    }
}
