using UnityEngine;
using UnityEngine.Localization;

[RequireComponent(typeof(Collider2D))]
public class UpgradeZoneTrigger : MonoBehaviour
{
    public UpgradeManager upgradeManager;
    public PoolManager poolManager;
    DayNightManager dayNightManager;
    public LocalizedString InfoKey;
    public LocalizedString InfoFireKey;
    AudioSource source;
    public AudioClip upgradeClip;
    public AudioClip itemClip;
    string fireName = "Fire";

    private void Start()
    {
        if (upgradeManager == null)
            upgradeManager = GetComponent<UpgradeManager>();

        source = GetComponent<AudioSource>();
        dayNightManager = FindAnyObjectByType<DayNightManager>();
        poolManager = FindAnyObjectByType<PoolManager>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        ItemPickup2D item = other.GetComponent<ItemPickup2D>();
        if (item == null || item.itemData == null)
            return;

        // 아이템 등록
        bool collected = upgradeManager.AddCollectedItem(item);
        if (!collected)
            return;

        // 업그레이드 시도
        bool upgraded = upgradeManager.TryUpgradeAndCheckSuccess();

        // -----------------------------------------------------------------------------------
        // 레벨 업 메시지 처리
        // -----------------------------------------------------------------------------------

        // 레벨 1 → 2 업그레이드 성공
        if (upgraded && upgradeManager.CurrentLevel == 2)
        {
            source.PlayOneShot(upgradeClip);
            dayNightManager.ShowLocalizedMessage(InfoKey);
        }

        // 레벨 2 → 3 업그레이드 성공
        if (upgraded && upgradeManager.CurrentLevel == 3)
        {
            source.PlayOneShot(upgradeClip);

            if (gameObject.name == fireName)
            {
                dayNightManager.ShowLocalizedMessage(InfoFireKey);
            }
        }

        // 레벨 3 미만이면 아이템 회수
        if (upgradeManager.CurrentLevel < 3)
        {
            item.ForceDropBySystem();
            source.PlayOneShot(itemClip);

            if (poolManager != null)
            {
                poolManager.ReturnToPool(item.itemData.itemID, item.gameObject);
            }
            else
            {
                item.gameObject.SetActive(false);
            }
        }
    }
}
