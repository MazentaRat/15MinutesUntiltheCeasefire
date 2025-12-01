using UnityEngine;
using System.Collections;

public class TurtlePettingTrigger : MonoBehaviour
{
    [Header("참조")]
    public GameObject heartObj;            // 하트 오브젝트
    public PetFollowAdvanced follow;       // 거북이 AI (친밀도 증가)
    public float friendshipGain = 0.01f;   // 쓰다듬을 때 증가하는 친밀도
    public float heartTime = 0.7f;         // 하트 표시 시간
    public GameObject blood;

    private bool isHeartShowing = false;
    bool isPetting = false;
    public bool isDead;

    void Start()
    {
        follow = GetComponent<PetFollowAdvanced>();
        if (heartObj != null) heartObj.SetActive(false);
        if(blood != null) blood.SetActive(false);
        isDead = false;
    }

    // 플레이어가 X키를 눌렀을 때 ItemPickup2D에서 호출
    public void Pet()
    {
        if (isPetting || isDead) return;
        isPetting = true;
        // 친밀도 증가
        if (follow != null)
        {
            follow.friendship += friendshipGain;
            follow.friendship = Mathf.Clamp01(follow.friendship);
        }

        // 하트 표시 코루틴 실행
        if (!isHeartShowing)
            StartCoroutine(HeartRoutine());
    }

    IEnumerator HeartRoutine()
    {
        isHeartShowing = true;

        if (heartObj != null)
            heartObj.SetActive(true);

        yield return new WaitForSeconds(heartTime);

        if (heartObj != null)
            heartObj.SetActive(false);

        isHeartShowing = false;
        isPetting = false ;
    }
}
