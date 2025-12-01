using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ItemSpawnEntry
{
    public int itemID;
    public int spawnCount;

    public ItemSpawnEntry(int id, int count)
    {
        itemID = id;
        spawnCount = count;
    }
}

public class ItemManager : MonoBehaviour
{
    public PoolManager poolManager;

    public float minX = -8f;
    public float maxX = 8f;
    public float spawnY = 5f;

    public float baseFallSpeed = 2f;
    public float fallSpeedRandomRange = 0.3f;
    public float minTargetY = 1.8f;
    public float maxTargetY = 2.3f;

    public float totalSpawnDuration = 600f;

    public bool logSpawnInfo = true;
    private bool isSpawning = false;

    private List<ItemData> allItems = new List<ItemData>();
    private Dictionary<int, int> remainingSpawnCount = new Dictionary<int, int>();

    // BloodBeach 동안 추가 스폰 배율
    public int extraSpawnMultiplier = 1;

    public float soldierSpawnXMin = -8f;
    public float soldierSpawnXMax = 8f;
    public float soldierSpawnY = 5f;

    public float soldierFallSpeed = 2f;
    public float soldierTargetY = 2f;
    public Coroutine soldierRoutine;
    WaitForSeconds ws1 = new WaitForSeconds(1f);

    [Header("Spawn Curve Settings")]
    float earlySpawnMultiplier = 1.5f; // 초반 스폰
    float lateSpawnMultiplier = 0.25f;  // 후반 스폰

    void Awake()
    {
        if (poolManager == null)
            poolManager = FindAnyObjectByType<PoolManager>();
    }

    void Start()
    {
        allItems = poolManager.allItemData;

        SetupSpawnList();
        // StartCoroutine(SpawnLoop());
    }

    public void StartSpawning()
    {
        if (isSpawning) return; // 중복 방지
        isSpawning = true;

        StartCoroutine(SpawnLoop());
    }

    private void SetupSpawnList()
    {
        remainingSpawnCount.Clear();

        AddSpawn(100, 12);
        AddSpawn(101, 12);
        AddSpawn(102, 15);
        AddSpawn(103, 12);
        AddSpawn(104, 12);
        AddSpawn(105, 12);
        AddSpawn(106, 15);
        AddSpawn(107, 12);
        AddSpawn(108, 12);
        AddSpawn(200, 12);
        AddSpawn(201, 12);
        AddSpawn(202, 12);
        AddSpawn(203, 12);
        AddSpawn(300, 20);
        AddSpawn(301, 20);
        AddSpawn(302, 12);
        AddSpawn(303, 12);
        AddSpawn(304, 12);
        AddSpawn(400, 15);
        AddSpawn(600, 12);
        AddSpawn(601, 2);
        AddSpawn(602, 12);
        AddSpawn(700, 20);
        AddSpawn(701, 12);
        AddSpawn(702, 12);
        AddSpawn(703, 12);
        AddSpawn(800, 15);
        AddSpawn(801, 15);
        AddSpawn(802, 15);
        AddSpawn(803, 15);
        AddSpawn(804, 15);
        AddSpawn(805, 15);
        AddSpawn(806, 15);
        AddSpawn(807, 15);
        AddSpawn(808, 15);
        AddSpawn(809, 15);
        AddSpawn(900, 15);
        AddSpawn(901, 15);
        AddSpawn(902, 15);
        AddSpawn(903, 15);
        AddSpawn(904, 15);
    }

    private void AddSpawn(int itemID, int count)
    {
        if (!remainingSpawnCount.ContainsKey(itemID))
            remainingSpawnCount.Add(itemID, count);
        else
            remainingSpawnCount[itemID] += count;
    }

    IEnumerator SpawnLoop()
    {
        if (remainingSpawnCount.Count == 0)
        {
            Debug.LogWarning("[ItemManager] No spawn items defined.");
            yield break;
        }

        int totalSpawnLeft = remainingSpawnCount.Values.Sum();
        float elapsed = 0f;

        while (totalSpawnLeft > 0 && elapsed < totalSpawnDuration)
        {
            float remainingTime = Mathf.Max(0f, totalSpawnDuration - elapsed);
            float avgInterval = remainingTime / Mathf.Max(1, totalSpawnLeft);

            float spawnProgress = elapsed / totalSpawnDuration;
            float currentMultiplier = Mathf.Lerp(earlySpawnMultiplier, lateSpawnMultiplier, spawnProgress);

            float dynamicInterval = avgInterval * currentMultiplier;

            float waitTime = Random.Range(dynamicInterval * 0.7f, dynamicInterval * 1.3f);

            yield return new WaitForSeconds(waitTime);
            elapsed += waitTime;

            // 스폰 로직
            var candidates = allItems
                .Where(d => remainingSpawnCount.ContainsKey(d.itemID) && remainingSpawnCount[d.itemID] > 0)
                .ToList();

            if (candidates.Count == 0)
                break;

            ItemData randomItem = candidates[Random.Range(0, candidates.Count)];

            SpawnItem(randomItem);
            remainingSpawnCount[randomItem.itemID]--;

            // 추가 스폰 (감소 없음)
            for (int i = 1; i < extraSpawnMultiplier; i++)
                SpawnItem(randomItem);

            totalSpawnLeft = remainingSpawnCount.Values.Sum();
        }
    }


    public void SpawnItem(ItemData data)
    {
        if (poolManager == null || data == null || data.prefab == null)
            return;

        GameObject item = poolManager.GetFromPool(data.itemID);
        if (item == null) return;

        float randomX = Random.Range(minX, maxX);
        Vector3 spawnPos = new Vector3(randomX, spawnY, 0f);

        item.transform.position = spawnPos;
        item.transform.rotation = Quaternion.identity;
        item.transform.localScale = Vector3.one;
        item.SetActive(true);

        float targetY = Random.Range(minTargetY, maxTargetY);
        float fallSpeed = baseFallSpeed + Random.Range(-fallSpeedRandomRange, fallSpeedRandomRange);

        StartCoroutine(FallDown(item, targetY, fallSpeed));
    }

    IEnumerator FallDown(GameObject obj, float targetY, float speed)
    {
        while (obj.activeInHierarchy && obj.transform.position.y > targetY)
        {
            obj.transform.Translate(Vector3.down * speed * Time.deltaTime);
            yield return null;
        }
    }
    public void SpawnSoldierOnce()
    {
        if (poolManager == null || poolManager.soldierPrefab == null) return;

        GameObject soldier = poolManager.GetSoldier();
        if (soldier == null) return;

        float randomX = Random.Range(soldierSpawnXMin, soldierSpawnXMax);
        soldier.transform.position = new Vector3(randomX, soldierSpawnY, 0f);
        soldier.transform.rotation = Quaternion.identity;
        soldier.transform.localScale = Vector3.one;
        soldier.SetActive(true);

        StartCoroutine(FallDownSoldier(soldier, soldierTargetY, soldierFallSpeed));
    }

    IEnumerator FallDownSoldier(GameObject obj, float targetY, float speed)
    {
        while (obj.activeInHierarchy && obj.transform.position.y > targetY)
        {
            obj.transform.Translate(Vector3.down * speed * Time.deltaTime);
            yield return null;
        }
    }
    public void SpawnSoldiersRoutine(int count = 10)
    {
        if (soldierRoutine != null) StopCoroutine(soldierRoutine);
        soldierRoutine = StartCoroutine(SoldierSpawnRoutine(count));
    }

    private IEnumerator SoldierSpawnRoutine(int count)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnSoldierOnce();
            yield return ws1;
        }
    }

    public void SpawnItemAtX(ItemData data, float x)
    {
        if (poolManager == null || data == null || data.prefab == null)
            return;

        GameObject item = poolManager.GetFromPool(data.itemID);
        if (item == null) return;

        // Y는 기존 spawnY 그대로 사용
        Vector3 spawnPos = new Vector3(x, spawnY, 0);

        item.transform.position = spawnPos;
        item.transform.rotation = Quaternion.identity;
        item.transform.localScale = Vector3.one;
        item.SetActive(true);

        // FallDown 로직 동일
        float targetY = Random.Range(minTargetY, maxTargetY);
        float fallSpeed = baseFallSpeed + Random.Range(-fallSpeedRandomRange, fallSpeedRandomRange);

        StartCoroutine(FallDown(item, targetY, fallSpeed));
    }
}
