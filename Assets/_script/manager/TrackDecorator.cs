using UnityEngine;

public class TrackDecorator : MonoBehaviour
{
    [Header("預設裝飾物（找不到 StageManager 時用）")]
    public GameObject[] defaultDecorPrefabs;

    [Header("地板 MeshRenderer（用來換材質）")]
    public MeshRenderer trackRenderer;

    [Header("生成設定")]
    public int countPerSide = 3;
    public float sideOffsetMin = 5f;
    public float sideOffsetMax = 10f;
    public float zRangeMin = 0f;
    public float zRangeMax = 10f;

    [Header("隨機縮放")]
    public float scaleMin = 0.8f;
    public float scaleMax = 1.5f;

    void Start()
    {
        StageManager.StageData stage = StageManager.instance?.GetCurrentStage();

        // 1. 換地板材質
        if (stage != null && stage.trackMaterial != null && trackRenderer != null)
        {
            trackRenderer.material = stage.trackMaterial;
        }

        // 2. 選裝飾物來源
        GameObject[] prefabs = (stage != null && stage.decorPrefabs != null && stage.decorPrefabs.Length > 0)
            ? stage.decorPrefabs
            : defaultDecorPrefabs;

        if (prefabs == null || prefabs.Length == 0) return;

        SpawnSide(1f, prefabs);
        SpawnSide(-1f, prefabs);
    }

    void SpawnSide(float side, GameObject[] prefabs)
    {
        for (int i = 0; i < countPerSide; i++)
        {
            GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];

            float x = side * Random.Range(sideOffsetMin, sideOffsetMax);
            float z = Random.Range(zRangeMin, zRangeMax);
            Vector3 spawnPos = transform.position + new Vector3(x, 10f, z);

            if (Physics.Raycast(spawnPos, Vector3.down, out RaycastHit hit, 20f))
                 spawnPos = hit.point;
            else
                 spawnPos = transform.position + new Vector3(x, 0, z);

            GameObject obj = Instantiate(prefab, spawnPos, Quaternion.Euler(0, Random.Range(0f, 360f), 0), transform);

            float scale = Random.Range(scaleMin, scaleMax);
            obj.transform.localScale = Vector3.one * scale;
        }
    }
}