using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour
{
    [Header("업그레이드 대상")]
    public UpgradeTarget upgradeTarget;

    [Header("각 단계별 요구 아이템")]
    public List<UpgradeRequirement> toLevel2Requirements;
    public List<UpgradeRequirement> toLevel3Requirements;

    [Header("UI 연동")]
    [Tooltip("레벨2 요구 아이템 UI 텍스트 순서대로")]
    public List<TextMeshProUGUI> level2ProgressTexts;
    [Tooltip("레벨2 요구 아이템 UI 이미지 순서대로")]
    public List<Image> level2ProgressImages;

    [Tooltip("레벨3 요구 아이템 UI 텍스트 순서대로")]
    public List<TextMeshProUGUI> level3ProgressTexts;
    [Tooltip("레벨3 요구 아이템 UI 이미지 순서대로")]
    public List<Image> level3ProgressImages;

    public GameObject lv1UI;
    public GameObject lv2UI;

    [Header("현재 레벨")]
    [SerializeField] private int currentLevel = 1;
    public int CurrentLevel => currentLevel;

    // 수집 상태: "ItemEffectType_itemID" => 수량
    private Dictionary<string, int> collectedItems = new Dictionary<string, int>();

    private void Start()
    {
        upgradeTarget.SetActiveLevel(currentLevel);
        UpdateUI();
    }

    // 아이템 수집만 수행. 업그레이드는 여기서 하지 않음.
    public bool AddCollectedItem(ItemPickup2D item)
    {
        if (currentLevel >= 3 || item == null || item.itemData == null)
            return false;

        List<UpgradeRequirement> required =
            (currentLevel == 1) ? toLevel2Requirements : toLevel3Requirements;

        bool matched = false;

        foreach (var r in required)
        {
            if (r.itemType == item.itemData.effectType &&
                r.itemID == item.itemData.itemID)
            {
                matched = true;

                string key = r.itemType.ToString() + "_" + r.itemID;

                if (!collectedItems.ContainsKey(key))
                    collectedItems[key] = 0;

                collectedItems[key]++;
                break;
            }
        }

        if (!matched)
            return false;

        UpdateUI();
        return true;
    }

    private void TryUpgrade()
    {
        if (currentLevel >= 3)
            return;

        List<UpgradeRequirement> required =
            (currentLevel == 1) ? toLevel2Requirements : toLevel3Requirements;

        // 요구 아이템 충족 여부 검사
        foreach (var r in required)
        {
            string key = r.itemType.ToString() + "_" + r.itemID;
            int count = collectedItems.ContainsKey(key) ? collectedItems[key] : 0;

            if (count < r.amountRequired)
                return; // 아직 조건 부족 → 업그레이드 실패
        }

        // 아이템 소모
        foreach (var r in required)
        {
            string key = r.itemType.ToString() + "_" + r.itemID;

            if (collectedItems.ContainsKey(key))
            {
                collectedItems[key] -= r.amountRequired;

                if (collectedItems[key] <= 0)
                    collectedItems.Remove(key);
            }
        }

        currentLevel++;
        upgradeTarget.SetActiveLevel(currentLevel);
        UpdateUI();
    }

    // 업그레이드 시도 후 성공 여부 반환
    public bool TryUpgradeAndCheckSuccess()
    {
        int before = currentLevel;
        TryUpgrade();
        bool success = currentLevel > before;

        if (success)
        {
            TutorialManager tm = FindAnyObjectByType<TutorialManager>();
            tm?.ReportUpgradeSuccess();
        }

        return success;
    }

    private void UpdateUI()
    {
        // UI 활성/비활성
        if (currentLevel == 1)
        {
            if (lv1UI != null) lv1UI.SetActive(true);
            if (lv2UI != null) lv2UI.SetActive(false);
        }
        else if (currentLevel == 2)
        {
            if (lv1UI != null) lv1UI.SetActive(false);
            if (lv2UI != null) lv2UI.SetActive(true);
        }
        else
        {
            if (lv1UI != null) lv1UI.SetActive(false);
            if (lv2UI != null) lv2UI.SetActive(false);
        }

        // UI 선택
        List<UpgradeRequirement> required;
        List<TextMeshProUGUI> uiTexts;
        List<Image> uiImages;

        if (currentLevel == 1)
        {
            required = toLevel2Requirements;
            uiTexts = level2ProgressTexts;
            uiImages = level2ProgressImages;
        }
        else if (currentLevel == 2)
        {
            required = toLevel3Requirements;
            uiTexts = level3ProgressTexts;
            uiImages = level3ProgressImages;
        }
        else return;

        // UI 갱신
        for (int i = 0; i < required.Count; i++)
        {
            if (i >= uiTexts.Count || i >= uiImages.Count)
                continue;

            var r = required[i];
            string key = r.itemType.ToString() + "_" + r.itemID;

            int collected =
                collectedItems.ContainsKey(key) ? collectedItems[key] : 0;

            collected = Mathf.Min(collected, r.amountRequired);
            float progress = (float)collected / r.amountRequired;

            uiTexts[i].text = $"{collected}/{r.amountRequired}";
            uiImages[i].color = Color.Lerp(Color.red, Color.green, progress);
        }
    }
}
