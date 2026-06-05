using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float floatSpeed = 2f;
    public float fadeTime = 1f;
    
    // 將 TextMeshPro 更改為通用的 TMP_Text
    private TMP_Text tmp; 
    private float timer;
    private Color startColor;

    void Start()
    {
        // 改用 GetComponentInChildren，確保即便文字在子物件也能抓到
        tmp = GetComponentInChildren<TMP_Text>();
        
        // 加入防呆機制，確認有抓到組件才執行後續設定
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
        // 往上飄
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;
        
        // 漸漸消失 (確保 tmp 存在才修改顏色)
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