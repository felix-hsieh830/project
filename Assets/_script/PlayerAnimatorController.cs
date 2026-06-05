using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour
{
    [Header("組件引用")]
    public Animator animator;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    /// <summary>
    /// 控制下半身/全身的移動動畫（對應 Base Layer 的 2D 混合樹）
    /// </summary>
    /// <param name="horizontal">左右移動輸入：-1 代表左、0 代表靜止、1 代表右</param>
    /// <param name="vertical">前後移動輸入：0 代表原地、1 代表向前跑（《箭箭劍》通常持續傳入 1）</param>
    public void UpdateMovementAnimation(float horizontal, float vertical)
    {
        if (animator == null) return;

        // 傳遞參數給混合樹，Unity 會自動融合 Idle、前跑、左移、右移的動作
        animator.SetFloat("SpeedX", horizontal);
        animator.SetFloat("SpeedY", vertical);
    }

    /// <summary>
    /// 觸發射箭動畫（對應 UpperBody 圖層的 Shoot 觸發器）
    /// </summary>
    public void TriggerShootAnimation()
    {
        if (animator == null) return;

        // 觸發 Shoot 參數，讓上半身獨立執行拉弓、放箭的流程
        animator.SetTrigger("Shoot");
    }
    /// <summary>
    /// 根據當前的發射間隔，動態調整射箭動畫的播放速度
    /// </summary>
    /// <param name="currentFireRate">當前的射擊間隔（秒）</param>
    /// <param name="baseFireRate">初始的射擊間隔（例如 0.5 秒）</param>
    public void UpdateLayerSpeed(float currentFireRate, float baseFireRate)
    {
        if (animator == null) return;

        // 計算速度倍率：初始間隔 / 當前間隔
        // 例如：原本 0.5 秒射一次（速度 1），現在變 0.25 秒射一次，速度就會變成 0.5 / 0.25 = 2 倍速！
        float speedMultiplier = baseFireRate / currentFireRate;

        // 取得 UpperBody 圖層的索引（通常 Base Layer 是 0，UpperBody 是 1）
        int layerIndex = animator.GetLayerIndex("UpperBody");

        if (layerIndex != -1)
        {
            // 🌟 關鍵代碼：直接改變整個圖層的動畫播放速度！
            animator.SetLayerWeight(layerIndex, 1f); // 確保權重是 1
                                                     // 注意：如果要針對特定狀態，可以用參數控制，但直接改圖層速度最省事
        }

        // 另一種更精準的做法：利用 Animator 的 Parameter 傳遞速度變數
        animator.SetFloat("AttackSpeedParam", speedMultiplier);
    }
}