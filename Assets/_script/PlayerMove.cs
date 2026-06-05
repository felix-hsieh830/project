using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("移動速度設定")]
    public float forwardSpeed = 5f;       // 自動向前奔跑的速度
    public float horizontalSpeed = 8f;    // 玩家左右閃躲的速度

    [Header("跑道邊界限制")]
    public float minX = -4f;              // 跑道左邊界
    public float maxX = 4f;               // 跑道右邊界

    [Header("引用其他組件")]
    private PlayerAnimatorController animController;
    private float horizontalInput;

    void Start()
    {
        // 取得同一個物件上的動畫控制器
        animController = GetComponent<PlayerAnimatorController>();
    }

    void Update()
    {
        // 1. 偵測玩家的左右輸入 (A/D 鍵 或 左右方向鍵)
        horizontalInput = Input.GetAxis("Horizontal");

        // 2. 計算這一幀的移動向量
        // Vector3.forward * forwardSpeed -> 自動往前跑
        // Vector3.right * horizontalInput * horizontalSpeed -> 玩家控制左右
        Vector3 movement = (Vector3.forward * forwardSpeed) + (Vector3.right * horizontalInput * horizontalSpeed);

        // 3. 套用移動
        transform.Translate(movement * Time.deltaTime, Space.World);

        // 4. 限制角色的 X 軸位置，防止玩家跑到跑道外面
        float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
        transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);

        // 5. 將移動數據傳給動畫控制器 (《箭箭劍》直跑 vertical 傳 1.0f)
        if (animController != null)
        {
            animController.UpdateMovementAnimation(horizontalInput, 1.0f);
        }
    }

    // 提供一個公開方法，讓其他腳本（如果需要）知道目前的輸入值
    public float GetCurrentHorizontalInput()
    {
        return horizontalInput;
    }
}