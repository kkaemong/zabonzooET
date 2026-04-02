using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class FinanceOptionPanelLayout
{
    private const float TwoOptionOffset = 360f;
    private const float ThreeOptionOffset = 560f;

    public static void RecenterActiveButtons(IReadOnlyList<Button> buttons)
    {
        if (buttons == null || buttons.Count == 0)
        {
            return;
        }

        List<RectTransform> activeRects = new List<RectTransform>();
        for (int i = 0; i < buttons.Count; i++)
        {
            Button button = buttons[i];
            if (button == null || !button.gameObject.activeSelf)
            {
                continue;
            }

            RectTransform rect = button.transform as RectTransform;
            if (rect != null)
            {
                activeRects.Add(rect);
            }
        }

        if (activeRects.Count == 0)
        {
            return;
        }

        float[] targetPositions = ResolvePositions(activeRects.Count);
        for (int i = 0; i < activeRects.Count; i++)
        {
            RectTransform rect = activeRects[i];
            Vector2 anchoredPosition = rect.anchoredPosition;

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(targetPositions[i], anchoredPosition.y);
        }
    }

    private static float[] ResolvePositions(int activeCount)
    {
        switch (activeCount)
        {
            case 1:
                return new[] { 0f };
            case 2:
                return new[] { -TwoOptionOffset, TwoOptionOffset };
            case 3:
                return new[] { -ThreeOptionOffset, 0f, ThreeOptionOffset };
            default:
            {
                float spacing = ThreeOptionOffset;
                float start = -spacing * (activeCount - 1) * 0.5f;
                float[] positions = new float[activeCount];
                for (int i = 0; i < activeCount; i++)
                {
                    positions[i] = start + (spacing * i);
                }

                return positions;
            }
        }
    }
}
