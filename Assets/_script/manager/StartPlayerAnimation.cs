using UnityEngine;

public class StartPlayerAnimation : MonoBehaviour
{
    public PlayerAnimatorController animatorController;

    void Start()
    {
        if (animatorController != null && animatorController.animator != null)
        {
            // 修正後：直接透過控制器裡的 animator 組件，播放你當初在 Animator 視窗裡設定的待機動畫名稱
            animatorController.animator.Play("standing_idle_01");
        }
    }
}