using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float floatSpeed = 2f;
    public float fadeTime = 1f;

    // 🌟 飄移方向，預設往上；往右飄就傳 Vector3.right
    public Vector3 floatDirection = Vector3.up;

    private TMP_Text tmp;
    private float timer;
    private Color startColor;

    void Start()
    {
        tmp = GetComponentInChildren<TMP_Text>();

        if (tmp != null)
        {
            startColor = tmp.color;
        }
        else
        {
            Debug.LogError("找不到 TMP_Text 組件，請檢查 Prefab 設定！");
        }

        Destroy(gameObject, fadeTime);
    }

    void Update()
    {
        // 🌟 依照指定方向飄移
        transform.position += floatDirection * floatSpeed * Time.deltaTime;

        // 漸漸消失
        if (tmp != null)
        {
            timer += Time.deltaTime;
            float alpha = 1f - (timer / fadeTime);
            tmp.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
        }

        // 面向相機
        if (Camera.main != null)
        {
            Vector3 dirToCamera = Camera.main.transform.position - transform.position;
            transform.rotation = Quaternion.LookRotation(-dirToCamera);
        }
    }
}