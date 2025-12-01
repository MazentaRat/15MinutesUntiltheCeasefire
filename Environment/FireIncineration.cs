using UnityEngine;

public class FireIncineration : MonoBehaviour
{
    PoolManager poolManager;
    public UpgradeManager upgradeManager;

    private void Start()
    {
        poolManager = FindAnyObjectByType<PoolManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 업그레이드 레벨 3 미만 → 아무 효과 없음
        if (upgradeManager == null || upgradeManager.CurrentLevel < 3)
            return;

        ItemPickup2D item = collision.GetComponent<ItemPickup2D>();
        if (item == null) return;
        if(item.itemData.itemID == 204) return;

        // 즉시 소각
        if (poolManager != null)
            poolManager.ReturnToPool(item.itemData.itemID, item.gameObject);
        else
            item.gameObject.SetActive(false);
    }
}
