using UnityEngine;

public class Blanket : MonoBehaviour
{
    public enum BlanketLevel { Level1, Level2 }
    public BlanketLevel blanketLevel = BlanketLevel.Level1;
    DayNightManager dayNightManager;

    private void Start()
    {
        dayNightManager = FindAnyObjectByType<DayNightManager>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerStatus ps = other.GetComponent<PlayerStatus>();
        if (ps != null)
        {
            ps.isInBlanket = true;
            ps.currentBlanketLevel = (int)blanketLevel;

            // 컬러 변경 : (128, 128, 128)
            if (ps.playerSR != null)
                ps.playerSR.color = new Color(128f / 255f, 128f / 255f, 128f / 255f);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerStatus ps = other.GetComponent<PlayerStatus>();
        if (ps != null)
        {
            ps.isInBlanket = false;
            ps.currentBlanketLevel = 0;

            // 담요에서 나오면 원래 색으로 복구
            if (ps.playerSR != null)
                ps.playerSR.color = ps.originalPlayerColor;
        }
    }
}
