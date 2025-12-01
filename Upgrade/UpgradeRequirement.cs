[System.Serializable]
public class UpgradeRequirement
{
    public ItemEffectType itemType;  // 타입 (담요, 모닥불 등)
    public int itemID;               // ScriptableObject 고유 ID
    public int amountRequired;       // 필요 수량
}