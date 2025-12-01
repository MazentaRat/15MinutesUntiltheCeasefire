using UnityEngine;
using System.Collections;

public class PetFollowAdvanced : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Movement Settings")]
    public float minRadius = 2f;   // 호감도 최대일 때 거리
    public float maxRadius = 3f;   // 호감도 최소일 때 거리
    public float baseSpeed = 0.1f;     // 기본 이동 속도
    public float followSpeed = 0.3f; // 플레이어 이동 시 속도
    public float stopChance = 0.2f;  // 멈출 확률
    public float stopTime = 1.2f;    // 멈추는 시간
    public float rotationSpeed = 180f; // 회전 속도(도/초)

    [Header("Behavior Settings")]
    [Range(0f, 1f)]
    public float friendship = 0.5f; // 호감도
    public float circleChance = 0.3f; // 호감도 높을수록 빙글빙글 돌 확률 (내부 보정 있음)

    private Vector2 targetPos;
    private float currentStopTimer;
    private bool isStopped;
    private bool isCircling;
    private float circleAngle;
    private float circleSpeed;

    private Vector2 lastPlayerPos;
    private bool playerMoving;
    PlayerInputSystem inputSystem;

    Animator animator;
    int HashWalk = Animator.StringToHash("IsWalking");
    int HashDead = Animator.StringToHash("IsDead");
    private Vector2 lastPos;
    TurtlePettingTrigger turtlePettingTrigger;
    public GameObject blood;

    private void Awake()
    {
        inputSystem = FindAnyObjectByType<PlayerInputSystem>();
        if (inputSystem != null)
            player = inputSystem.gameObject.transform;

        turtlePettingTrigger = GetComponent<TurtlePettingTrigger>();
        blood.SetActive(false);
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        if (player != null)
            lastPlayerPos = player.position;
        lastPos = transform.position;
        ChooseNewTarget();
    }

    void Update()
    {
        if (player == null) return;

        if(turtlePettingTrigger.isDead)
        {
            animator.SetBool(HashDead, true);
            blood.SetActive(true);
            StartCoroutine(dead());
        }

        // 플레이어 이동 여부 체크
        playerMoving = (Vector2.Distance(lastPlayerPos, player.position) > 0.01f);
        lastPlayerPos = player.position;

        float radius = Mathf.Lerp(maxRadius, minRadius, friendship);

        // --- 빙글빙글 도는 행동 ---
        if (isCircling)
        {
            circleAngle += circleSpeed * Time.deltaTime;
            targetPos = (Vector2)player.position + new Vector2(Mathf.Cos(circleAngle), Mathf.Sin(circleAngle)) * radius;

            MoveTowardTarget(followSpeed * 0.8f);
            if (Random.value < 0.01f)
            {
                // 1% 확률로 원래 패턴 복귀
                isCircling = false;
                ChooseNewTarget(radius);
            }
            return;
        }

        // --- 멈춤 처리 ---
        if (isStopped)
        {
            currentStopTimer -= Time.deltaTime;
            if (currentStopTimer <= 0f)
            {
                isStopped = false;

                // 호감도가 높을수록 가끔 플레이어 주변 빙글빙글
                float chance = circleChance * friendship;
                if (Random.value < chance)
                {
                    StartCircling(radius);
                    return;
                }

                ChooseNewTarget(radius);
            }
            return;
        }

        // --- 이동 ---
        float currentSpeed = playerMoving ? followSpeed : baseSpeed;
        MoveTowardTarget(currentSpeed);

        // 목표 도달 시 행동 전환
        if (Vector2.Distance(transform.position, targetPos) < 0.1f)
        {
            if (Random.value < stopChance)
            {
                isStopped = true;
                currentStopTimer = stopTime * Random.Range(0.5f, 1.5f);
            }
            else
            {
                ChooseNewTarget(radius);
            }
        }
    }

    IEnumerator dead()
    {
        yield return new WaitForSeconds(1f);
        enabled = false;
    }
    void MoveTowardTarget(float speed)
    {
        Vector2 beforePos = transform.position;   // 이동 전 위치

        Vector2 dirVec = (targetPos - (Vector2)transform.position);
        Vector2 dir = dirVec.normalized;

        // 이동
        transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        // --- 애니메이션 업데이트 (움직이면 걷기) ---
        float movedDist = Vector2.Distance(beforePos, transform.position);
        animator.SetBool(HashWalk, movedDist > 0.001f);

        // --- 회전: 스프라이트의 "상단(Up, +Y)"이 이동 방향을 보도록 한다 ---
        // dir이 거의 0이면 회전하지 않음(정지 상태 유지)
        if (dir.sqrMagnitude > 0.0001f)
        {
            // Atan2는 +X 기준 각도(도 단위)를 리턴. 스프라이트의 상단이 정면이면 -90도 보정 필요.
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            Quaternion targetRot = Quaternion.Euler(0f, 0f, angle);

            // 부드럽게 회전: rotationSpeed는 '도/초' 단위로 해석
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }

    void ChooseNewTarget(float radius = 1f)
    {
        if (player == null) return;

        Vector2 randomCircle = Random.insideUnitCircle.normalized * radius;
        targetPos = (Vector2)player.position + randomCircle;
    }

    void StartCircling(float radius)
    {
        isCircling = true;
        circleAngle = Random.Range(0f, Mathf.PI * 2f);
        circleSpeed = Random.Range(2f, 4f); // 자연스러운 회전 속도
    }
}
