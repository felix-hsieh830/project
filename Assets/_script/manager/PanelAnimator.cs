using UnityEngine;
using System.Collections;

public class PanelAnimator : MonoBehaviour
{
    [Header("動畫設定")]
    public float animDuration = 0.3f;       // 動畫時間
    public RectTransform settingsButton;    // 右上角設定按鈕（當作展開起點）

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private Vector3 originalPos;  // 🌟 記住面板原始位置

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        originalPos = rectTransform.position; // 🌟 記住原始位置
    }

    // 🌟 開啟面板：從按鈕位置展開
    public void Show()
    {
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(AnimateShow());
    }

    // 🌟 關閉面板：縮回按鈕位置
    public void Hide()
    {
        StopAllCoroutines();
        StartCoroutine(AnimateHide());
    }

    IEnumerator AnimateShow()
    {
        float elapsed = 0f;

        // 起點：按鈕的位置，縮小到 0
        Vector3 startPos = settingsButton != null
            ? settingsButton.position
            : rectTransform.position;

        Vector3 endPos = originalPos;

        canvasGroup.alpha = 0f;
        rectTransform.localScale = Vector3.zero;
        rectTransform.position = startPos;

        while (elapsed < animDuration)
        {
            elapsed += Time.unscaledDeltaTime; // 🌟 用 unscaledDeltaTime，暫停時也能播放
            float t = Mathf.Clamp01(elapsed / animDuration);
            float eased = EaseOutBack(t); // 有點彈性的緩動

            rectTransform.localScale = Vector3.one * eased;
            rectTransform.position = Vector3.Lerp(startPos, endPos, t);
            canvasGroup.alpha = t;

            yield return null;
        }

        rectTransform.localScale = Vector3.one;
        rectTransform.position = endPos;
        canvasGroup.alpha = 1f;
    }

    IEnumerator AnimateHide()
    {
        float elapsed = 0f;

        Vector3 startPos = rectTransform.position;
        Vector3 endPos = settingsButton != null
            ? settingsButton.position
            : rectTransform.position;

        while (elapsed < animDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / animDuration);
            float eased = 1f - EaseInBack(t);

            rectTransform.localScale = Vector3.one * eased;
            rectTransform.position = Vector3.Lerp(startPos, endPos, t);
            canvasGroup.alpha = 1f - t;

            yield return null;
        }

        rectTransform.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    // 有彈性的緩動公式
    float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    float EaseInBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return c3 * t * t * t - c1 * t * t;
    }
}