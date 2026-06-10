using UnityEngine;

public class BossRewardChest : MonoBehaviour
{
    public bool isBigBoss = false;
    private bool hasTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        PlayerStats stats = other.GetComponent<PlayerStats>();
        if (stats != null)
        {
            hasTriggered = true;
            SfxManager.Play("reward", 0.85f, 0.08f);
            GameManager gm = FindAnyObjectByType<GameManager>();
            if (gm != null) gm.ShowReward(isBigBoss);
            Destroy(gameObject);
        }
    }
}
