using UnityEngine;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class PlayerItemActionUIController : MonoBehaviour
{
    [Header("UI 오브젝트")]
    public GameObject actionUI;
    public TextMeshProUGUI actionText;

    private ItemPickup2D currentItem;

    [Header("로컬라이징 문구")]
    public LocalizedString Eat;
    public LocalizedString Read;
    public LocalizedString Wear;
    public LocalizedString Search;   // Body, GarbageBag 공용
    public LocalizedString Pet;

    private void Start()
    {
        if (actionUI != null)
            actionUI.SetActive(false);
    }

    public void ShowPetText()
    {
        if (actionUI == null || actionText == null) return;

        actionUI.SetActive(true);

        // Pet LocalizedString 적용
        ApplyLocalized(Pet);
    }

    private async void ApplyLocalized(LocalizedString localizedStr)
    {
        var result = await localizedStr.GetLocalizedStringAsync().Task;
        actionText.text = result;
    }


    /// <summary>
    /// 플레이어가 아이템을 집었을 때 호출
    /// </summary>
    public void OnPickUpItem(ItemPickup2D item)
    {
        currentItem = item;

        if (item == null || item.itemData == null)
        {
            HideUI();
            return;
        }

        LocalizedString localized = GetLocalizedString(item.itemData.effectType);

        if (localized == null)
        {
            HideUI();
            return;
        }

        ShowUI(localized);
    }

    /// <summary>
    /// 플레이어가 아이템을 내려놓을 때 호출
    /// </summary>
    public void OnDropItem()
    {
        currentItem = null;
        HideUI();
    }

    /// <summary>
    /// Inspector에 넣은 LocalizedString을 반환
    /// </summary>
    private LocalizedString GetLocalizedString(ItemEffectType type)
    {
        switch (type)
        {
            case ItemEffectType.Eat: return Eat;
            case ItemEffectType.Read: return Read;
            case ItemEffectType.Wear: return Wear;
            case ItemEffectType.Body: return Search;
            case ItemEffectType.GarbageBag: return Search;
        }

        // UI 필요 없는 타입
        return null;
    }

    private async void ShowUI(LocalizedString localizedStr)
    {
        if (actionUI == null || actionText == null)
            return;

        actionUI.SetActive(true);

        // Localization 적용
        var result = await localizedStr.GetLocalizedStringAsync().Task;

        actionText.text = result;
    }

    private void HideUI()
    {
        if (actionUI != null)
            actionUI.SetActive(false);
    }
}
