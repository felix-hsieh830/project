using UnityEngine;

public class TrackDecorator : MonoBehaviour
{
    [Header("預設裝飾物（找不到 StageManager 時用）")]
    public GameObject[] defaultDecorPrefabs;

    [Header("地板 MeshRenderer（用來換材質）")]
    public MeshRenderer trackRenderer;

    [Header("外側生成設定（樹、石頭）")]
    public int countPerSide = 20;
    public float sideOffsetMin = 8f;
    public float sideOffsetMax = 11.5f;
    public float zRangeMin = 0f;
    public float zRangeMax = 30f;

    [Header("外側縮放")]
    public float scaleMin = 1f;
    public float scaleMax = 3f;

    [Header("內側生成設定（花、草）")]
    public GameObject[] innerDecorPrefabs;
    public int innerCountPerSide = 15;
    public float innerOffsetMin = 0f;
    public float innerOffsetMax = 4.5f;

    [Header("內側縮放")]
    public float innerScaleMin = 0.8f;
    public float innerScaleMax = 1.4f;

    [Header("柵欄設定")]
    public GameObject fencePrefab;
    public float fenceSpacing = 4.5f;
    public float fenceSideOffset = 5f;

    [Header("廢墟牆設定")]
    public GameObject[] wallPrefabs;
    public int wallCountPerSide = 5;
    public float wallOffsetMin = 12f;
    public float wallOffsetMax = 12f;
    public float wallScaleMin = 1f;
    public float wallScaleMax = 1f;

    [Header("水池設定")]
    public Material waterMaterial;
    public float waterSideOffset = 14f;
    public float waterWidth = 8f;
    public float waterYOffset = 0.3f;

    void Start()
    {
        StageManager.StageData stage = StageManager.instance?.GetCurrentStage();

        // 1. 換地板材質
        if (stage != null && stage.trackMaterial != null && trackRenderer != null)
        {
            trackRenderer.material = stage.trackMaterial;
        }

        // 2. 選外側裝飾物來源
        GameObject[] outerPrefabs = (stage != null && stage.decorPrefabs != null && stage.decorPrefabs.Length > 0)
            ? stage.decorPrefabs
            : defaultDecorPrefabs;

        // 3. 生成水池
        SpawnWater();

        // 4. 生成外側裝飾物
        if (outerPrefabs != null && outerPrefabs.Length > 0)
        {
            SpawnSide(1f, outerPrefabs, sideOffsetMin, sideOffsetMax, countPerSide, scaleMin, scaleMax);
            SpawnSide(-1f, outerPrefabs, sideOffsetMin, sideOffsetMax, countPerSide, scaleMin, scaleMax);
        }

        // 5. 生成廢墟牆（固定朝向）
        if (wallPrefabs != null && wallPrefabs.Length > 0)
        {
            SpawnWalls(1f);
            SpawnWalls(-1f);
        }

        // 6. 生成內側花草
        if (innerDecorPrefabs != null && innerDecorPrefabs.Length > 0)
        {
            SpawnSide(1f, innerDecorPrefabs, innerOffsetMin, innerOffsetMax, innerCountPerSide, innerScaleMin, innerScaleMax);
            SpawnSide(-1f, innerDecorPrefabs, innerOffsetMin, innerOffsetMax, innerCountPerSide, innerScaleMin, innerScaleMax);
        }

        // 7. 生成柵欄
        SpawnFences();
    }

    void SpawnSide(float side, GameObject[] prefabs, float offsetMin, float offsetMax, int count, float sMin, float sMax)
    {
        float zStep = (zRangeMax - zRangeMin) / count;

        for (int i = 0; i < count; i++)
        {
            GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];

            float x = side * Random.Range(offsetMin, offsetMax);
            float z = zRangeMin + (i * zStep) + Random.Range(0f, zStep * 0.8f);
            Vector3 spawnPos = transform.position + new Vector3(x, -0.5f, z);

            GameObject obj = Instantiate(prefab, spawnPos, Quaternion.Euler(0, Random.Range(0f, 360f), 0), transform);
            float scale = Random.Range(sMin, sMax);
            obj.transform.localScale = Vector3.one * scale;
        }
    }

    void SpawnWalls(float side)
    {
        float zStep = (zRangeMax - zRangeMin) / wallCountPerSide;

        for (int i = 0; i < wallCountPerSide; i++)
        {
            GameObject prefab = wallPrefabs[Random.Range(0, wallPrefabs.Length)];

            float x = side * Random.Range(wallOffsetMin, wallOffsetMax);
            float z = zRangeMin + (i * zStep) + Random.Range(0f, zStep * 0.8f);
            Vector3 spawnPos = transform.position + new Vector3(x, -0.5f, z);

            // 固定朝向 Z 軸，不隨機旋轉
            GameObject obj = Instantiate(prefab, spawnPos, Quaternion.Euler(0, 90f, 0), transform);
            float scale = Random.Range(wallScaleMin, wallScaleMax);
            obj.transform.localScale = Vector3.one * scale;
        }
    }

    void SpawnWater()
    {
        if (waterMaterial == null) return;

        GameObject waterLeft = GameObject.CreatePrimitive(PrimitiveType.Plane);
        waterLeft.name = "Water_Left";
        waterLeft.transform.parent = transform;
        waterLeft.transform.position = transform.position + new Vector3(-waterSideOffset, waterYOffset, zRangeMax / 2f);
        waterLeft.transform.localScale = new Vector3(waterWidth / 10f, 1f, zRangeMax / 10f);
        waterLeft.GetComponent<MeshRenderer>().material = waterMaterial;
        Destroy(waterLeft.GetComponent<Collider>());

        GameObject waterRight = GameObject.CreatePrimitive(PrimitiveType.Plane);
        waterRight.name = "Water_Right";
        waterRight.transform.parent = transform;
        waterRight.transform.position = transform.position + new Vector3(waterSideOffset, waterYOffset, zRangeMax / 2f);
        waterRight.transform.localScale = new Vector3(waterWidth / 10f, 1f, zRangeMax / 10f);
        waterRight.GetComponent<MeshRenderer>().material = waterMaterial;
        Destroy(waterRight.GetComponent<Collider>());
    }

    void SpawnFences()
    {
        if (fencePrefab == null) return;

        for (float z = zRangeMin; z < zRangeMax; z += fenceSpacing)
        {
            Vector3 leftPos = transform.position + new Vector3(-fenceSideOffset, -0.5f, z);
            Instantiate(fencePrefab, leftPos, Quaternion.Euler(0, 90f, 0), transform);

            Vector3 rightPos = transform.position + new Vector3(fenceSideOffset, -0.5f, z);
            Instantiate(fencePrefab, rightPos, Quaternion.Euler(0, 90f, 0), transform);
        }
    }
}