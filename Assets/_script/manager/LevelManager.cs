using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public GameObject trackPrefab;
    public GameObject safeTrackPrefab;
    public Transform player;

    private float spawnZ = 0f;
    public float trackLength = 30f;

    private List<GameObject> activeTracks = new List<GameObject>();
    private int maxTracksOnScreen = 5;

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
        if (player.position.z > activeTracks[0].transform.position.z + trackLength * 2)
        {
            DeleteTrack();
            SpawnTrack();
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