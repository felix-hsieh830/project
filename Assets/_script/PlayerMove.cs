using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float forwardSpeed = 5f;
    public float sideSpeed = 5f;

    // 新增這行：設定左右的邊界距離是 4.5
    public float limitX = 4.5f;

    void Update()
    {
        // 1. 自動往前跑
        transform.Translate(0, 0, forwardSpeed * Time.deltaTime);

        // 2. 左右移動
        float h = Input.GetAxis("Horizontal");
        transform.Translate(h * sideSpeed * Time.deltaTime, 0, 0);

        Vector3 currentPos = transform.position;

        currentPos.x = Mathf.Clamp(currentPos.x, -limitX, limitX);

        // 最後，把修改好的座標，正式還給方塊
        transform.position = currentPos;
    }
}