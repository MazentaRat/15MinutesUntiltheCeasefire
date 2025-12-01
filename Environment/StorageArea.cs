using UnityEngine;

public class StorageArea : MonoBehaviour
{
    private Color storedColor = new Color(0f, 1f, 0f); // 초록색 (R=0, G=1, B=0)

    private void OnTriggerEnter2D(Collider2D col)
    {
        var pickup = col.GetComponent<ItemPickup2D>();
        if (pickup == null) return;

        // 손에 들린 아이템이면 무시
        if (pickup.isHeld) return;

        // SpriteRenderer 가져오기
        var sr = pickup.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            // 원래 색 없으면 저장
            if (!pickup.hasOriginalColorSaved)
            {
                pickup.originalColor = sr.color;
                pickup.hasOriginalColorSaved = true;
            }

            // 저장된 아이템 색 = 초록색
            sr.color = storedColor;
        }

        // 자동복귀 타이머 일시정지
        var autoReturn = col.GetComponent<ItemAutoReturn>();
        if (autoReturn != null)
        {
            autoReturn.StopAutoReturn();
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        var pickup = col.GetComponent<ItemPickup2D>();
        if (pickup == null) return;

        // 손에 들고 나가는 건 무시 (들고 있으면 색 복원 필요 없고 autoReturn도 아님)
        if (pickup.isHeld)
        {
            RestoreColor(pickup);
            return;
        }

        // 창고 밖 → 원래 색 복원
        RestoreColor(pickup);

        // 자동복귀 타이머를 다시 처음부터 시작
        var autoReturn = col.GetComponent<ItemAutoReturn>();
        if (autoReturn != null)
        {
            autoReturn.StartAutoReturn();
        }
    }

    private void RestoreColor(ItemPickup2D pickup)
    {
        var sr = pickup.GetComponent<SpriteRenderer>();
        if (sr != null && pickup.hasOriginalColorSaved)
        {
            sr.color = pickup.originalColor;
        }
    }
}