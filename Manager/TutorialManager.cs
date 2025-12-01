using UnityEngine;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    [Header("튜토리리얼 스폰 아이템")]
    public List<ItemSpawnEntry> tutorialItems;

    [Header("튜토리얼 Read 아이템 설정")]
    int readItemID = 401;   // 신문 ID
    float readSpawnX = 5f;  // 스폰될 X 좌표

    [Header("튜토리얼 상태")]
    public bool tutorialStarted = false;
    public bool tutorialFinished = false;
    private int upgradeSuccessCount = 0;

    private PoolManager pool;
    private ItemManager itemManager;
    private PlayerStatus player;
    private DayNightManager dayNight;

    private void Awake()
    {
        pool = FindAnyObjectByType<PoolManager>();
        itemManager = FindAnyObjectByType<ItemManager>();
        player = FindAnyObjectByType<PlayerStatus>();
        dayNight = FindAnyObjectByType<DayNightManager>();
    }

    private void Start()
    {
        StartTutorial();
    }

    // ======================================================================
    // 튜토리얼 시작
    // ======================================================================
    public void StartTutorial()
    {
        if (tutorialStarted) return;
        tutorialStarted = true;

        // SpawnTutorialItems();
        // 시간, 굶주림, 온도는 시작하지 않음
    }

    private void SpawnTutorialItems()
    {
        if (pool == null || itemManager == null) return;

        foreach (var entry in tutorialItems)
        {
            ItemData data = pool.allItemData.Find(d => d.itemID == entry.itemID);
            if (data == null) continue;

            for (int i = 0; i < entry.spawnCount; i++)
                itemManager.SpawnItem(data);
        }
    }

    // ======================================================================
    // 업그레이드 성공 보고
    // ======================================================================
    public void ReportUpgradeSuccess()
    {
        if (tutorialFinished) return;

        upgradeSuccessCount++;

        if (upgradeSuccessCount >= 3)
        {
            SpawnReadItem();
            tutorialFinished = true;
        }
    }

    // ======================================================================
    // Read 아이템을 특정 위치에 스폰
    // ======================================================================
    private void SpawnReadItem()
    {
        if (pool == null || itemManager == null) return;

        ItemData data = pool.allItemData.Find(d => d.itemID == readItemID);
        if (data == null) return;

        // 기존 랜덤 대신, 지정한 X 좌표로 위에서 떨어뜨림
        itemManager.SpawnItemAtX(data, readSpawnX);
    }

    // ======================================================================
    // Read 소비 후 정식 게임 시작
    // ======================================================================
    public void OnReadConsumed()
    {
        if (!tutorialFinished) return;

        dayNight?.StartCycle();
        itemManager?.StartSpawning();
        player?.StartRoutines();
    }
}
