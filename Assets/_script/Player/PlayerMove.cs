using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("移動速度設定")]
    public float forwardSpeed = 5f;
    public float horizontalSpeed = 5f;
    public float maxForwardSpeed = 12f;
    public float maxHorizontalSpeed = 12f;

    [Header("加速設定")]
    public float speedPerHundredMeters = 0.5f;

    [Header("跑道邊界限制")]
    public float limitX = 4.5f;

    [Header("大 Boss 戰鬥狀態")]
    public bool isFightingBigBoss = false; // 🌟 新增：用來判斷是不是正在打大王

    private PlayerAnimatorController animController;
    private float horizontalInput;
    public float CurrentForwardSpeed { get; private set; }
    public float CurrentHorizontalSpeed { get; private set; }

    void Start()
    {
        animController = GetComponent<PlayerAnimatorController>();
    }

    void Update()
    {
        float touchInput = 0f;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.position.x < Screen.width / 2f)
                touchInput = -1f;
            else
                touchInput = 1f;
        }

        float keyInput = Input.GetAxisRaw("Horizontal");

        // 🌟 觸控優先，有觸控就用觸控，沒有才用鍵盤
        horizontalInput = (Input.touchCount > 0) ? touchInput : keyInput;

        // 🌟 刪掉原本下面這行！
        // horizontalInput = Input.GetAxisRaw("Horizontal"); 

        float distance = transform.position.z - 30f;
        if (distance < 0) distance = 0;

        CurrentForwardSpeed = 0f;

        if (!isFightingBigBoss)
        {
            float extraSpeed = (distance / 100f) * speedPerHundredMeters;
            CurrentForwardSpeed = Mathf.Min(forwardSpeed + extraSpeed, maxForwardSpeed);
        }

        CurrentHorizontalSpeed = Mathf.Min(horizontalSpeed, maxHorizontalSpeed);
        Vector3 movement = new Vector3(horizontalInput * CurrentHorizontalSpeed, 0, CurrentForwardSpeed);
        transform.Translate(movement * Time.deltaTime, Space.World);

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -limitX, limitX);
        transform.position = pos;

        if (animController != null)
        {
            float verticalInput = isFightingBigBoss ? 0f : 1.0f;
            animController.UpdateMovementAnimation(horizontalInput, verticalInput);
        }
    }

    public float GetCurrentHorizontalInput()
    {
        return horizontalInput;
    }
}
