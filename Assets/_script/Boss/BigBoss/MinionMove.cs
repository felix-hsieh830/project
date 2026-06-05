using UnityEngine;

public class MinionMove : MonoBehaviour
{
    [Header("追蹤設定")]
    public float moveSpeed = 8f; // 小怪衝向玩家的速度

    // 🌟 用來當作目標的「玩家座標」
    private Transform targetPlayer;

    void Start()
    {
        // 遊戲一開始，死靈就立刻在場上尋找並鎖定主角
        PlayerMove player = FindAnyObjectByType<PlayerMove>();
        if (player != null)
        {
            targetPlayer = player.transform;
        }
    }

    void Update()
    {
        if (targetPlayer != null)
        {
            // 1. 讓死靈永遠面朝玩家 (這樣模型轉向看起來比較有壓迫感)
            transform.LookAt(targetPlayer);

            // 2. 🌟 核心追蹤魔法：MoveTowards (從自己的位置，以固定速度走到玩家的位置)
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPlayer.position,
                moveSpeed * Time.deltaTime
            );
        }
        else
        {
            // 如果場上找不到玩家 (例如玩家剛好死掉了)，就維持原本的瞎走模式
            transform.Translate(0, 0, moveSpeed * Time.deltaTime);
        }
    }
}