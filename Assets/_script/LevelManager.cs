using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public GameObject trackPrefab;
    public GameObject safeTrackPrefab;
    public Transform player;

    private float spawnZ = 0f;
    public float trackLength = 10f;

    private List<GameObject> activeTracks = new List<GameObject>();
    private int maxTracksOnScreen = 7;

    void Start()
    {
        // 遊戲一開始，先鋪幾塊賽道
        for (int i = 0; i < maxTracksOnScreen; i++)
        {

            if (i < 1)
            {
                SpawnSafeTrack();
            }
            else
            {
                SpawnTrack();
            }
        }
    }

    void Update()
    {
        if (player.position.z > (spawnZ - maxTracksOnScreen * trackLength) + (trackLength * 2))
        {
            SpawnTrack();    // 前面蓋一塊新的 (遊戲開始後蓋的都有怪)
            DeleteTrack();   // 後面拆一塊舊的
        }
    }

    // 專門鋪安全地板的函數
    void SpawnSafeTrack()
    {
        GameObject newTrack = Instantiate(safeTrackPrefab, new Vector3(0, 0, spawnZ), Quaternion.identity);
        activeTracks.Add(newTrack);
        spawnZ += trackLength;
    }

    // 原本鋪危險地板的函數
    void SpawnTrack()
    {
        GameObject newTrack = Instantiate(trackPrefab, new Vector3(0, 0, spawnZ), Quaternion.identity);
        activeTracks.Add(newTrack);
        spawnZ += trackLength;
    }

    void DeleteTrack()
    {
        Destroy(activeTracks[0]);
        activeTracks.RemoveAt(0);
    }
}