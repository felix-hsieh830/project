using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public GameObject arrowPrefab;
    [SerializeField] private float offsetWidth = 0.5f;

    private float timer = 0f;
    private PlayerStats stats;

    // ==========================================
    // 🌟 新增：用來記錄玩家位置與計算即時速度的變數
    // ==========================================
    private float currentPlayerZSpeed = 0f;
    private Vector3 lastPosition;

    void Start()
    {
        stats = GetComponent<PlayerStats>();
        lastPosition = transform.position; // 紀錄初始位置
    }

    void Update()
    {

        if (Time.deltaTime > 0f)
        {
            currentPlayerZSpeed = (transform.position.z - lastPosition.z) / Time.deltaTime;
        }
        else
        {
            currentPlayerZSpeed = 0f; // 時間暫停時，動能歸零
        }
        
        lastPosition = transform.position; // 更新紀錄點

        timer += Time.deltaTime;
        float fireCooldown = 1f / stats.attackSpeed;

        if (timer >= fireCooldown)
        {
            Shoot();
            timer = 0f;
        }
    }

    void Shoot()
    {
        for (int i = 0; i < stats.arrowCount; i++)
        {
            float offsetX = 0;
            if (stats.arrowCount > 1)
            {
                offsetX = -offsetWidth * (stats.arrowCount - 1) / 2f + (offsetWidth * i);
            }

            // 🌟 順便把生成點往前挪 1.5f (第一招跟第二招結合最完美)
            Vector3 spawnPosition = transform.position + new Vector3(offsetX, 0, 1.5f);

            GameObject arrow = Instantiate(arrowPrefab, spawnPosition, transform.rotation);

            ArrowFly arrowScript = arrow.GetComponent<ArrowFly>();
            if (arrowScript != null)
            {
                // ==========================================
                // 🌟 修改這裡：把 currentPlayerZSpeed (玩家動能) 當作第 5 個參數傳過去！
                // ==========================================
                arrowScript.Setup(stats.baseDamage, stats.attackRange, stats.critRate, stats.critDamage, currentPlayerZSpeed);
            }
        }
    }
}