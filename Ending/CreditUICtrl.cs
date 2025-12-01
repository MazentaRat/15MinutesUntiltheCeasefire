using UnityEngine;
using System.Collections;

public class CreditUICtrl : MonoBehaviour
{
    public GameObject[] Num1;
    public GameObject[] Num2;
    public GameObject[] Num3;

    private void OnEnable()
    {
        StartCoroutine(ShowCredits());
    }

    private IEnumerator ShowCredits()
    {
        // 1 → 2 → 3 순서대로 재생
        yield return StartCoroutine(ActivateList(Num1));
        yield return StartCoroutine(ActivateList(Num2));
        yield return StartCoroutine(ActivateList(Num3));
    }

    private IEnumerator ActivateList(GameObject[] objs)
    {
        if (objs == null || objs.Length == 0)
            yield break;

        for (int i = 0; i < objs.Length; i++)
        {
            objs[i].SetActive(true);
            yield return new WaitForSeconds(1f);  // 1초 간격
        }
    }
}
