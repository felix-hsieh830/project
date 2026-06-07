using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("移動速度設定")]
    public float forwardSpeed = 5f;
    public float horizontalSpeed = 5f;

    [Header("加速設定")]
    public float speedPerHundredMeters = 0.5f;

    [Header("跑道邊界限制")]
    public float limitX = 4.5f;

    [Header("大 Boss 戰鬥狀態")]
    public bool isFightingBigBoss = false; // 🌟 新增：用來判斷是不是正在打大王

    private PlayerAnimatorController animController;
    private float horizontalInput;

    void Start()
    {
        animController = GetComponent<PlayerAnimatorController>();
    }

    void Update()
    {
        float touchInput = 0f;

        // 觸控支援
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.position.x < Screen.width / 2f)
                touchInput = -1f;
            else
                touchInput = 1f;
        }

        // 鍵盤輸入（AD + 方向鍵）
        float keyInput = Input.GetAxisRaw("Horizontal");

        // 合併輸入，觸控優先
        horizontalInput = (Input.touchCount > 0) ? touchInput : keyInput;

        float distance = transform.position.z;
        if (distance < 0) distance = 0;

        float currentForwardSpeed = 0f;

        // 🌟 核心修改：如果「沒有」在打大 Boss，才給予前進速度
        if (!isFightingBigBoss)
        {
            float extraSpeed = (distance / 100f) * speedPerHundredMeters;
            currentForwardSpeed = forwardSpeed + extraSpeed;
        }

        // 左右輸入保持正常運作
        horizontalInput = Input.GetAxisRaw("Horizontal");

        Vector3 movement = new Vector3(horizontalInput * horizontalSpeed, 0, currentForwardSpeed);
        transform.Translate(movement * Time.deltaTime, Space.World);

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -limitX, limitX);
        transform.position = pos;

        if (animController != null)
        {
            // 🌟 如果正在打大王，傳入 0 讓下半身停止跑步動畫；反之則傳 1 繼續跑
            float verticalInput = isFightingBigBoss ? 0f : 1.0f;
            animController.UpdateMovementAnimation(horizontalInput, verticalInput);
        }
    }

    public float GetCurrentHorizontalInput()
    {
        return horizontalInput;
    }
}