using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI 面板綁定")]
    public GameObject settingsPanel;
    public TextMeshProUGUI bestRecordText;

    private Transform uiRoot;
    private GameObject historyPanel;
    private CanvasGroup historyCanvasGroup;
    private Transform recordsContent;

    private readonly Color deepWood = new Color(0.17f, 0.10f, 0.05f, 0.96f);
    private readonly Color warmWood = new Color(0.38f, 0.23f, 0.10f, 0.95f);
    private readonly Color gold = new Color(1f, 0.83f, 0.22f, 1f);
    private readonly Color softText = new Color(1f, 0.92f, 0.72f, 1f);
    private readonly Color historyBg = new Color(0.16f, 0.10f, 0.04f, 0.98f);
    private readonly Color historyLine = new Color(0.86f, 0.62f, 0.16f, 0.75f);

    void Start()
    {
        BuildMainMenu();
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(1);
    }

    public void OpenSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void OpenHistory()
    {
        if (historyPanel == null && uiRoot != null)
        {
            CreateHistoryPanel(uiRoot);
        }
        if (historyPanel == null) return;

        historyPanel.transform.SetAsLastSibling();
        SetHistoryVisible(true);
        RefreshRecords();
        Canvas.ForceUpdateCanvases();
    }

    public void CloseHistory()
    {
        SetHistoryVisible(false);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void ClearRecords()
    {
        GameRecordStore.ClearRecords();
        PlayerPrefs.DeleteKey("BestDistance");
        PlayerPrefs.DeleteKey("BestKills");
        PlayerPrefs.Save();
        RefreshRecords();
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void BuildMainMenu()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Canvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<GraphicRaycaster>();
            canvasObject.AddComponent<CanvasScaler>();
        }

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        for (int i = canvas.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(canvas.transform.GetChild(i).gameObject);
        }

        GameObject rootObject = CreateUIObject("MainMenuRoot", canvas.transform);
        uiRoot = rootObject.transform;
        RectTransform rootRect = rootObject.GetComponent<RectTransform>();
        Stretch(rootRect);

        Image backdrop = rootObject.AddComponent<Image>();
        backdrop.color = new Color(0.06f, 0.11f, 0.08f, 0.82f);

        CreateForestBands(rootObject.transform);

        GameObject menuPanel = CreatePanel("MenuPanel", rootObject.transform, new Vector2(500f, 610f), Vector2.zero, deepWood);
        CreateTitle(menuPanel.transform);
        CreateBestSummary(menuPanel.transform);
        CreateMenuButton(menuPanel.transform, "開始遊戲", new Vector2(0f, 80f), PlayGame);
        CreateMenuButton(menuPanel.transform, "歷史紀錄", new Vector2(0f, 0f), OpenHistory);
        CreateMenuButton(menuPanel.transform, "遊戲設定", new Vector2(0f, -80f), OpenSettings);
        CreateMenuButton(menuPanel.transform, "退出遊戲", new Vector2(0f, -160f), QuitGame);

        CreateHistoryPanel(rootObject.transform);
        CreateSettingsPanel(rootObject.transform);
        RefreshRecords();
    }

    private GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>();
        return obj;
    }

    private GameObject CreatePanel(string name, Transform parent, Vector2 size, Vector2 position, Color color)
    {
        GameObject panel = CreateUIObject(name, parent);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        Image image = panel.AddComponent<Image>();
        image.color = color;

        Outline outline = panel.AddComponent<Outline>();
        outline.effectColor = new Color(0.82f, 0.62f, 0.32f, 0.9f);
        outline.effectDistance = new Vector2(3f, -3f);

        return panel;
    }

    private void CreateForestBands(Transform parent)
    {
        CreateBand(parent, new Vector2(0f, 265f), new Vector2(1280f, 190f), new Color(0.23f, 0.36f, 0.30f, 0.6f));
        CreateBand(parent, new Vector2(0f, -270f), new Vector2(1280f, 190f), new Color(0.11f, 0.22f, 0.13f, 0.72f));
    }

    private void CreateBand(Transform parent, Vector2 position, Vector2 size, Color color)
    {
        GameObject band = CreateUIObject("AtmosphereBand", parent);
        RectTransform rect = band.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        Image image = band.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
    }

    private void CreateTitle(Transform parent)
    {
        TextMeshProUGUI title = CreateText(parent, "Title", "FOREST RUN", new Vector2(0f, 210f), new Vector2(440f, 82f), 50f, gold);
        title.characterSpacing = 3f;
        title.fontStyle = FontStyles.Bold;

        TextMeshProUGUI subtitle = CreateText(parent, "Subtitle", "荒野弓手生存戰", new Vector2(0f, 152f), new Vector2(340f, 40f), 26f, softText);
        subtitle.fontStyle = FontStyles.Bold;
    }

    private void CreateBestSummary(Transform parent)
    {
        List<GameRecord> records = GameRecordStore.LoadRecords();
        string summary = "尚無紀錄";
        if (records.Count > 0)
        {
            GameRecord best = records[0];
            summary = $"最佳分數 {FormatNumber(best.distance)}m  |  擊殺 {FormatNumber(best.kills)}";
        }

        bestRecordText = CreateText(parent, "BestRecord", summary, new Vector2(0f, -238f), new Vector2(390f, 42f), 24f, softText);
        bestRecordText.fontStyle = FontStyles.Bold;
    }

    private void CreateHistoryPanel(Transform parent)
    {
        historyPanel = CreatePanel("HistoryPanel", parent, new Vector2(560f, 660f), Vector2.zero, historyBg);
        historyPanel.transform.SetAsLastSibling();
        historyCanvasGroup = historyPanel.AddComponent<CanvasGroup>();

        CreateRecordsHeader(historyPanel.transform);
        CreateRecordsScroll(historyPanel.transform);
        CreateDivider(historyPanel.transform, new Vector2(0f, -242f), new Vector2(500f, 2f), 0.34f);
        CreateMenuButton(historyPanel.transform, "清除紀錄", new Vector2(186f, -288f), ClearRecords, new Vector2(142f, 48f), new Color(0.50f, 0.17f, 0.09f, 0.96f));
        CreateMenuButton(historyPanel.transform, "關閉", new Vector2(208f, 284f), CloseHistory, new Vector2(96f, 42f), warmWood);

        SetHistoryVisible(false);
    }

    private void SetHistoryVisible(bool visible)
    {
        if (historyCanvasGroup == null) return;

        historyCanvasGroup.alpha = visible ? 1f : 0f;
        historyCanvasGroup.interactable = visible;
        historyCanvasGroup.blocksRaycasts = visible;
    }

    private void CreateRecordsHeader(Transform parent)
    {
        TextMeshProUGUI title = CreateText(parent, "HistoryTitle", "歷史紀錄", new Vector2(0f, 278f), new Vector2(260f, 48f), 30f, gold);
        title.fontStyle = FontStyles.Bold;

        CreateDivider(parent, new Vector2(0f, 238f), new Vector2(500f, 2f), 0.7f);
        CreateDivider(parent, new Vector2(0f, 206f), new Vector2(500f, 1f), 0.42f);
        CreateDiamond(parent, new Vector2(0f, 238f));

        CreateText(parent, "ColumnRank", "排名", new Vector2(-226f, 217f), new Vector2(52f, 28f), 17f, gold);
        CreateText(parent, "ColumnScore", "距離", new Vector2(-142f, 217f), new Vector2(82f, 28f), 17f, gold);
        CreateText(parent, "ColumnDate", "日期", new Vector2(-12f, 217f), new Vector2(112f, 28f), 17f, gold);
        CreateText(parent, "ColumnKills", "擊殺", new Vector2(132f, 217f), new Vector2(70f, 28f), 17f, gold);
        CreateText(parent, "ColumnTime", "時間", new Vector2(190f, 217f), new Vector2(60f, 28f), 17f, gold);
        CreateText(parent, "ColumnRewards", "獎勵", new Vector2(252f, 217f), new Vector2(70f, 28f), 17f, gold);
    }

    private void CreateRecordsScroll(Transform parent)
    {
        GameObject viewport = CreateUIObject("RecordsViewport", parent);
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = new Vector2(0.5f, 0.5f);
        viewportRect.anchorMax = new Vector2(0.5f, 0.5f);
        viewportRect.sizeDelta = new Vector2(505f, 430f);
        viewportRect.anchoredPosition = new Vector2(0f, -24f);

        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = new Color(0.12f, 0.075f, 0.03f, 0.34f);
        viewport.AddComponent<Mask>().showMaskGraphic = true;

        GameObject content = CreateUIObject("RecordsContent", viewport.transform);
        recordsContent = content.transform;
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0f, 0f);

        VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.spacing = 7f;
        layout.padding = new RectOffset(0, 0, 8, 8);
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scroll = viewport.AddComponent<ScrollRect>();
        scroll.content = contentRect;
        scroll.viewport = viewportRect;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 32f;
    }

    private void RefreshRecords()
    {
        if (recordsContent == null) return;

        for (int i = recordsContent.childCount - 1; i >= 0; i--)
        {
            Destroy(recordsContent.GetChild(i).gameObject);
        }

        List<GameRecord> records = GameRecordStore.LoadRecords();
        if (records.Count == 0)
        {
            CreateRecordEmptyState();
            if (bestRecordText != null) bestRecordText.text = "尚無紀錄";
            LayoutRebuilder.ForceRebuildLayoutImmediate(recordsContent.GetComponent<RectTransform>());
            return;
        }

        if (bestRecordText != null)
        {
            GameRecord best = records[0];
            bestRecordText.text = $"最佳分數 {FormatNumber(best.distance)}m  |  擊殺 {FormatNumber(best.kills)}";
        }

        for (int i = 0; i < records.Count; i++)
        {
            CreateRecordRow(records[i], i + 1);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(recordsContent.GetComponent<RectTransform>());
    }

    private void CreateSettingsPanel(Transform parent)
    {
        settingsPanel = CreatePanel("SettingsPanel", parent, new Vector2(420f, 270f), Vector2.zero, new Color(0.11f, 0.08f, 0.04f, 0.98f));
        settingsPanel.transform.SetAsLastSibling();

        CreateText(settingsPanel.transform, "SettingsTitle", "遊戲設定", new Vector2(0f, 78f), new Vector2(320f, 50f), 34f, gold).fontStyle = FontStyles.Bold;
        TextMeshProUGUI message = CreateText(settingsPanel.transform, "SettingsMessage", "設定功能保留中", new Vector2(0f, 10f), new Vector2(320f, 42f), 24f, softText);
        message.fontStyle = FontStyles.Bold;
        CreateMenuButton(settingsPanel.transform, "關閉", new Vector2(0f, -80f), CloseSettings, new Vector2(180f, 54f), warmWood);

        settingsPanel.SetActive(false);
    }

    private void CreateRecordEmptyState()
    {
        GameObject row = CreateUIObject("EmptyRecord", recordsContent);
        LayoutElement layout = row.AddComponent<LayoutElement>();
        layout.preferredHeight = 96f;
        TextMeshProUGUI text = row.AddComponent<TextMeshProUGUI>();
        text.text = "還沒有歷史紀錄";
        text.fontSize = 28f;
        text.color = new Color(1f, 0.9f, 0.68f, 0.86f);
        text.alignment = TextAlignmentOptions.Center;
    }

    private void CreateRecordRow(GameRecord record, int rank)
    {
        GameObject row = CreateUIObject("RecordRow_" + rank, recordsContent);
        LayoutElement layout = row.AddComponent<LayoutElement>();
        layout.preferredHeight = 38f;

        Image bg = row.AddComponent<Image>();
        bg.color = RowColor(rank);
        bg.raycastTarget = false;

        Outline outline = row.AddComponent<Outline>();
        outline.effectColor = RowBorderColor(rank);
        outline.effectDistance = new Vector2(1f, -1f);

        CreateText(row.transform, "Rank", rank.ToString(), new Vector2(-226f, 0f), new Vector2(52f, 32f), 19f, RankColor(rank));
        CreateText(row.transform, "Distance", FormatNumber(record.distance) + "m", new Vector2(-142f, 0f), new Vector2(82f, 32f), 19f, new Color(1f, 0.90f, 0.55f, 1f)).fontStyle = FontStyles.Bold;
        CreateText(row.transform, "Date", ShortDate(record.date), new Vector2(-12f, 0f), new Vector2(112f, 32f), 15f, new Color(0.83f, 0.72f, 0.52f, 1f));
        CreateText(row.transform, "Kills", FormatNumber(record.kills), new Vector2(132f, 0f), new Vector2(70f, 32f), 17f, new Color(1f, 0.58f, 0.22f, 1f));
        CreateText(row.transform, "PlayTime", FormatPlayTime(record.playTimeSeconds), new Vector2(190f, 0f), new Vector2(58f, 32f), 15f, new Color(0.62f, 0.83f, 0.48f, 1f));
        CreateRewardIcons(row.transform, record.rewardIcons);
    }

    private void CreateRewardIcons(Transform parent, string rewardIcons)
    {
        if (string.IsNullOrEmpty(rewardIcons)) return;

        string[] keys = rewardIcons.Split(',');
        int visibleCount = Mathf.Min(keys.Length, 4);
        for (int i = 0; i < visibleCount; i++)
        {
            string key = keys[i].Trim();
            if (string.IsNullOrEmpty(key)) continue;

            Sprite sprite = Resources.Load<Sprite>("RewardIcons/" + key);
            if (sprite == null)
            {
                Texture2D texture = Resources.Load<Texture2D>("RewardIcons/" + key);
                if (texture != null)
                {
                    sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
                }
            }
            if (sprite == null) continue;

            GameObject icon = CreateUIObject("RewardIcon_" + key, parent);
            RectTransform rect = icon.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(24f, 24f);
            rect.anchoredPosition = new Vector2(222f + i * 22f, 0f);

            Image image = icon.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
        }
    }

    private void CreateDivider(Transform parent, Vector2 position, Vector2 size, float alpha)
    {
        GameObject divider = CreateUIObject("Divider", parent);
        RectTransform rect = divider.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        Image image = divider.AddComponent<Image>();
        image.color = new Color(historyLine.r, historyLine.g, historyLine.b, alpha);
        image.raycastTarget = false;
    }

    private void CreateDiamond(Transform parent, Vector2 position)
    {
        GameObject diamond = CreateUIObject("Diamond", parent);
        RectTransform rect = diamond.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(12f, 12f);
        rect.anchoredPosition = position;
        rect.localRotation = Quaternion.Euler(0f, 0f, 45f);
        Image image = diamond.AddComponent<Image>();
        image.color = gold;
        image.raycastTarget = false;
    }

    private Color RowColor(int rank)
    {
        if (rank == 1) return new Color(0.56f, 0.34f, 0.10f, 0.86f);
        if (rank == 2) return new Color(0.23f, 0.23f, 0.22f, 0.90f);
        if (rank == 3) return new Color(0.34f, 0.18f, 0.06f, 0.86f);
        return new Color(0.24f, 0.15f, 0.04f, 0.76f);
    }

    private Color RowBorderColor(int rank)
    {
        if (rank == 1) return new Color(1f, 0.74f, 0.08f, 0.95f);
        if (rank == 2) return new Color(0.78f, 0.78f, 0.72f, 0.7f);
        if (rank == 3) return new Color(0.9f, 0.45f, 0.12f, 0.72f);
        return new Color(0.45f, 0.32f, 0.09f, 0.55f);
    }

    private Color RankColor(int rank)
    {
        if (rank == 1) return new Color(1f, 0.25f, 0.18f, 1f);
        if (rank == 2) return new Color(1f, 0.58f, 0.18f, 1f);
        if (rank == 3) return new Color(1f, 0.84f, 0.25f, 1f);
        return Color.white;
    }

    private string ShortDate(string date)
    {
        if (string.IsNullOrEmpty(date)) return "--";
        return date.Length >= 10 ? date.Substring(0, 10) : date;
    }

    private TextMeshProUGUI CreateText(Transform parent, string name, string value, Vector2 position, Vector2 size, float fontSize, Color color)
    {
        GameObject obj = CreateUIObject(name, parent);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.raycastTarget = false;
        ApplyTextStyle(text, 0.12f);
        return text;
    }

    private void CreateMenuButton(Transform parent, string label, Vector2 position, UnityEngine.Events.UnityAction action)
    {
        CreateMenuButton(parent, label, position, action, new Vector2(280f, 66f), warmWood);
    }

    private void CreateMenuButton(Transform parent, string label, Vector2 position, UnityEngine.Events.UnityAction action, Vector2 size, Color color)
    {
        GameObject buttonObject = CreateUIObject("Button_" + label, parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        Image image = buttonObject.AddComponent<Image>();
        image.color = color;

        Button button = buttonObject.AddComponent<Button>();
        button.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.9f, 0.62f, 1f);
        colors.pressedColor = new Color(0.75f, 0.55f, 0.32f, 1f);
        colors.disabledColor = new Color(0.34f, 0.28f, 0.22f, 0.7f);
        colors.fadeDuration = 0.08f;
        button.colors = colors;
        button.onClick.AddListener(action);

        Outline outline = buttonObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.65f);
        outline.effectDistance = new Vector2(2f, -2f);

        TextMeshProUGUI text = CreateText(buttonObject.transform, "Label", label, Vector2.zero, new Vector2(size.x - 22f, size.y - 10f), 28f, softText);
        text.fontStyle = FontStyles.Bold;
    }

    private void ApplyTextStyle(TextMeshProUGUI text, float outlineWidth)
    {
        text.fontMaterial = new Material(text.fontSharedMaterial);
        text.outlineColor = new Color(0.12f, 0.07f, 0.02f, 1f);
        text.outlineWidth = outlineWidth;
        text.fontMaterial.EnableKeyword("UNDERLAY_ON");
        text.fontMaterial.SetColor("_UnderlayColor", new Color(0f, 0f, 0f, 0.55f));
        text.fontMaterial.SetFloat("_UnderlayOffsetX", 0.7f);
        text.fontMaterial.SetFloat("_UnderlayOffsetY", -0.7f);
        text.fontMaterial.SetFloat("_UnderlaySoftness", 0.14f);
    }

    private void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private string FormatPlayTime(float seconds)
    {
        int totalSeconds = Mathf.Max(0, Mathf.RoundToInt(seconds));
        int minutes = totalSeconds / 60;
        int remainSeconds = totalSeconds % 60;
        return $"{minutes:00}:{remainSeconds:00}";
    }

    public string FormatNumber(float number)
    {
        if (number >= 1000000000) return (number / 1000000000f).ToString("0.##") + "B";
        if (number >= 1000000) return (number / 1000000f).ToString("0.##") + "M";
        if (number >= 1000) return (number / 1000f).ToString("0.##") + "K";
        return Mathf.FloorToInt(number).ToString();
    }
}
