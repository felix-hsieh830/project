using UnityEngine;
using UnityEngine.Rendering;

public class StageManager : MonoBehaviour
{
    public static StageManager instance;

    [System.Serializable]
    public class StageData
    {
        public string stageName;
        public float startDistance;      // 這個階段從哪個距離開始

        [Header("地板材質")]
        public Material trackMaterial;

        [Header("天空盒")]
        public Material skyboxMaterial;

        [Header("霧氣設定")]
        public bool fogEnabled = false;
        public Color fogColor = Color.gray;
        public float fogDensity = 0.02f;

        [Header("環境光")]
        public Color ambientColor = Color.white;

        [Header("裝飾物 Prefab（這個階段用的樹木岩石）")]
        public GameObject[] decorPrefabs;
    }

    [Header("四個階段設定")]
    public StageData[] stages = new StageData[4];

    [Header("引用")]
    public PlayerStats playerStats;

    private int currentStage = -1;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (playerStats == null)
            playerStats = FindAnyObjectByType<PlayerStats>();

        // 初始化第一個階段
        ApplyStage(0);
    }

    void Update()
    {
        if (playerStats == null) return;

        float distance = playerStats.transform.position.z - 30f;
        if (distance < 0) distance = 0;

        // 找出當前應該在哪個階段
        int newStage = 0;
        for (int i = stages.Length - 1; i >= 0; i--)
        {
            if (distance >= stages[i].startDistance)
            {
                newStage = i;
                break;
            }
        }

        // 如果階段改變了，套用新階段
        if (newStage != currentStage)
        {
            currentStage = newStage;
            ApplyStage(currentStage);
        }
    }

    void ApplyStage(int index)
    {
        if (index < 0 || index >= stages.Length) return;
        StageData stage = stages[index];

        Debug.Log($"🌲 進入階段：{stage.stageName}");

        // 1. 切換天空盒
        if (stage.skyboxMaterial != null)
            RenderSettings.skybox = stage.skyboxMaterial;

        // 2. 霧氣
        RenderSettings.fog = stage.fogEnabled;
        if (stage.fogEnabled)
        {
            RenderSettings.fogColor = stage.fogColor;
            RenderSettings.fogDensity = stage.fogDensity;
            RenderSettings.fogMode = FogMode.Exponential;
        }

        // 3. 環境光
        RenderSettings.ambientLight = stage.ambientColor;

        // 4. 通知 TrackDecorator 換裝飾物（新生成的地板會用新的）
        // 地板材質由 TrackDecorator 在生成時套用
    }

    // 🌟 提供給 TrackDecorator 和 LevelManager 查詢當前階段
    public StageData GetCurrentStage()
    {
        if (currentStage < 0 || currentStage >= stages.Length) return null;
        return stages[currentStage];
    }
}