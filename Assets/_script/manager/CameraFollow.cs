using System.Collections;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;

    [Header("視角設定")]
    public Vector3 defaultOffset = new Vector3(0, 9.6f, -5.5f); // 🌟 已換成你的完美數字
    public Vector3 bossOffset = new Vector3(0, 2f, -6f);
    public Vector3 bossRotation = new Vector3(10, 0, 0);

    private Vector3 currentOffset;
    private Quaternion defaultRotation;

    void Start()
    {
        currentOffset = defaultOffset;
        defaultRotation = transform.rotation;

        // 🌟 開局強制對齊：遊戲第 0 秒直接把鏡頭釘死在這個位置，消滅所有滑動感
        if (player != null)
        {
            transform.position = new Vector3(
                transform.position.x,
                currentOffset.y,
                player.position.z + currentOffset.z
            );
        }
    }

    void LateUpdate()
    {
        if (player == null) return;

        // 計算完美的目標位置
        Vector3 targetPosition = new Vector3(
            transform.position.x,
            currentOffset.y,                       // 鎖死的高度
            player.position.z + currentOffset.z    // 鎖死的 Z 軸距離
        );

        // 🌟 核心修改：把 Vector3.Lerp 拔掉！直接強制等於目標位置
        // 這樣就不會有橡皮筋拉扯感，開局絕對不會再前後滑動！
        transform.position = targetPosition;
    }

    public void SwitchToBossCamera()
    {
        StopAllCoroutines();
        StartCoroutine(TransitionCamera(bossOffset, Quaternion.Euler(bossRotation)));
    }

    public void SwitchToNormalCamera()
    {
        StopAllCoroutines();
        StartCoroutine(TransitionCamera(defaultOffset, defaultRotation));
    }

    IEnumerator TransitionCamera(Vector3 targetOffset, Quaternion targetRot)
    {
        float duration = 2.0f;
        float elapsed = 0f;
        Vector3 startOffset = currentOffset;
        Quaternion startRot = transform.rotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t);

            currentOffset = Vector3.Lerp(startOffset, targetOffset, t);
            transform.rotation = Quaternion.Lerp(startRot, targetRot, t);
            yield return null;
        }
        currentOffset = targetOffset;
        transform.rotation = targetRot;
    }
}