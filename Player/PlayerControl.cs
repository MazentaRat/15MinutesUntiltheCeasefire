using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    private PlayerInputSystem input;
    private Animator animator;
    private Transform tr;
    PlayerStatus status;

    [Header("Movement Settings")]
    public float moveSpeed = 2.0f;
    public bool isPaused = false;

    [Header("Map Boundary Settings")]
    public float minX = -3.7f;
    public float maxX = 14.2f;
    public float minY = -1.84f;
    public float maxY = 0.5f;

    // 마지막 바라본 방향(Idle 유지용)
    private Vector2 lastDir = Vector2.down;

    private int basePlayerOrder;
    public SpriteRenderer playerSprite;

    string aniMoveX = "MoveX";
    string aniMoveY = "MoveY";
    string aniSpeed = "Speed";

    public AudioSource walkSource;   // 걷기 사운드 전용
    public AudioSource sfxSource;    // 효과음 전용
    public AudioSource effectSource;    // 이펙트 전용
    public AudioClip walkSound;

    void Start()
    {
        input = GetComponent<PlayerInputSystem>();
        animator = GetComponent<Animator>();
        tr = GetComponent<Transform>();
        playerSprite = GetComponent<SpriteRenderer>();
        status = GetComponent<PlayerStatus>();
        basePlayerOrder = playerSprite.sortingOrder;
    }

    void Update()
    {
        if (isPaused) return;
        if(status.isDead || status.killedBySoldier) return;

        Movement();
        LocationLimits();
        UpdateHeldItemSorting();
    }

    private void Movement()
    {
        Vector2 dir = input.moveDir;
        float speed = dir.magnitude;
        Vector2 moveDir = dir.normalized;

        // 이동 처리
        Vector3 move = new Vector3(dir.x, dir.y, 0) * moveSpeed * Time.deltaTime;
        tr.Translate(move);

        bool isMoving = dir.sqrMagnitude > 0.01f;

        // --- 걷기 사운드 처리 ---
        if (isMoving)
        {
            if (!walkSource.isPlaying)
            {
                walkSource.clip = walkSound;
                walkSource.loop = true;        // 계속 걷는 동안 반복 재생
                walkSource.Play();
            }
        }
        else
        {
            if (walkSource.isPlaying)
            {
                walkSource.Stop();
            }
        }

        // 방향 갱신
        if (isMoving)
            lastDir = moveDir;

        // 애니메이터 파라미터 적용
        if (animator != null)
        {
            animator.SetFloat(aniMoveX, lastDir.x);
            animator.SetFloat(aniMoveY, lastDir.y);
            animator.SetFloat(aniSpeed, speed);
        }
    }

    private void LocationLimits()
    {
        Vector2 pos = tr.position;

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        tr.position = pos;
    }

    private void UpdateHeldItemSorting()
    {
        if (status.currentItem == null) return;
        if (status.currentItem.itemSprite == null) return;

        int playerOriginal = basePlayerOrder;
        int itemOriginal = status.currentItem.originalOrder;

        if (lastDir.y >= 0.8f)
        {
            playerSprite.sortingOrder = itemOriginal;
            status.currentItem.itemSprite.sortingOrder = playerOriginal;
        }
        else
        {
            playerSprite.sortingOrder = playerOriginal;
            status.currentItem.itemSprite.sortingOrder = itemOriginal;
        }
    }
}
