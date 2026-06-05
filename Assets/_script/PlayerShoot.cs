using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [Header("射擊設定")]
    public GameObject arrowPrefab;        // 箭矢的 Prefab (預製物)
    public Transform shootPoint;          // 箭矢生成的起點
    public float fireRate = 0.5f;         // 射擊間隔（秒），吃寶箱後這個數字會變小（射速變快）

    [Header("初始射速設定")]
    public float baseFireRate = 0.5f;     // 🌟 新增：基準發射速度（用來計算倍率，通常設跟初始 fireRate 一樣）

    [Header("引用其他組件")]
    private PlayerAnimatorController animController;
    private float shotTimer = 0f;
    private bool justShot = false;

    void Start()
    {
        // 取得同一個物件上的動畫控制器
        animController = GetComponent<PlayerAnimatorController>();

        // 如果沒有手動在 Inspector 指派發射點，預設使用角色自己的位置
        if (shootPoint == null)
        {
            shootPoint = this.transform;
        }
    }

    void Update()
    {
        // 重設每幀的射擊狀態
        justShot = false;

        // 🌟 核心邏輯：動態計算動畫速度倍率並傳給 Animator
        // 原理：初始間隔 / 當前間隔。例如原本 0.5 秒一發(1倍速)，變成 0.25 秒一發時，倍率 = 0.5 / 0.25 = 2 倍速
        if (animController != null && animController.animator != null)
        {
            float speedMultiplier = baseFireRate / fireRate;
            animController.animator.SetFloat("AttackSpeedParam", speedMultiplier);
        }

        // 累加計時器
        shotTimer += Time.deltaTime;

        // 當時間到了，就自動發射
        if (shotTimer >= fireRate)
        {
            Shoot();
            shotTimer = 0f; // 重置計時器
        }
    }

    void Shoot()
    {
        if (arrowPrefab == null)
        {
            Debug.LogWarning("請在 Inspector 中指派 Arrow Prefab！");
            return;
        }

        // 1. 生成箭矢
        Instantiate(arrowPrefab, shootPoint.position, shootPoint.rotation);

        // 2. 標記剛剛射擊了
        justShot = true;

        // 3. 通知動畫控制器：播放射箭動畫
        if (animController != null)
        {
            animController.TriggerShootAnimation();
        }
    }

    /// <summary>
    /// 提供給外部（如動畫控制器）查詢：這一幀是否剛好發射了箭
    /// </summary>
    public bool CheckIfJustShot()
    {
        return justShot;
    }
}