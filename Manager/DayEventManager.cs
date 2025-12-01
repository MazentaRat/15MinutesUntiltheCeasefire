using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class TimedEvent
{
    [Range(0f, 1f)]
    public float triggerPercent;
    public UnityEvent onTrigger;
    [HideInInspector] public bool triggered = false;
}

public class DayEventManager : MonoBehaviour
{
    public List<TimedEvent> timedEvents = new List<TimedEvent>();

    private PoolManager pool;
    private ItemManager itemManager;

    public SpriteRenderer[] bloodBeach;
    float fadeInDuration = 5f;
    float fadeOutDuration = 5f;

    WaitForSeconds ws90 = new WaitForSeconds(30f);

    public GameObject lights;

    void Awake()
    {
        pool = FindAnyObjectByType<PoolManager>();
        itemManager = FindAnyObjectByType<ItemManager>();

        if (lights != null)
            lights.SetActive(false);
    }

    public void CheckProgress(float progress)
    {
        for (int i = 0; i < timedEvents.Count; i++)
        {
            var e = timedEvents[i];
            if (e.triggered) continue;

            if (progress >= e.triggerPercent)
            {
                e.triggered = true;
                e.onTrigger?.Invoke();
            }
        }
    }

    public void ResetEvents()
    {
        foreach (var e in timedEvents)
            e.triggered = false;
    }

    public void SpawnItem(int itemNum)
    {
        if (pool == null || itemManager == null) return;

        int id = itemNum;

        ItemData data = itemManager.poolManager.allItemData
            .Find(d => d.itemID == id);

        if (data == null) return;

        itemManager.SpawnItem(data);
    }

    public void BloodBeach()
    {
        StartCoroutine(BloodBeachRoutine());
    }

    private IEnumerator BloodBeachRoutine()
    {
        DayNightManager dm = FindAnyObjectByType<DayNightManager>();
        ItemManager im = FindAnyObjectByType<ItemManager>();

        // 조명 Freeze
        if (dm != null)
            dm.FreezeLight();

        // 스폰량 2배 활성화
        if (im != null)
            im.extraSpawnMultiplier = 2;

        if (lights != null)
            lights.SetActive(true);

        // 초기 알파 0
        foreach (var s in bloodBeach)
        {
            if (s == null) continue;
            Color c = s.color;
            c.a = 0f;
            s.color = c;
        }

        // 페이드 인
        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / fadeInDuration);

            foreach (var s in bloodBeach)
            {
                if (s == null) continue;
                Color c = s.color;
                c.a = a;
                s.color = c;
            }

            yield return null;
        }

        // 유지
        yield return ws90;

        if (lights != null)
            lights.SetActive(false);

        // 페이드 아웃
        t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            float a = 1f - Mathf.Clamp01(t / fadeOutDuration);

            foreach (var s in bloodBeach)
            {
                if (s == null) continue;
                Color c = s.color;
                c.a = a;
                s.color = c;
            }

            yield return null;
        }

        // 마지막 0 보정
        foreach (var s in bloodBeach)
        {
            if (s == null) continue;
            Color c = s.color;
            c.a = 0f;
            s.color = c;
        }

        // 스폰량 원상복구
        if (im != null)
            im.extraSpawnMultiplier = 1;

        // 조명 Unfreeze
        if (dm != null)
            dm.UnfreezeLight();
    }
    public void SpawnSoldiers()
    {
        if (itemManager == null) return;
        itemManager.SpawnSoldiersRoutine(10);
    }
}
