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
    private bool suppressRewards;

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
        if (activeTracks.Count == 0) return;

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
        if (suppressRewards) ClearTrackRewards(newTrack);
        activeTracks.Add(newTrack);
        spawnZ += trackLength;
    }

    // 原本鋪危險地板的函數
    void SpawnTrack()
    {
        GameObject newTrack = Instantiate(trackPrefab, new Vector3(0, 0, spawnZ), Quaternion.identity);
        if (suppressRewards) ClearTrackRewards(newTrack);
        activeTracks.Add(newTrack);
        spawnZ += trackLength;
    }

    void DeleteTrack()
    {
        Destroy(activeTracks[0]);
        activeTracks.RemoveAt(0);
    }

    public void BeginBigBossArena(float bossZ)
    {
        suppressRewards = true;
        float playerZ = player != null ? player.position.z : bossZ;
        ClearRewardsInRange(playerZ - 10f, bossZ + 12f);
        ClearEnemiesInRange(playerZ - 8f, bossZ + 12f);
    }

    public void EndBigBossArena()
    {
        suppressRewards = false;
    }

    private void ClearTrackRewards(GameObject track)
    {
        if (track == null) return;

        Transform gateGroup = track.transform.Find("GateGroup");
        if (gateGroup != null)
        {
            Destroy(gateGroup.gameObject);
        }

        TreasureChest[] chests = track.GetComponentsInChildren<TreasureChest>(true);
        for (int i = 0; i < chests.Length; i++)
        {
            if (chests[i] != null) Destroy(chests[i].gameObject);
        }
    }

    private void ClearRewardsInRange(float minZ, float maxZ)
    {
        HashSet<GameObject> targets = new HashSet<GameObject>();

        BuffGate[] gates = FindObjectsByType<BuffGate>(FindObjectsSortMode.None);
        for (int i = 0; i < gates.Length; i++)
        {
            if (gates[i] == null) continue;
            float z = gates[i].transform.position.z;
            if (z < minZ || z > maxZ) continue;

            GameObject target = gates[i].transform.parent != null ? gates[i].transform.parent.gameObject : gates[i].gameObject;
            targets.Add(target);
        }

        TreasureChest[] chests = FindObjectsByType<TreasureChest>(FindObjectsSortMode.None);
        for (int i = 0; i < chests.Length; i++)
        {
            if (chests[i] == null) continue;
            float z = chests[i].transform.position.z;
            if (z < minZ || z > maxZ) continue;
            targets.Add(chests[i].gameObject);
        }

        foreach (GameObject target in targets)
        {
            if (target != null) Destroy(target);
        }
    }

    private void ClearEnemiesInRange(float minZ, float maxZ)
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] == null) continue;
            float z = enemies[i].transform.position.z;
            if (z < minZ || z > maxZ) continue;
            Destroy(enemies[i].gameObject);
        }
    }
}
