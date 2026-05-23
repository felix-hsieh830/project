using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public GameObject arrowPrefab;
    [SerializeField] private float offsetWidth = 0.5f;

    private float timer = 0f;
    private PlayerStats stats; // 用來綁定大腦

    void Start()
    {
        // 遊戲一開始，自動去身上找 PlayerStats 腳本
        stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        timer += Time.deltaTime;

        // 將「每秒攻擊次數(攻速)」轉換成「冷卻時間(秒)」
        float fireCooldown = 1f / stats.attackSpeed;

        if (timer >= fireCooldown)
        {
            Shoot();
            timer = 0f;
        }
    }

    void Shoot()
    {
        for (int i = 0; i < stats.arrowCount; i++) // 現在讀取 stats 裡的箭數
        {
            float offsetX = 0;
            if (stats.arrowCount > 1)
            {
                offsetX = -offsetWidth * (stats.arrowCount - 1) / 2f + (offsetWidth * i);
            }

            Vector3 spawnPosition = transform.position + new Vector3(offsetX, 0, 0);

            // 生成箭矢
            GameObject arrow = Instantiate(arrowPrefab, spawnPosition, transform.rotation);

            // 找到這支箭身上的 ArrowFly 腳本，把玩家的屬性傳給它！
            ArrowFly arrowScript = arrow.GetComponent<ArrowFly>();
            if (arrowScript != null)
            {
                arrowScript.Setup(stats.baseDamage, stats.attackRange, stats.critRate, stats.critDamage);
            }
        }
    }
}