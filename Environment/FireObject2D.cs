using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class FireObject2D : MonoBehaviour
{
    [Header("Fire Settings")]
    public GameObject fireEffect;

    // 연료 칸 수 (총 연료 최대량)
    // burnIntervalSeconds × requiredFuelCount 만큼 불이 지속됨
    public int requiredFuelCount = 5;

    // 연료 1칸이 타서 사라지는 데 걸리는 시간
    // 예: 3초 → 3초마다 1칸씩 연료 감소
    public float burnIntervalSeconds = 30f;

    [Header("UI")]
    public TextMeshProUGUI statusText;

    // 불 안에서 저장된 "대기중인 연료 아이템" 리스트
    // FULL일 때 들어온 아이템, 남은 시간이 찼을 때 차례대로 소비됨
    private List<ItemPickup2D> fuelItems = new List<ItemPickup2D>();

    // 불이 켜져 있는지 여부
    public bool fireStarted = false;
    private Coroutine burnCoroutine;

    // -----------------------------------
    // 연료 시간 계산용 변수들
    // -----------------------------------

    // 총 연료 시간 (requiredFuelCount × burnIntervalSeconds)
    // 예: 5칸 × 3초 = 15초
    private float totalBurnTime;

    // 현재 남은 연료 시간 (0 ~ totalBurnTime)
    private float remainingBurnTime;

    // burnIntervalSeconds 만큼 누적되면 1칸 연료가 빠진 것으로 계산됨
    private float currentBurnTimer;

    // -----------------------------------
    // 거북이은 연료로 취급하지 않음
    // -----------------------------------
    private const int turtleItemID = 204;  // Eat 타입의 204번 아이템은 무시

    // -----------------------------------
    // 알파(투명도) 업데이트 최적화용
    // -----------------------------------
    private Coroutine alphaRoutine;
    private WaitForSeconds alphaDelay = new WaitForSeconds(0.25f); // 0.25초마다 투명도 업데이트

    void OnEnable()
    {
        totalBurnTime = burnIntervalSeconds * requiredFuelCount;

        // 초기 연료는 MAX로 설정
        remainingBurnTime = totalBurnTime;

        // 첫 점화 (initial:false → 연료 1칸 제한 없이 그대로 시작)
        StartFire(initial: false);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        ItemPickup2D item = col.GetComponent<ItemPickup2D>();
        if (item == null || item.itemData == null) return;

        // 예외: 거북이(204)는 완전히 무시 (연료 아님)
        if (item.itemData.effectType == ItemEffectType.Eat &&
            item.itemData.itemID == turtleItemID)
            return;

        // 어떤 아이템이든 스택에 연료로 저장
        AddFuel(item);

        // 불이 꺼져 있다면 바로 점화 (연료 1칸)
        if (!fireStarted)
        {
            StartFire(initial: true);
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        ItemPickup2D item = col.GetComponent<ItemPickup2D>();
        if (item == null || item.itemData == null) return;

        // 예외 아이템은 fuelItems에 들어가지 않으므로 삭제할 필요 없음
        fuelItems.Remove(item);
    }



    // ====================================================================
    // Fuel 로직 어떤 아이템이든 연료로 취급
    // ====================================================================
    private void AddFuel(ItemPickup2D item)
    {
        PoolManager pm = FindAnyObjectByType<PoolManager>();

        // 연료 스택에 추가 (대기중인 아이템)
        if (!fuelItems.Contains(item))
            fuelItems.Add(item);

        // 불이 켜져있고 연료칸이 FULL이 아니면 즉시 연료 증가
        if (fireStarted && remainingBurnTime < totalBurnTime)
        {
            // 연료 1칸 증가
            remainingBurnTime += burnIntervalSeconds;
            remainingBurnTime = Mathf.Clamp(remainingBurnTime, 0, totalBurnTime);

            // 소비된 아이템은 비활성화(풀로 되돌림)
            pm.ReturnToPool(item.itemData.itemID, item.gameObject);
            fuelItems.Remove(item);
            return;
        }

        // FULL이면 → 연료 쌓아두기 (대기)
        // 불이 타면서 연료가 떨어졌을 때 FIFO 방식으로 소비됨
    }



    // ====================================================================
    // Fire Control
    // ====================================================================
    private void StartFire(bool initial)
    {
        fireStarted = true;

        PoolManager pm = FindAnyObjectByType<PoolManager>();

        if (initial)
        {
            // original 초기 점화 로직
            remainingBurnTime = burnIntervalSeconds;

            if (fuelItems.Count > 0)
            {
                ItemPickup2D first = fuelItems[0];
                pm.ReturnToPool(first.itemData.itemID, first.gameObject);
                fuelItems.RemoveAt(0);
            }
        }

        // initial:false일 경우 remainingBurnTime 유지됨 (5칸 그대로)

        currentBurnTimer = 0f;

        if (fireEffect != null)
        {
            fireEffect.SetActive(true);
            var sr = fireEffect.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color;
                c.a = 1f;
                sr.color = c;
            }
        }

        burnCoroutine = StartCoroutine(BurnFuelCycle());

        if (alphaRoutine != null)
            StopCoroutine(alphaRoutine);
        alphaRoutine = StartCoroutine(AlphaRoutine());
    }

    private void StopFire()
    {
        fireStarted = false;

        if (burnCoroutine != null)
            StopCoroutine(burnCoroutine);

        burnCoroutine = null;

        if (alphaRoutine != null)
            StopCoroutine(alphaRoutine);
        alphaRoutine = null;

        if (fireEffect != null)
            fireEffect.SetActive(false);
    }



    // ====================================================================
    // BurnFuelCycle 연료 소비 시스템
    // ====================================================================
    private IEnumerator BurnFuelCycle()
    {
        PoolManager pm = FindAnyObjectByType<PoolManager>();
        currentBurnTimer = 0f;

        while (fireStarted)
        {
            currentBurnTimer += Time.deltaTime;

            // 연료 1칸 소비 시간 도달
            if (currentBurnTimer >= burnIntervalSeconds)
            {
                currentBurnTimer = 0f;

                // 연료 1칸 감소
                remainingBurnTime -= burnIntervalSeconds;

                // FULL이 아니게 되었으므로 대기중 연료가 있다면 자동 소비
                if (remainingBurnTime < totalBurnTime)
                {
                    TryConsumeFuelInCollider();
                }

                // 모든 연료 소진 → 불 꺼짐
                if (remainingBurnTime <= 0f)
                {
                    StopFire();
                    yield break;
                }
            }

            yield return null;
        }
    }



    // ====================================================================
    // AlphaRoutine 불 밝기(투명도) 업데이트
    // ====================================================================
    private IEnumerator AlphaRoutine()
    {
        while (fireStarted)
        {
            UpdateAlphaByBurnTime();
            yield return alphaDelay; // 0.25초마다 갱신
        }
    }



    // ====================================================================
    // 대기중 연료 자동 소비
    // ====================================================================
    private void TryConsumeFuelInCollider()
    {
        if (fuelItems.Count == 0)
            return;

        PoolManager pm = FindAnyObjectByType<PoolManager>();

        // 가장 먼저 들어온 아이템부터 소비
        ItemPickup2D item = fuelItems[0];

        remainingBurnTime += burnIntervalSeconds;
        remainingBurnTime = Mathf.Clamp(remainingBurnTime, 0, totalBurnTime);

        pm.ReturnToPool(item.itemData.itemID, item.gameObject);
        fuelItems.RemoveAt(0);
    }



    // ====================================================================
    // 불 밝기 조절
    // ====================================================================
    private void UpdateAlphaByBurnTime()
    {
        if (fireEffect == null) return;

        SpriteRenderer sr = fireEffect.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        // 남은 연료 비율 → 알파값
        float alpha = remainingBurnTime / totalBurnTime;
        alpha = Mathf.Clamp01(alpha);

        Color c = sr.color;
        c.a = alpha;
        sr.color = c;
    }
}
