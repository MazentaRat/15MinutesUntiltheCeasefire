using UnityEngine;

[System.Serializable]
public class UpgradeTarget
{
    public GameObject level1;
    public GameObject level2;
    public GameObject level3;

    public void SetActiveLevel(int level)
    {
        if (level1 != null) level1.SetActive(level == 1);
        if (level2 != null) level2.SetActive(level == 2);
        if (level3 != null) level3.SetActive(level == 3);
    }
}
