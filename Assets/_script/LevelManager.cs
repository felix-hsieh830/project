using UnityEngine;
using System.Collections.Generic; // 要使用 List 必須加這一行

public class LevelManager : MonoBehaviour
{
    public GameObject trackPrefab;
    public Transform player;

    private float spawnZ = 0f;
    public float trackLength = 10f;

    // 這裡我們準備一個小本子，用來記錄畫面上目前有哪些賽道
    private List<GameObject> activeTracks = new List<GameObject>();

    // 設定畫面上最多只保留幾個賽道 (避免電腦卡死)
    private int maxTracksOnScreen = 7;

    void Start()
    {
        // 遊戲一開始，先鋪幾塊賽道
        for (int i = 0; i < maxTracksOnScreen; i++)
        {
            SpawnTrack();
        }
    }

    void Update()
    {
        // 如果玩家跑過了第一塊賽道... (預留兩塊的緩衝距離)
        if (player.position.z > (spawnZ - maxTracksOnScreen * trackLength) + (trackLength * 2))
        {
            SpawnTrack();    // 前面蓋一塊新的
            DeleteTrack();   // 後面拆一塊舊的
        }
    }

    void SpawnTrack()
    {
        // 蓋出新賽道
        GameObject newTrack = Instantiate(trackPrefab, new Vector3(0, 0, spawnZ), Quaternion.identity);

        // 把新蓋好的賽道記在小本子裡
        activeTracks.Add(newTrack);

        spawnZ += trackLength;
    }

    void DeleteTrack()
    {
        // 把小本子裡「最舊的」那一塊 (也就是第 0 個) 刪除
        Destroy(activeTracks[0]);

        // 從小本子紀錄上劃掉它
        activeTracks.RemoveAt(0);
    }
}