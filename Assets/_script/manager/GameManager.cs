using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    [Header("結算 UI")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalDistanceText;
    public TextMeshProUGUI finalKillText;
    public Sprite gameOverButtonPressedSprite;
    private TextMeshProUGUI gameOverTitleText;
    private bool gameOverUIStyled = false;
    private bool rewardUIStyled = false;
    private bool isGameOver = false;
    private float runStartRealtime;

    [Header("Boss 獎勵 UI")]
    public GameObject rewardPanel;
    public TextMeshProUGUI btnAText;
    public TextMeshProUGUI btnBText;
    public TextMeshProUGUI btnCText;
    public GameObject buttonCObject;

    [Header("暫停與生成")]
    public GameObject pausePanel;
    public PanelAnimator pausePanelAnimator;
    private bool isPaused = false;
    public float nextBossDistance = 450f;
    public float bossInterval = 450f;
    private int bossSpawnCount = 0;
    public float bossOffset = 10f;
    public GameObject[] smallBossPrefabs;
    public GameObject bigBossPrefab;
    public float bossSpawnDistance = 55f;
    public float bigBossExtraSpawnDistance = 18f;
    public GameObject smallBossClawProjectilePrefab;

    private bool isBigBossReward = false;
    private PlayerStats playerStats;
    private bool isSpawning = false;
    private bool enemyPlusOneWaitingForBoss = false;
    private float enemyPlusOneTrackedBossZ = -1f;
    private const float enemyPlusOneBossPassDistance = 30f;
    private int smallBossAttackPatternIndex = 0;
    private int smallBossEncounterCount = 0;

    [Header("Boss 獎勵箱")]
    public GameObject bossRewardChestPrefab;

    [Header("設定按鈕")]
    public Image settingsButtonImage;  // 拖入按鈕的 Image 組件
    public Sprite spriteNormal;        // 正常狀態的圖
    public Sprite spritePaused;        // 暫停狀態的圖

    [Header("暫停面板")]
    public PauseMenuUI pauseMenuUI; // 🌟 拖入掛有 PauseMenuUI 的物件
    public RectTransform settingsButtonRect; // 🌟 拖入右上角設定按鈕

    // 🌟 這裡改成 EnemyPlusOne
    public enum RewardType { Light, Heavy, Multi, BigAtk, BigSpd, BigHp, Lifesteal, Resist, EnemyPlusOne, Magnet }
    private List<RewardType> currentOptions = new List<RewardType>();
    private List<string> collectedRewardIcons = new List<string>();

    void Start()
    {
        runStartRealtime = Time.realtimeSinceStartup;

        playerStats = FindAnyObjectByType<PlayerStats>();
        SetupGameOverUI();
        SetupRewardUI();
    }

    void Update()
    {
        if (isGameOver) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!gameOverPanel.activeSelf && !rewardPanel.activeSelf)
            {
                if (isPaused) ResumeGameFromPause();
                else PauseGame();
            }
        }

        if (playerStats != null && !isSpawning)
        {
            float playerDistance = playerStats.transform.position.z - 30f;
            if (playerDistance >= nextBossDistance - bossSpawnDistance)
            {
                isSpawning = true;
                bossSpawnCount++;
                bool isBigBoss = (bossSpawnCount % 4 == 0);
                float bossWorldZ = (nextBossDistance + bossOffset) + 35f;
                if (isBigBoss) bossWorldZ += bigBossExtraSpawnDistance;
                SpawnBoss(isBigBoss, bossWorldZ);
                nextBossDistance += bossInterval;
                Invoke("ResetSpawnLock", 2.0f);
            }
        }

        CheckEnemyPlusOneBossPassed();
    }

    private void ResetSpawnLock() { isSpawning = false; }
    public void PauseGame()
    {
        if (isGameOver) return;

        isPaused = true;
        Time.timeScale = 0f;
        pauseMenuUI.Show(settingsButtonRect.position); // 🌟 從按鈕位置展開
        if (settingsButtonImage != null) settingsButtonImage.sprite = spritePaused;
    }


    public void ResumeGameFromPause()
    {
        if (isGameOver) return;

        isPaused = false;
        Time.timeScale = 1f;
        pauseMenuUI.Hide(settingsButtonRect.position); // 🌟 縮回按鈕位置
        if (settingsButtonImage != null) settingsButtonImage.sprite = spriteNormal;
    }

    public void TogglePause()
    {
        if (isGameOver) return;

        if (isPaused) ResumeGameFromPause();
        else PauseGame();
    }
    public void ReturnToMainMenu() { Time.timeScale = 1f; SceneManager.LoadScene(0); }
    public void QuitGame() { Application.Quit(); }

    public void ShowGameOver(int distance, int kills)
    {
        if (isGameOver) return;

        SetupGameOverUI();
        isGameOver = true;
        isPaused = false;
        if (pauseMenuUI != null) pauseMenuUI.HideImmediate();
        if (settingsButtonImage != null) settingsButtonImage.sprite = spriteNormal;

        gameOverPanel.SetActive(true);
        finalDistanceText.text = "最終距離: " + FormatNumber(distance) + " m";
        finalKillText.text = "總擊殺數: " + FormatNumber(kills);
        int bestDist = PlayerPrefs.GetInt("BestDistance", 0);
        if (distance > bestDist) PlayerPrefs.SetInt("BestDistance", distance);
        int bestKill = PlayerPrefs.GetInt("BestKills", 0);
        if (kills > bestKill) PlayerPrefs.SetInt("BestKills", kills);
        GameRecordStore.AddRecord(distance, kills, Time.realtimeSinceStartup - runStartRealtime, string.Join(",", collectedRewardIcons));
        PlayerPrefs.Save();
        Time.timeScale = 0f;
    }

    private void SetupGameOverUI()
    {
        if (gameOverUIStyled || gameOverPanel == null) return;

        RectTransform panelRect = gameOverPanel.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(760f, 540f);
        }

        TextMeshProUGUI[] texts = gameOverPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (TextMeshProUGUI text in texts)
        {
            if (text.text == "GAME OVER")
            {
                gameOverTitleText = text;
                break;
            }
        }

        StyleGameOverTitle();
        StyleGameOverStats();
        StyleGameOverButtons();
        gameOverUIStyled = true;
    }

    private void StyleGameOverTitle()
    {
        if (gameOverTitleText == null) return;

        RectTransform titleRect = gameOverTitleText.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = new Vector2(0f, 188f);
        titleRect.sizeDelta = new Vector2(700f, 82f);

        gameOverTitleText.fontSize = 64f;
        gameOverTitleText.characterSpacing = 4f;
        gameOverTitleText.alignment = TextAlignmentOptions.Center;
        gameOverTitleText.fontStyle = FontStyles.Bold;
        gameOverTitleText.enableVertexGradient = true;
        gameOverTitleText.colorGradient = new VertexGradient(
            new Color(1f, 0.26f, 0.14f, 1f),
            new Color(1f, 0.18f, 0.08f, 1f),
            new Color(0.55f, 0f, 0f, 1f),
            new Color(0.55f, 0f, 0f, 1f)
        );
        ApplyTMPOutline(gameOverTitleText, new Color(0.05f, 0.01f, 0f, 1f), 0.22f, 0.85f);
    }

    private void StyleGameOverStats()
    {
        HideGameOverCard("DistanceStatCard");
        HideGameOverCard("KillStatCard");

        StyleStatText(finalDistanceText, new Vector2(0f, 78f));
        StyleStatText(finalKillText, new Vector2(0f, 24f));
        StyleStatsDivider();
    }

    private void StyleStatText(TextMeshProUGUI text, Vector2 position)
    {
        if (text == null) return;

        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = position;
        textRect.sizeDelta = new Vector2(520f, 44f);
        textRect.SetAsLastSibling();

        text.raycastTarget = false;
        text.fontSize = 31f;
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;
        text.color = new Color(1f, 0.86f, 0.28f, 1f);
        ApplyTMPOutline(text, new Color(0.18f, 0.1f, 0.02f, 1f), 0.12f, 0.45f);
    }

    private void HideGameOverCard(string cardName)
    {
        Transform existing = gameOverPanel.transform.Find(cardName);
        if (existing != null)
        {
            existing.gameObject.SetActive(false);
        }
    }

    private void StyleStatsDivider()
    {
        const string dividerName = "StatsDivider";
        Transform existing = gameOverPanel.transform.Find(dividerName);
        GameObject dividerObject = existing != null ? existing.gameObject : new GameObject(dividerName);
        dividerObject.SetActive(true);
        dividerObject.transform.SetParent(gameOverPanel.transform, false);

        RectTransform dividerRect = dividerObject.GetComponent<RectTransform>();
        if (dividerRect == null) dividerRect = dividerObject.AddComponent<RectTransform>();
        dividerRect.anchorMin = new Vector2(0.5f, 0.5f);
        dividerRect.anchorMax = new Vector2(0.5f, 0.5f);
        dividerRect.pivot = new Vector2(0.5f, 0.5f);
        dividerRect.anchoredPosition = new Vector2(0f, 51f);
        dividerRect.sizeDelta = new Vector2(360f, 2f);

        Image dividerImage = dividerObject.GetComponent<Image>();
        if (dividerImage == null) dividerImage = dividerObject.AddComponent<Image>();
        dividerImage.color = new Color(0.36f, 0.19f, 0.06f, 0.24f);
        dividerImage.raycastTarget = false;
        if (finalKillText != null)
        {
            dividerRect.SetSiblingIndex(Mathf.Max(0, finalKillText.transform.GetSiblingIndex() - 1));
        }
    }

    private void StyleGameOverButtons()
    {
        Button[] buttons = gameOverPanel.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            RectTransform buttonRect = button.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.sizeDelta = new Vector2(340f, 70f);
            buttonRect.anchoredPosition = new Vector2(0f, -82f - (i * 88f));
            button.transition = Selectable.Transition.None;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.96f, 0.82f, 1f);
            colors.pressedColor = new Color(0.78f, 0.66f, 0.45f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(0.45f, 0.39f, 0.32f, 0.75f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label == null) continue;

            RectTransform labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.sizeDelta = new Vector2(310f, 56f);
            labelRect.anchoredPosition = new Vector2(0f, 4f);

            label.fontSize = 34f;
            label.alignment = TextAlignmentOptions.Center;
            label.fontStyle = FontStyles.Bold;
            label.color = new Color(1f, 0.86f, 0.28f, 1f);
            ApplyTMPOutline(label, new Color(0.13f, 0.07f, 0.01f, 1f), 0.13f, 0.4f);

            ButtonTextPress pressEffect = button.GetComponent<ButtonTextPress>();
            if (pressEffect == null)
            {
                pressEffect = button.gameObject.AddComponent<ButtonTextPress>();
            }

            if (pressEffect != null)
            {
                pressEffect.textRectTransform = labelRect;
                pressEffect.buttonImage = button.image;
                pressEffect.normalSprite = button.image != null ? button.image.sprite : null;
                pressEffect.pressedSprite = gameOverButtonPressedSprite;
                pressEffect.RefreshOriginalState();
            }
        }
    }

    private void ApplyTMPOutline(TextMeshProUGUI text, Color outlineColor, float outlineWidth, float shadowAlpha)
    {
        if (text == null) return;

        Material outlineMaterial = null;
        try
        {
            Material baseMaterial = text.fontSharedMaterial;
            if (baseMaterial != null)
            {
                outlineMaterial = new Material(baseMaterial);
            }
        }
        catch (System.ArgumentNullException)
        {
            outlineMaterial = null;
        }

        if (outlineMaterial == null)
        {
            text.outlineColor = outlineColor;
            text.outlineWidth = outlineWidth;
            return;
        }

        text.fontMaterial = outlineMaterial;
        text.outlineColor = outlineColor;
        text.outlineWidth = outlineWidth;
        text.fontMaterial.EnableKeyword("UNDERLAY_ON");
        text.fontMaterial.SetColor("_UnderlayColor", new Color(0f, 0f, 0f, shadowAlpha));
        text.fontMaterial.SetFloat("_UnderlayOffsetX", 0.9f);
        text.fontMaterial.SetFloat("_UnderlayOffsetY", -0.9f);
        text.fontMaterial.SetFloat("_UnderlaySoftness", 0.18f);
    }

    private void SetupRewardUI()
    {
        if (rewardUIStyled || rewardPanel == null) return;

        RectTransform overlayRect = rewardPanel.GetComponent<RectTransform>();
        if (overlayRect != null)
        {
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
        }

        Image overlayImage = rewardPanel.GetComponent<Image>();
        if (overlayImage != null)
        {
            overlayImage.color = new Color(0.05f, 0.045f, 0.035f, 0.78f);
        }

        RectTransform boardRect = FindRewardBoard();
        if (boardRect != null)
        {
            boardRect.anchorMin = new Vector2(0.5f, 0.5f);
            boardRect.anchorMax = new Vector2(0.5f, 0.5f);
            boardRect.pivot = new Vector2(0.5f, 0.5f);
            boardRect.anchoredPosition = Vector2.zero;
            boardRect.sizeDelta = new Vector2(780f, 510f);
            boardRect.localScale = Vector3.one;

            Image boardImage = boardRect.GetComponent<Image>();
            if (boardImage != null)
            {
                boardImage.color = new Color(0.55f, 0.31f, 0.08f, 0.96f);
            }
        }

        StyleRewardHeader();
        StyleRewardRefreshButton();
        StyleRewardCard(btnAText, new Vector2(-250f, -24f), "RewardCard_A");
        StyleRewardCard(btnBText, new Vector2(0f, -24f), "RewardCard_B");
        StyleRewardCard(btnCText, new Vector2(250f, -24f), "RewardCard_C");
        rewardUIStyled = true;
    }

    private RectTransform FindRewardBoard()
    {
        if (rewardPanel == null) return null;

        foreach (Transform child in rewardPanel.transform)
        {
            RectTransform rect = child.GetComponent<RectTransform>();
            if (rect != null && child.GetComponent<Button>() == null)
            {
                return rect;
            }
        }

        return rewardPanel.GetComponent<RectTransform>();
    }

    private void StyleRewardHeader()
    {
        if (rewardPanel == null) return;

        TextMeshProUGUI[] texts = rewardPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (TextMeshProUGUI text in texts)
        {
            if (!text.text.Contains("請選擇")) continue;

            RectTransform rect = text.GetComponent<RectTransform>();
            RectTransform boardRect = FindRewardBoard();
            if (boardRect != null)
            {
                rect.SetParent(boardRect, false);
            }

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(-350f, 202f);
            rect.sizeDelta = new Vector2(360f, 64f);

            text.text = "選擇獎勵";
            text.fontSize = 38f;
            text.alignment = TextAlignmentOptions.Left;
            text.fontStyle = FontStyles.Bold;
            text.color = new Color(1f, 0.88f, 0.42f, 1f);
            ApplyTMPOutline(text, new Color(0.18f, 0.09f, 0.01f, 1f), 0.18f, 0.65f);
            return;
        }
    }

    private void StyleRewardRefreshButton()
    {
        if (rewardPanel == null) return;

        Button[] buttons = rewardPanel.GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label == btnAText || label == btnBText || label == btnCText) continue;
            if (label == null || !label.text.Contains("刷新")) continue;

            RectTransform rect = button.GetComponent<RectTransform>();
            RectTransform boardRect = FindRewardBoard();
            if (boardRect != null)
            {
                rect.SetParent(boardRect, false);
            }

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.anchoredPosition = new Vector2(354f, 206f);
            rect.sizeDelta = new Vector2(104f, 48f);

            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = new Color(0.92f, 0.74f, 0.43f, 1f);
            }

            label.fontSize = 28f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = new Color(0.22f, 0.12f, 0.03f, 1f);
            ApplyTMPOutline(label, new Color(1f, 0.94f, 0.72f, 0.9f), 0.08f, 0.2f);
            return;
        }
    }

    private void StyleRewardCard(TextMeshProUGUI effectText, Vector2 position, string cardName)
    {
        if (effectText == null) return;

        Button button = effectText.GetComponentInParent<Button>(true);
        if (button == null) return;

        button.name = cardName;
        button.transition = Selectable.Transition.ColorTint;

        RectTransform boardRect = FindRewardBoard();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(1f, 0.94f, 0.79f, 1f);
        colors.highlightedColor = new Color(1f, 0.98f, 0.86f, 1f);
        colors.pressedColor = new Color(0.82f, 0.66f, 0.42f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.5f, 0.42f, 0.3f, 0.6f);
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        RectTransform cardRect = button.GetComponent<RectTransform>();
        if (boardRect != null)
        {
            cardRect.SetParent(boardRect, false);
        }

        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.anchoredPosition = position;
        cardRect.sizeDelta = new Vector2(218f, 330f);
        cardRect.localScale = Vector3.one;

        Image cardImage = button.GetComponent<Image>();
        if (cardImage != null)
        {
            cardImage.color = new Color(0.62f, 0.39f, 0.18f, 1f);
            ApplyGraphicOutline(cardImage.gameObject, new Color(0.28f, 0.16f, 0.06f, 0.95f), new Vector2(3f, -3f));
        }

        RectTransform imageSlot = EnsureRewardChild<Image>(button.transform, "ImageSlot").GetComponent<RectTransform>();
        imageSlot.anchorMin = new Vector2(0.5f, 1f);
        imageSlot.anchorMax = new Vector2(0.5f, 1f);
        imageSlot.pivot = new Vector2(0.5f, 1f);
        imageSlot.anchoredPosition = new Vector2(0f, -16f);
        imageSlot.sizeDelta = new Vector2(174f, 148f);
        Image slotImage = imageSlot.GetComponent<Image>();
        slotImage.color = new Color(0.93f, 0.68f, 0.38f, 1f);
        slotImage.raycastTarget = false;
        ApplyGraphicOutline(slotImage.gameObject, new Color(0.72f, 0.45f, 0.19f, 0.9f), new Vector2(2f, -2f));

        RectTransform infoPanel = EnsureRewardChild<Image>(button.transform, "InfoPanel").GetComponent<RectTransform>();
        infoPanel.anchorMin = new Vector2(0.5f, 0f);
        infoPanel.anchorMax = new Vector2(0.5f, 0f);
        infoPanel.pivot = new Vector2(0.5f, 0f);
        infoPanel.anchoredPosition = new Vector2(0f, 8f);
        infoPanel.sizeDelta = new Vector2(174f, 106f);
        Image infoImage = infoPanel.GetComponent<Image>();
        infoImage.color = new Color(0.94f, 0.69f, 0.39f, 1f);
        infoImage.raycastTarget = false;
        infoPanel.SetAsFirstSibling();

        TextMeshProUGUI title = EnsureRewardChild<TextMeshProUGUI>(button.transform, "RewardTitle");
        if (title.font == null && effectText.font != null)
        {
            title.font = effectText.font;
        }

        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0f);
        titleRect.anchorMax = new Vector2(0.5f, 0f);
        titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = new Vector2(0f, 88f);
        titleRect.sizeDelta = new Vector2(180f, 38f);
        title.fontSize = 24f;
        title.alignment = TextAlignmentOptions.Center;
        title.fontStyle = FontStyles.Bold;
        title.color = new Color(1f, 0.9f, 0.48f, 1f);
        title.raycastTarget = false;
        ApplyTMPOutline(title, new Color(0.2f, 0.1f, 0.01f, 1f), 0.12f, 0.35f);

        RectTransform effectRect = effectText.GetComponent<RectTransform>();
        effectRect.anchorMin = new Vector2(0.5f, 0f);
        effectRect.anchorMax = new Vector2(0.5f, 0f);
        effectRect.pivot = new Vector2(0.5f, 0f);
        effectRect.anchoredPosition = new Vector2(0f, 8f);
        effectRect.sizeDelta = new Vector2(166f, 76f);
        effectText.fontSize = 21f;
        effectText.alignment = TextAlignmentOptions.Left;
        effectText.fontStyle = FontStyles.Bold;
        effectText.color = new Color(1f, 0.97f, 0.84f, 1f);
        effectText.raycastTarget = false;
        ApplyTMPOutline(effectText, new Color(0.24f, 0.12f, 0.02f, 1f), 0.08f, 0.3f);
    }

    private void ApplyGraphicOutline(GameObject target, Color color, Vector2 distance)
    {
        if (target == null) return;

        Outline outline = target.GetComponent<Outline>();
        if (outline == null) outline = target.AddComponent<Outline>();
        outline.effectColor = color;
        outline.effectDistance = distance;
        outline.useGraphicAlpha = true;
    }

    private T EnsureRewardChild<T>(Transform parent, string childName) where T : Component
    {
        Transform child = parent.Find(childName);
        if (child == null)
        {
            GameObject childObject = new GameObject(childName);
            childObject.transform.SetParent(parent, false);
            child = childObject.transform;
        }

        T component = child.GetComponent<T>();
        if (component == null) component = child.gameObject.AddComponent<T>();
        return component;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // 記得把時間恢復流動
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // 重新讀取當前場景
    }

    public void ShowReward(bool isBigBoss)
    {
        isBigBossReward = isBigBoss;
        SetupRewardUI();
        rewardPanel.SetActive(true);
        Time.timeScale = 0f;
        GenerateRewards();
    }

    public void RefreshRewards()
    {
        if (isBigBossReward) return;
        GenerateRewards();
    }

    private void GenerateRewards()
    {
        currentOptions.Clear();

        if (isBigBossReward)
        {
            currentOptions.Add(RewardType.BigAtk);
            currentOptions.Add(RewardType.BigSpd);
            currentOptions.Add(RewardType.BigHp);
        }
        else
        {
            List<RewardType> stylePool = new List<RewardType> { RewardType.Light, RewardType.Heavy, RewardType.Multi };
            ShuffleRewards(stylePool);

            currentOptions.Add(stylePool[0]);
            currentOptions.Add(stylePool[1]);
            currentOptions.Add(PickSupportReward());
        }

        if (buttonCObject != null) buttonCObject.SetActive(currentOptions.Count >= 3);
        if (currentOptions.Count > 0) UpdateButtonUI(btnAText, currentOptions[0]);
        if (currentOptions.Count > 1) UpdateButtonUI(btnBText, currentOptions[1]);
        if (currentOptions.Count > 2) UpdateButtonUI(btnCText, currentOptions[2]);
        else if (btnCText != null) btnCText.text = "";
    }

    private void ShuffleRewards(List<RewardType> rewards)
    {
        for (int i = 0; i < rewards.Count; i++)
        {
            RewardType temp = rewards[i];
            int randomIndex = Random.Range(i, rewards.Count);
            rewards[i] = rewards[randomIndex];
            rewards[randomIndex] = temp;
        }
    }

    private RewardType PickSupportReward()
    {
        List<RewardType> pool = new List<RewardType>();

        if (playerStats.lifestealLevel < 3) AddWeightedReward(pool, RewardType.Lifesteal, 2);
        if (playerStats.collisionResistLevel < 3) AddWeightedReward(pool, RewardType.Resist, 2);
        if (playerStats.magnetLevel < 3) AddWeightedReward(pool, RewardType.Magnet, 1);
        AddWeightedReward(pool, RewardType.EnemyPlusOne, 1);

        if (pool.Count == 0) return RewardType.EnemyPlusOne;
        return pool[Random.Range(0, pool.Count)];
    }

    private void AddWeightedReward(List<RewardType> pool, RewardType reward, int weight)
    {
        for (int i = 0; i < weight; i++)
        {
            pool.Add(reward);
        }
    }

    private void UpdateButtonUI(TextMeshProUGUI btnText, RewardType type)
    {
        SetRewardTitle(btnText, GetRewardTitle(type));
        SetRewardIcon(btnText, type);

        switch (type)
        {
            case RewardType.Light: btnText.text = "\n<color=#8EF18E>射速 +35%</color>\n<color=#FF5E57>傷害 -25%</color>"; break;
            case RewardType.Heavy: btnText.text = "\n<color=#8EF18E>攻擊力 +9</color>\n<color=#FF5E57>攻速 -18%</color>"; break;
            case RewardType.Multi: btnText.text = "\n<color=#8EF18E>箭矢數量 +1</color>\n<color=#FF5E57>傷害 -28%</color>"; break;
            case RewardType.BigAtk: btnText.text = "\n<color=#FFE66B>攻擊力 x1.5</color>\n王牌爆發"; break;
            case RewardType.BigSpd: btnText.text = "\n<color=#FFE66B>攻擊速度 x1.4</color>\n狂暴連射"; break;
            case RewardType.BigHp: btnText.text = "\n<color=#FFE66B>生命 +50%</color>\n存活強化"; break;
            case RewardType.Lifesteal: btnText.text = $"\n<color=#8EF18E>吸血 Lv{playerStats.lifestealLevel + 1}</color>\n傷害 {(playerStats.lifestealLevel + 1) * 5}% 回血"; break;
            case RewardType.Resist: btnText.text = $"\n<color=#8EF18E>抗撞 Lv{playerStats.collisionResistLevel + 1}</color>\n撞擊減傷 {(playerStats.collisionResistLevel + 1) * 10}%"; break;
            case RewardType.EnemyPlusOne: btnText.text = "\n<color=#8EF18E>下一段怪物 +1</color>\n通過 Boss 後結束"; break;
            case RewardType.Magnet: btnText.text = $"\n<color=#8EF18E>磁鐵 Lv{playerStats.magnetLevel + 1}</color>\n半徑 +{(playerStats.magnetLevel + 1) * 3}m"; break;
        }
    }

    private string GetRewardTitle(RewardType type)
    {
        switch (type)
        {
            case RewardType.Light: return "輕弩流派";
            case RewardType.Heavy: return "重砲流派";
            case RewardType.Multi: return "多重箭";
            case RewardType.BigAtk: return "王牌力量";
            case RewardType.BigSpd: return "狂暴極速";
            case RewardType.BigHp: return "生命強化";
            case RewardType.Lifesteal: return "吸血";
            case RewardType.Resist: return "堅若磐石";
            case RewardType.EnemyPlusOne: return "敵潮洶湧";
            case RewardType.Magnet: return "金幣磁鐵";
            default: return "獎勵";
        }
    }

    private string GetRewardIconKey(RewardType type)
    {
        return type.ToString();
    }

    private void SetRewardIcon(TextMeshProUGUI effectText, RewardType type)
    {
        if (effectText == null) return;

        Transform card = effectText.GetComponentInParent<Button>(true)?.transform;
        if (card == null) return;

        Image image = card.Find("ImageSlot")?.GetComponent<Image>();
        if (image == null) return;

        Sprite sprite = Resources.Load<Sprite>("RewardIcons/" + GetRewardIconKey(type));
        if (sprite == null)
        {
            Texture2D texture = Resources.Load<Texture2D>("RewardIcons/" + GetRewardIconKey(type));
            if (texture != null)
            {
                sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
            }
        }

        if (sprite == null) return;

        image.sprite = sprite;
        image.preserveAspect = true;
        image.color = Color.white;
    }

    private void SetRewardTitle(TextMeshProUGUI effectText, string title)
    {
        if (effectText == null) return;

        Transform card = effectText.GetComponentInParent<Button>(true)?.transform;
        if (card == null) return;

        TextMeshProUGUI titleText = card.Find("RewardTitle")?.GetComponent<TextMeshProUGUI>();
        if (titleText != null)
        {
            titleText.text = title;
        }
    }

    public void ChooseRewardA()
    {
        if (currentOptions.Count <= 0) return;
        ApplyReward(currentOptions[0]);
        ResumeGame();
    }
    public void ChooseRewardB()
    {
        if (currentOptions.Count <= 1) return;
        ApplyReward(currentOptions[1]);
        ResumeGame();
    }
    public void ChooseRewardC()
    {
        if (currentOptions.Count <= 2) return;
        ApplyReward(currentOptions[2]);
        ResumeGame();
    }

    private void ApplyReward(RewardType type)
    {
        if (playerStats == null) return;
        if (collectedRewardIcons.Count < 12)
        {
            collectedRewardIcons.Add(GetRewardIconKey(type));
        }

        switch (type)
        {
            case RewardType.Light:
                playerStats.attackSpeed *= 1.35f;
                playerStats.baseDamage *= 0.75f;
                playerStats.RegisterArrowStyle(PlayerStats.ArrowStyle.Speed);
                break;
            case RewardType.Heavy:
                playerStats.baseDamage += 9f;
                playerStats.attackSpeed *= 0.82f;
                playerStats.RegisterArrowStyle(PlayerStats.ArrowStyle.Attack);
                break;
            case RewardType.Multi:
                playerStats.arrowCount += 1;
                playerStats.baseDamage *= 0.72f;
                playerStats.RegisterArrowStyle(PlayerStats.ArrowStyle.Multi);
                break;
            case RewardType.BigAtk: playerStats.baseDamage *= 1.5f; break;
            case RewardType.BigSpd: playerStats.attackSpeed *= 1.4f; break;
            case RewardType.BigHp: playerStats.AddMaxHealth(Mathf.RoundToInt(playerStats.maxHp * 0.5f)); break;
            case RewardType.Lifesteal: playerStats.lifestealLevel++; break;
            case RewardType.Resist: playerStats.collisionResistLevel++; break;
            case RewardType.EnemyPlusOne:
                playerStats.extraEnemies = 1;
                enemyPlusOneWaitingForBoss = true;
                enemyPlusOneTrackedBossZ = -1f;
                Enemy.RefreshAllExtraEnemies(playerStats.extraEnemies);
                break; // 🌟 只影響下一段 Boss 距離
            case RewardType.Magnet: playerStats.magnetLevel++; break;
        }
    }

    private void ResumeGame() { rewardPanel.SetActive(false); Time.timeScale = 1f; }

    public void EndEnemyPlusOneStage()
    {
        if (playerStats == null || playerStats.extraEnemies <= 0) return;

        playerStats.extraEnemies = 0;
        enemyPlusOneWaitingForBoss = false;
        enemyPlusOneTrackedBossZ = -1f;
        Enemy.ClearAllExtraEnemies();
    }

    private void CheckEnemyPlusOneBossPassed()
    {
        if (!enemyPlusOneWaitingForBoss || enemyPlusOneTrackedBossZ < 0f || playerStats == null) return;

        if (playerStats.transform.position.z > enemyPlusOneTrackedBossZ + enemyPlusOneBossPassDistance)
        {
            EndEnemyPlusOneStage();
        }
    }

    public string FormatNumber(float number)
    {
        if (number >= 1000000000) return (number / 1000000000f).ToString("0.#") + "B";
        else if (number >= 1000000) return (number / 1000000f).ToString("0.#") + "M";
        else if (number >= 1000) return (number / 1000f).ToString("0.#") + "K";
        return Mathf.FloorToInt(number).ToString();
    }

    private void SpawnBoss(bool isBigBoss, float targetZ)
    {
        Vector3 spawnPos = new Vector3(0, 1.5f, targetZ);
        float baseEnemyMaxHp = 75f;
        float scalingDistance = Mathf.Max(0, spawnPos.z - 30f);
        float stage = Mathf.Floor(scalingDistance / 40f);
        float currentEnemyHp = Mathf.Round(baseEnemyMaxHp * Mathf.Pow(1.13f, stage));
        GameObject spawnedBoss = null;

        if (isBigBoss)
        {
            spawnedBoss = Instantiate(bigBossPrefab, spawnPos, Quaternion.identity);
            if (spawnedBoss.GetComponent<BossHealth>() != null) spawnedBoss.GetComponent<BossHealth>().SetupHealth(currentEnemyHp * 4f);
        }
        else
        {
            if (smallBossPrefabs != null && smallBossPrefabs.Length > 0)
            {
                int randomIndex = Random.Range(0, smallBossPrefabs.Length);
                spawnedBoss = Instantiate(smallBossPrefabs[randomIndex], spawnPos, Quaternion.identity);
                if (spawnedBoss.GetComponent<BossHealth>() != null) spawnedBoss.GetComponent<BossHealth>().SetupHealth(currentEnemyHp * 1.55f);
                SetupSmallBossAttack(spawnedBoss);
            }
        }

        if (spawnedBoss != null && enemyPlusOneWaitingForBoss && enemyPlusOneTrackedBossZ < 0f)
        {
            enemyPlusOneTrackedBossZ = spawnedBoss.transform.position.z;
        }
    }
    public void SpawnBossRewardChest(Vector3 position, bool isBigBoss)
    {
        if (bossRewardChestPrefab == null) return;
        Vector3 spawnPos = new Vector3(position.x - 1f, -0.45f, position.z);
        GameObject chest = Instantiate(bossRewardChestPrefab, spawnPos, Quaternion.Euler(0, 180f, 0));

        // 把 isBigBoss 傳給箱子，讓它知道要跳哪種獎勵
        BossRewardChest chestScript = chest.GetComponent<BossRewardChest>();
        if (chestScript != null) chestScript.isBigBoss = isBigBoss;
    }

    private void SetupSmallBossAttack(GameObject spawnedBoss)
    {
        if (spawnedBoss == null) return;

        SmallBossAttackAI attackAI = spawnedBoss.GetComponent<SmallBossAttackAI>();
        if (attackAI == null)
        {
            attackAI = spawnedBoss.AddComponent<SmallBossAttackAI>();
        }

        int patternCount = System.Enum.GetValues(typeof(SmallBossAttackAI.AttackPattern)).Length;
        SmallBossAttackAI.AttackPattern pattern = (SmallBossAttackAI.AttackPattern)(smallBossAttackPatternIndex % patternCount);
        smallBossAttackPatternIndex++;
        smallBossEncounterCount++;
        attackAI.clawProjectilePrefab = smallBossClawProjectilePrefab;
        attackAI.clawPrefabRotationOffset = new Vector3(-90f, 0f, 0f);
        attackAI.clawPrefabScale = Vector3.one * 2f;
        attackAI.SetPattern(pattern);
        attackAI.ApplyEncounterScaling(smallBossEncounterCount);
    }
}
