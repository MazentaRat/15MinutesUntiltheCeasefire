using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PoolManager : MonoBehaviour
{
    [Header("아이템 데이터 리스트")]
    public List<ItemData> allItemData = new List<ItemData>();

    public Dictionary<int, Queue<GameObject>> pools = new Dictionary<int, Queue<GameObject>>();

    [Header("총알 프리팹 & 풀 크기")]
    public GameObject bulletPrefab;
    public int bulletPoolSize = 100;

    private Queue<GameObject> bulletPool = new Queue<GameObject>();

    [Header("Soldier Prefab")]
    public GameObject soldierPrefab;
    public int soldierPoolSize = 10;

    private Queue<GameObject> soldierPool = new Queue<GameObject>();

    private void Awake()
    {
        InitializePools();
        InitializeSoldierPool();
        InitializeBulletPool();
    }

    private void InitializePools()
    {
        pools.Clear();

        foreach (var data in allItemData)
        {
            if (data == null || data.prefab == null) continue;
            if (pools.ContainsKey(data.itemID))
            {
                Debug.LogWarning($"[PoolManager] 중복된 itemID 감지: {data.itemID}");
                continue;
            }

            Queue<GameObject> pool = new Queue<GameObject>();
            for (int i = 0; i < data.poolSize; i++)
            {
                GameObject obj = Instantiate(data.prefab, transform);
                obj.SetActive(false);
                pool.Enqueue(obj);
            }

            pools.Add(data.itemID, pool);
        }
    }

    private void InitializeSoldierPool()
    {
        for (int i = 0; i < soldierPoolSize; i++)
        {
            GameObject s = Instantiate(soldierPrefab);
            s.SetActive(false);
            soldierPool.Enqueue(s);
        }
    }

    public GameObject GetSoldier()
    {
        if (soldierPool.Count == 0) return null;

        GameObject s = soldierPool.Dequeue();
        s.SetActive(true);
        return s;
    }

    public void ReturnSoldier(GameObject s)
    {
        s.SetActive(false);
        soldierPool.Enqueue(s);
    }

    private void InitializeBulletPool()
    { // 총알 풀 초기화
        bulletPool.Clear();
        if (bulletPrefab == null) return;

        for (int i = 0; i < bulletPoolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab, transform);
            bullet.SetActive(false);
            bulletPool.Enqueue(bullet);
        }
    }
    public GameObject GetBullet()
    { // 총알 꺼내기
        if (bulletPool.Count > 0)
        {
            GameObject b = bulletPool.Dequeue();
            b.SetActive(true);
            return b;
        }

        // 만약 다 쓰면 예외적으로 추가 생성
        GameObject extra = Instantiate(bulletPrefab, transform);
        extra.SetActive(true);
        return extra;
    }

    public void ReturnBullet(GameObject bullet)
    { // 총알 반환
        bullet.SetActive(false);
        bullet.transform.SetParent(transform);
        bulletPool.Enqueue(bullet);
    }

    public GameObject GetFromPool(int itemID)
    {
        if (!pools.ContainsKey(itemID)) return null;

        GameObject obj = null;

        if (pools[itemID].Count > 0)
            obj = pools[itemID].Dequeue();
        else
        {
            var data = allItemData.FirstOrDefault(x => x.itemID == itemID);
            obj = Instantiate(data.prefab, transform);
        }

        obj.SetActive(true);

        // 초기화
        var pickup = obj.GetComponent<ItemPickup2D>();
        if (pickup != null)
            pickup.ResetPickupState();

        return obj;
    }

    public void ReturnToPool(int itemID, GameObject obj)
    {
        if (obj == null) return;

        if (!pools.ContainsKey(itemID))
        {
            Destroy(obj);
            return;
        }

        obj.SetActive(false);
        obj.transform.SetParent(transform);
        pools[itemID].Enqueue(obj);
    }

    public ItemData GetItemData(int itemID)
    {
        return allItemData.FirstOrDefault(x => x.itemID == itemID);
    }
}
