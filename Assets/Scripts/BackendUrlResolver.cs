using System;
using UnityEngine;

public static class BackendUrlResolver
{
    public const string LocalBaseUrl = "http://localhost:8080";
    public const string ProductionBaseUrl = "http://j14a507.p.ssafy.io";

    public static bool UsesBrowserManagedCookies
    {
        get
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return true;
#else
            return false;
#endif
        }
    }

    public static string DefaultBaseUrl => Resolve(null);

    public static string Resolve(string configuredBaseUrl)
    {
        string normalizedBaseUrl = Normalize(configuredBaseUrl);

        // 💡 [환경별 자동 감지] 에디터에서는 로컬 백엔드를 기본으로 사용합니다.
#if UNITY_EDITOR
        if (string.IsNullOrWhiteSpace(normalizedBaseUrl) || normalizedBaseUrl == ProductionBaseUrl)
        {
            Debug.Log($"<color=yellow>[BackendUrlResolver]</color> Unity Editor Detected. Using Local Backend: {LocalBaseUrl}");
            return LocalBaseUrl;
        }
#endif

        if (UsesBrowserManagedCookies && ShouldUsePageOrigin(normalizedBaseUrl))
        {
            string pageOrigin = GetPageOrigin();
            if (!string.IsNullOrWhiteSpace(pageOrigin))
            {
                return pageOrigin;
            }
        }

        return string.IsNullOrWhiteSpace(normalizedBaseUrl)
            ? ProductionBaseUrl
            : normalizedBaseUrl;
    }

    public static string Normalize(string baseUrl)
    {
        return string.IsNullOrWhiteSpace(baseUrl)
            ? string.Empty
            : baseUrl.Trim().TrimEnd('/');
    }

    private static bool ShouldUsePageOrigin(string normalizedBaseUrl)
    {
        if (string.IsNullOrWhiteSpace(normalizedBaseUrl))
        {
            return true;
        }

        if (!Uri.TryCreate(normalizedBaseUrl, UriKind.Absolute, out Uri configuredUri))
        {
            return false;
        }

        if (!Uri.TryCreate(ProductionBaseUrl, UriKind.Absolute, out Uri productionUri))
        {
            return false;
        }

        return string.Equals(configuredUri.Host, productionUri.Host, StringComparison.OrdinalIgnoreCase);
    }

    private static string GetPageOrigin()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        string absoluteUrl = Application.absoluteURL;
        if (Uri.TryCreate(absoluteUrl, UriKind.Absolute, out Uri pageUri))
        {
            return pageUri.GetLeftPart(UriPartial.Authority).TrimEnd('/');
        }
#endif
        return string.Empty;
    }
}
