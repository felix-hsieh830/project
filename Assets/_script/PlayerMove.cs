using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float forwardSpeed = 5f; // 這個現在當作你的「初始起步速度」
    public float sideSpeed = 5f;

    // 設定左右的邊界距離是 4.5
    public float limitX = 4.5f;

    void Update()
    {
        // 🌟 1. 加速引擎：利用 Z 軸距離計算當下速度
        float distance = transform.position.z;
        if (distance < 0) distance = 0; // 避免剛開局在新手村時減速

        // 假設每跑 100 公尺，速度就增加 1.5
        float extraSpeed = (distance / 100f) * 1.1f; 
        
        // 最終的衝刺速度 = 初始速度 + 額外獲得的速度
        float currentSpeed = forwardSpeed + extraSpeed; 

        // 2. 移動邏輯
        // 自動往前跑
        transform.Translate(0, 0, currentSpeed * Time.deltaTime);

        // 左右移動
        float h = Input.GetAxis("Horizontal");
        transform.Translate(h * sideSpeed * Time.deltaTime, 0, 0);

        // 3. 邊界防護
        Vector3 currentPos = transform.position;
        currentPos.x = Mathf.Clamp(currentPos.x, -limitX, limitX);
        
        // 最後，把修改好的座標，正式還給方塊
        transform.position = currentPos;
    }
}