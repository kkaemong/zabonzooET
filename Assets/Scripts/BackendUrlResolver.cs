using System;
using UnityEngine;

public static class BackendUrlResolver
{
    public const string LegacyDefaultBaseUrl = "http://j14a507.p.ssafy.io";

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
        if (UsesBrowserManagedCookies && ShouldUsePageOrigin(normalizedBaseUrl))
        {
            string pageOrigin = GetPageOrigin();
            if (!string.IsNullOrWhiteSpace(pageOrigin))
            {
                return pageOrigin;
            }
        }

        return string.IsNullOrWhiteSpace(normalizedBaseUrl)
            ? LegacyDefaultBaseUrl
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

        if (!Uri.TryCreate(LegacyDefaultBaseUrl, UriKind.Absolute, out Uri legacyUri))
        {
            return false;
        }

        return string.Equals(configuredUri.Host, legacyUri.Host, StringComparison.OrdinalIgnoreCase);
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
