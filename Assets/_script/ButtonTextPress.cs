using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro; // 🌟 新增這一行：必須引入這個才能控制 TMP 文字！

public class ButtonTextPress : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("要把誰往下壓？(拖曳 Text(TMP) 到這裡)")]
    public RectTransform textRectTransform;
    
    [Header("按下去要往下移動多少像素？")]
    public float pressOffsetY = -5f;

    [Header("🌟 按下去時的文字顏色 (建議調暗)")]
    public Color pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f); // 預設為暗灰色

    [Header("按鈕底圖按下狀態")]
    public Image buttonImage;
    public Sprite normalSprite;
    public Sprite pressedSprite;

    private Vector3 originalPosition;
    private TextMeshProUGUI tmpText; // 用來抓取文字組件
    private Color originalColor;     // 用來記住文字原本的顏色
    private bool isPressed = false;

    void Start()
    {
        RefreshOriginalState();
    }

    public void RefreshOriginalState()
    {
        if (buttonImage == null)
        {
            buttonImage = GetComponent<Image>();
        }

        if (buttonImage != null && normalSprite == null)
        {
            normalSprite = buttonImage.sprite;
        }

        if (textRectTransform != null)
        {
            // 1. 記住原本的位置
            originalPosition = textRectTransform.localPosition;
            
            // 2. 抓取文字組件，並記住它原本的顏色
            tmpText = textRectTransform.GetComponent<TextMeshProUGUI>();
            if (tmpText != null)
            {
                originalColor = tmpText.color;
            }
        }
    }

    // 當滑鼠「按下去」的瞬間
    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;

        // 往下壓
        if (textRectTransform != null)
        {
            textRectTransform.localPosition = originalPosition + new Vector3(0, pressOffsetY, 0);
        }
        
        // 🌟 文字變暗
        if (tmpText != null)
        {
            tmpText.color = pressedColor;
        }

        if (buttonImage != null && pressedSprite != null)
        {
            buttonImage.sprite = pressedSprite;
        }
    }

    // 當滑鼠「放開」的瞬間
    public void OnPointerUp(PointerEventData eventData)
    {
        ResetPressState();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isPressed)
        {
            ResetPressState();
        }
    }

    void OnDisable()
    {
        ResetPressState();
    }

    private void ResetPressState()
    {
        isPressed = false;

        if (textRectTransform != null)
        {
            textRectTransform.localPosition = originalPosition;
        }
        
        // 🌟 文字恢復原本的顏色
        if (tmpText != null)
        {
            tmpText.color = originalColor;
        }

        if (buttonImage != null && normalSprite != null)
        {
            buttonImage.sprite = normalSprite;
        }
    }
}
