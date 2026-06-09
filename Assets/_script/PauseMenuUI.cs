using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class PauseMenuUI : MonoBehaviour
{
    [Header("素材（拖入 Sprite）")]
    public Sprite btnNormal;    // TextBTN_Medium
    public Sprite btnPressed;   // TextBTN_Medium_Pressed

    [Header("引用")]
    public GameManager gameManager;

    private GameObject pausePanel;
    private RectTransform panelRect;
    private RectTransform statsPanelRect;
    private CanvasGroup canvasGroup;
    private RectTransform overlayRect;
    private float fittedScale = 1f;
    private TextMeshProUGUI statsBodyText;

    // 顏色設定
    private Color overlayColor = new Color(0f, 0f, 0f, 0.76f);
    private Color bgColor = new Color(0.09f, 0.08f, 0.05f, 0.98f);
    private Color borderColor = new Color(0.7f, 0.5f, 0.14f, 1f);
    private Color titleColor = new Color(1f, 0.86f, 0.34f, 1f);
    private Color btnTextNormal = new Color(1f, 0.9f, 0.62f, 1f);
    private Color btnTextPressed = new Color(0.42f, 0.33f, 0.12f, 1f);
    private Color statsLabelColor = new Color(0.9f, 0.78f, 0.48f, 1f);
    private Color statsValueColor = new Color(0.96f, 0.92f, 0.82f, 1f);

    private const float MenuWidth = 440f;
    private const float MenuHeight = 620f;
    private const float StatsWidth = 340f;
    private const float ContentGap = 22f;
    private const float ContentWidth = MenuWidth + ContentGap + StatsWidth;
    private const float ContentHeight = MenuHeight;

    void Start()
    {
        if (gameManager == null)
            gameManager = FindAnyObjectByType<GameManager>();

        BuildUI();
    }

    void BuildUI()
    {
        // 找到場景的 Canvas
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("找不到 Canvas！");
            return;
        }

        // === 全螢幕黑色遮罩 ===
        pausePanel = new GameObject("PausePanel");
        pausePanel.transform.SetParent(canvas.transform, false);

        overlayRect = pausePanel.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        Image overlayImage = pausePanel.AddComponent<Image>();
        overlayImage.color = overlayColor;

        // === 內層內容群組 ===
        GameObject contentRoot = new GameObject("PauseContent");
        contentRoot.transform.SetParent(pausePanel.transform, false);

        panelRect = contentRoot.AddComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(ContentWidth, ContentHeight);
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;

        // === 左側選單卡片 ===
        GameObject menuCard = new GameObject("MenuCard");
        menuCard.transform.SetParent(contentRoot.transform, false);

        RectTransform menuRect = menuCard.AddComponent<RectTransform>();
        menuRect.sizeDelta = new Vector2(MenuWidth, MenuHeight);
        menuRect.anchorMin = new Vector2(0.5f, 0.5f);
        menuRect.anchorMax = new Vector2(0.5f, 0.5f);
        menuRect.pivot = new Vector2(0.5f, 0.5f);
        menuRect.anchoredPosition = new Vector2(-(StatsWidth + ContentGap) * 0.5f, 0f);

        Image bgImage = menuCard.AddComponent<Image>();
        bgImage.color = bgColor;

        CreateBorder(menuCard.transform, new Vector2(MenuWidth, MenuHeight), borderColor, 4f);

        // CanvasGroup（動畫用）
        canvasGroup = pausePanel.AddComponent<CanvasGroup>();

        // === 頂部裝飾線 ===
        CreateDivider(menuCard.transform, new Vector2(0f, 260f), new Vector2(340f, 3f));

        // === 標題 ===
        CreateTitle(menuCard.transform);

        // === 底部裝飾線 ===
        CreateDivider(menuCard.transform, new Vector2(0f, 185f), new Vector2(340f, 3f));

        // === 按鈕 ===
        string[] btnLabels = { "繼續遊戲", "重新開始", "遊戲設定", "回主畫面", "退出遊戲" };
        float startY = 128f;
        float spacing = 80f;

        for (int i = 0; i < btnLabels.Length; i++)
        {
            CreateButton(menuCard.transform, btnLabels[i], new Vector2(0f, startY - i * spacing), i);
        }

        // === 底部裝飾線 ===
        CreateDivider(menuCard.transform, new Vector2(0f, -255f), new Vector2(340f, 3f));

        CreateStatsPanel(contentRoot.transform);

        // 預設隱藏
        FitPanelToScreen();
        pausePanel.SetActive(false);
    }

    void FitPanelToScreen()
    {
        if (panelRect == null) return;

        float availableWidth = ContentWidth;
        float availableHeight = ContentHeight;
        if (overlayRect != null)
        {
            Rect rect = overlayRect.rect;
            if (rect.width > 1f) availableWidth = Mathf.Max(300f, rect.width - 80f);
            if (rect.height > 1f) availableHeight = Mathf.Max(420f, rect.height - 80f);
        }

        fittedScale = Mathf.Min(1f, availableWidth / ContentWidth, availableHeight / ContentHeight);
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(ContentWidth, ContentHeight);
        panelRect.anchoredPosition = Vector2.zero;
    }

    void CreateStatsPanel(Transform parent)
    {
        GameObject statsCard = new GameObject("StatsCard");
        statsCard.transform.SetParent(parent, false);

        statsPanelRect = statsCard.AddComponent<RectTransform>();
        statsPanelRect.sizeDelta = new Vector2(StatsWidth, MenuHeight);
        statsPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
        statsPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
        statsPanelRect.pivot = new Vector2(0.5f, 0.5f);
        statsPanelRect.anchoredPosition = new Vector2((MenuWidth + ContentGap) * 0.5f, 0f);

        Image bgImage = statsCard.AddComponent<Image>();
        bgImage.color = new Color(0.12f, 0.09f, 0.04f, 0.96f);

        CreateBorder(statsCard.transform, new Vector2(StatsWidth, MenuHeight), borderColor, 4f);
        CreateDivider(statsCard.transform, new Vector2(0f, 260f), new Vector2(260f, 3f));

        GameObject titleObj = new GameObject("StatsTitle");
        titleObj.transform.SetParent(statsCard.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.sizeDelta = new Vector2(290f, 54f);
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = new Vector2(0f, 224f);

        TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
        title.text = "目前數據";
        title.fontSize = 32f;
        title.color = titleColor;
        title.alignment = TextAlignmentOptions.Center;
        title.fontStyle = FontStyles.Bold;
        ApplyTextStyle(title, 0.2f, 0.14f);

        CreateDivider(statsCard.transform, new Vector2(0f, 185f), new Vector2(260f, 3f));

        GameObject bodyObj = new GameObject("StatsBody");
        bodyObj.transform.SetParent(statsCard.transform, false);
        RectTransform bodyRect = bodyObj.AddComponent<RectTransform>();
        bodyRect.sizeDelta = new Vector2(286f, 430f);
        bodyRect.anchorMin = new Vector2(0.5f, 0.5f);
        bodyRect.anchorMax = new Vector2(0.5f, 0.5f);
        bodyRect.anchoredPosition = new Vector2(0f, -52f);

        statsBodyText = bodyObj.AddComponent<TextMeshProUGUI>();
        statsBodyText.fontSize = 22f;
        statsBodyText.color = statsValueColor;
        statsBodyText.alignment = TextAlignmentOptions.TopLeft;
        statsBodyText.enableWordWrapping = false;
        statsBodyText.richText = true;
        statsBodyText.lineSpacing = 4f;

        CreateDivider(statsCard.transform, new Vector2(0f, -282f), new Vector2(260f, 3f));
    }

    void CreateTitle(Transform parent)
    {
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(parent, false);

        RectTransform rt = titleObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(380f, 70f);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, 222f);

        TextMeshProUGUI tmp = titleObj.AddComponent<TextMeshProUGUI>();
        tmp.text = "遊戲暫停";
        tmp.fontSize = 40f;
        tmp.color = titleColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        ApplyTextStyle(tmp, 0.24f, 0.18f);
    }

    void CreateDivider(Transform parent, Vector2 pos, Vector2 size)
    {
        // 左右線
        GameObject divObj = new GameObject("Divider");
        divObj.transform.SetParent(parent, false);
        RectTransform rt = divObj.AddComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        Image img = divObj.AddComponent<Image>();
        img.color = borderColor;

        // 中間菱形裝飾
        GameObject diamond = new GameObject("Diamond");
        diamond.transform.SetParent(parent, false);
        RectTransform drt = diamond.AddComponent<RectTransform>();
        drt.sizeDelta = new Vector2(14f, 14f);
        drt.anchorMin = new Vector2(0.5f, 0.5f);
        drt.anchorMax = new Vector2(0.5f, 0.5f);
        drt.anchoredPosition = pos;
        drt.localRotation = Quaternion.Euler(0, 0, 45f);
        Image dimg = diamond.AddComponent<Image>();
        dimg.color = borderColor;
    }

    void CreateBorder(Transform parent, Vector2 size, Color color, float thickness)
    {
        string[] sides = { "Top", "Bottom", "Left", "Right" };
        foreach (string side in sides)
        {
            GameObject border = new GameObject("Border_" + side);
            border.transform.SetParent(parent, false);
            RectTransform rt = border.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);

            if (side == "Top") { rt.sizeDelta = new Vector2(size.x, thickness); rt.anchoredPosition = new Vector2(0, size.y / 2f); }
            if (side == "Bottom") { rt.sizeDelta = new Vector2(size.x, thickness); rt.anchoredPosition = new Vector2(0, -size.y / 2f); }
            if (side == "Left") { rt.sizeDelta = new Vector2(thickness, size.y); rt.anchoredPosition = new Vector2(-size.x / 2f, 0); }
            if (side == "Right") { rt.sizeDelta = new Vector2(thickness, size.y); rt.anchoredPosition = new Vector2(size.x / 2f, 0); }

            Image img = border.AddComponent<Image>();
            img.color = color;
        }
    }

    void CreateButton(Transform parent, string label, Vector2 pos, int index)
    {
        GameObject btnObj = new GameObject("Btn_" + label);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(340f, 68f);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;

        Image img = btnObj.AddComponent<Image>();
        if (btnNormal != null)
        {
            img.sprite = btnNormal;
            img.type = Image.Type.Sliced;
        }
        else
        {
            // 沒有素材時用純色
            img.color = new Color(0.35f, 0.23f, 0.06f, 1f);
        }

        Button btn = btnObj.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;

        // 文字
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        RectTransform trt = textObj.AddComponent<RectTransform>();
        trt.sizeDelta = new Vector2(310f, 58f);
        trt.anchorMin = new Vector2(0.5f, 0.5f);
        trt.anchorMax = new Vector2(0.5f, 0.5f);
        trt.anchoredPosition = new Vector2(0f, 6f);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 31f;
        tmp.color = btnTextNormal;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        ApplyTextStyle(tmp, 0.2f, 0.12f);

        // 按下效果（文字變暗 + 往下位移）
        RPGButtonEffect effect = btnObj.AddComponent<RPGButtonEffect>();
        effect.textTransform = trt;
        effect.textComponent = tmp;
        effect.normalColor = btnTextNormal;
        effect.pressedColor = btnTextPressed;
        effect.buttonImage = img;
        effect.normalSprite = btnNormal;
        effect.pressedSprite = btnPressed;
        effect.normalButtonColor = img.color;
        effect.pressedButtonColor = new Color(0.22f, 0.15f, 0.06f, 1f);

        // 綁定功能
        int capturedIndex = index;
        btn.onClick.AddListener(() => OnButtonClick(capturedIndex));
    }

    void ApplyTextStyle(TextMeshProUGUI tmp, float outlineWidth, float underlaySoftness)
    {
        tmp.fontMaterial = new Material(tmp.fontSharedMaterial);
        tmp.enableVertexGradient = true;
        tmp.colorGradient = new VertexGradient(
            new Color(1f, 0.96f, 0.66f, 1f),
            new Color(1f, 0.86f, 0.34f, 1f),
            new Color(0.82f, 0.58f, 0.14f, 1f),
            new Color(0.82f, 0.58f, 0.14f, 1f)
        );
        tmp.outlineColor = new Color(0.22f, 0.12f, 0.02f, 1f);
        tmp.outlineWidth = outlineWidth;
        tmp.fontMaterial.EnableKeyword("UNDERLAY_ON");
        tmp.fontMaterial.SetColor("_UnderlayColor", new Color(0f, 0f, 0f, 0.75f));
        tmp.fontMaterial.SetFloat("_UnderlayOffsetX", 0.8f);
        tmp.fontMaterial.SetFloat("_UnderlayOffsetY", -0.8f);
        tmp.fontMaterial.SetFloat("_UnderlaySoftness", underlaySoftness);
    }

    void OnButtonClick(int index)
    {
        switch (index)
        {
            case 0: gameManager?.ResumeGameFromPause(); break;
            case 1: gameManager?.RestartGame(); break;
            case 2: Debug.Log("遊戲設定（待實作）"); break;
            case 3: gameManager?.ReturnToMainMenu(); break;
            case 4: gameManager?.QuitGame(); break;
        }
    }

    void UpdateStatsPanel()
    {
        if (statsBodyText == null) return;

        PlayerStats stats = FindAnyObjectByType<PlayerStats>();
        if (stats == null)
        {
            statsBodyText.text = "找不到玩家數據";
            return;
        }

        string labelColor = ColorUtility.ToHtmlStringRGB(statsLabelColor);
        string valueColor = ColorUtility.ToHtmlStringRGB(statsValueColor);

        statsBodyText.text =
            StatLine("生命值", FormatNumber(stats.currentHp) + " / " + FormatNumber(stats.maxHp), labelColor, valueColor) +
            StatLine("攻擊力", FormatNumber(stats.baseDamage), labelColor, valueColor) +
            StatLine("攻速", stats.attackSpeed.ToString("0.##") + " /s", labelColor, valueColor) +
            StatLine("攻距", stats.attackRange.ToString("0.#") + " m", labelColor, valueColor) +
            StatLine("箭矢", stats.arrowCount.ToString(), labelColor, valueColor) +
            StatLine("爆率", FormatPercent(stats.critRate), labelColor, valueColor) +
            StatLine("爆傷", "x" + stats.critDamage.ToString("0.##"), labelColor, valueColor) +
            StatLine("重砲強化", stats.GetArrowStyleCount(PlayerStats.ArrowStyle.Attack).ToString(), labelColor, valueColor) +
            StatLine("輕弩強化", stats.GetArrowStyleCount(PlayerStats.ArrowStyle.Speed).ToString(), labelColor, valueColor) +
            StatLine("多重箭強化", stats.GetArrowStyleCount(PlayerStats.ArrowStyle.Multi).ToString(), labelColor, valueColor) +
            StatLine("吸血", "Lv" + stats.lifestealLevel + "  " + FormatPercent(stats.lifestealLevel * 0.05f), labelColor, valueColor) +
            StatLine("抗撞", "Lv" + stats.collisionResistLevel + "  " + FormatPercent(stats.collisionResistLevel * 0.1f), labelColor, valueColor) +
            StatLine("磁鐵", "Lv" + stats.magnetLevel + "  +" + (stats.magnetLevel * 3).ToString() + "m", labelColor, valueColor);
    }

    string StatLine(string label, string value, string labelColor, string valueColor)
    {
        return "<color=#" + labelColor + ">" + label + "</color><pos=54%><color=#" + valueColor + ">" + value + "</color>\n";
    }

    string FormatPercent(float value)
    {
        return (value * 100f).ToString("0.#") + "%";
    }

    string FormatNumber(float value)
    {
        if (value >= 1000000000f) return (value / 1000000000f).ToString("0.##") + "B";
        if (value >= 1000000f) return (value / 1000000f).ToString("0.##") + "M";
        if (value >= 1000f) return (value / 1000f).ToString("0.##") + "K";
        return Mathf.FloorToInt(value).ToString();
    }

    // === 動畫控制 ===
    public void Show(Vector3 fromPosition)
    {
        if (pausePanel == null) return;

        UpdateStatsPanel();
        pausePanel.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(AnimateShow());
    }

    public void Hide(Vector3 toPosition)
    {
        if (pausePanel == null) return;

        StopAllCoroutines();
        StartCoroutine(AnimateHide());
    }

    public void HideImmediate()
    {
        if (pausePanel == null) return;

        StopAllCoroutines();
        if (panelRect != null)
        {
            FitPanelToScreen();
            panelRect.localScale = Vector3.one * fittedScale;
            panelRect.anchoredPosition = Vector2.zero;
        }
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        pausePanel.SetActive(false);
    }

    IEnumerator AnimateShow()
    {
        float elapsed = 0f;
        float duration = 0.3f;

        FitPanelToScreen();
        canvasGroup.alpha = 0f;
        panelRect.localScale = Vector3.one * fittedScale * 0.84f;
        panelRect.anchoredPosition = Vector2.zero;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = EaseOutBack(t);

            panelRect.localScale = Vector3.one * Mathf.Lerp(fittedScale * 0.84f, fittedScale, eased);
            panelRect.anchoredPosition = Vector2.zero;
            canvasGroup.alpha = t;
            yield return null;
        }

        panelRect.localScale = Vector3.one * fittedScale;
        panelRect.anchoredPosition = Vector2.zero;
        canvasGroup.alpha = 1f;
    }

    IEnumerator AnimateHide()
    {
        float elapsed = 0f;
        float duration = 0.25f;

        FitPanelToScreen();
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = EaseInBack(t);

            panelRect.localScale = Vector3.one * Mathf.Lerp(fittedScale, fittedScale * 0.84f, eased);
            panelRect.anchoredPosition = Vector2.zero;
            canvasGroup.alpha = 1f - t;
            yield return null;
        }

        panelRect.localScale = Vector3.one * fittedScale * 0.84f;
        panelRect.anchoredPosition = Vector2.zero;
        canvasGroup.alpha = 0f;
        pausePanel.SetActive(false);
    }

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

// 按鈕按下效果：文字往下沉 + 變暗
public class RPGButtonEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public RectTransform textTransform;
    public TextMeshProUGUI textComponent;
    public Color normalColor;
    public Color pressedColor;
    public Image buttonImage;
    public Sprite normalSprite;
    public Sprite pressedSprite;
    public Color normalButtonColor;
    public Color pressedButtonColor;

    private Vector2 originalTextPos;
    private const float pressedOffsetY = -9f;

    void Start()
    {
        if (textTransform != null)
            originalTextPos = textTransform.anchoredPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (textTransform != null)
            textTransform.anchoredPosition = originalTextPos + new Vector2(0f, pressedOffsetY);
        if (textComponent != null)
            textComponent.color = pressedColor;
        if (buttonImage != null)
        {
            if (pressedSprite != null) buttonImage.sprite = pressedSprite;
            else buttonImage.color = pressedButtonColor;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ResetText();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetText();
    }

    void OnDisable()
    {
        ResetText();
    }

    private void ResetText()
    {
        if (textTransform != null)
            textTransform.anchoredPosition = originalTextPos;
        if (textComponent != null)
            textComponent.color = normalColor;
        if (buttonImage != null)
        {
            if (normalSprite != null) buttonImage.sprite = normalSprite;
            buttonImage.color = normalButtonColor;
        }
    }
}
