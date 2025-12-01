using UnityEngine;
using System.Collections;

public class SoldierAction : MonoBehaviour
{
    Animator animator;
    Transform tr;
    public PlayerStatus player;
    SpriteRenderer sr;

    public GameObject gunEffect;
    float playerSoldierDis = 1.5f;
    public float moveSpeed = 1.5f;

    public GameObject patrolP;
    Transform[] walkingPoints;
    public Transform gunPivot;

    public SoldierGun gun;
    TurtlePettingTrigger turtle;

    private int currentPointIndex = -1;
    int hashAttak = Animator.StringToHash("IsAttacking");

    private enum State { Patrol, Chase, Shoot }
    private State currentState = State.Patrol;

    private enum TargetType { None, Player, Turtle }
    private TargetType currentTargetType = TargetType.None;
    private Transform currentTarget;

    private Vector2 lastShootDir;
    AudioSource souce;

    private void Start()
    {
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        tr = transform;

        patrolP = FindAnyObjectByType<PatrolPoint>().gameObject;
        player = FindAnyObjectByType<PlayerStatus>();
        turtle = FindAnyObjectByType<TurtlePettingTrigger>();

        gunEffect.SetActive(false);

        int childCount = patrolP.transform.childCount;
        walkingPoints = new Transform[childCount];

        for (int i = 0; i < childCount; i++)
            walkingPoints[i] = patrolP.transform.GetChild(i);

        souce = GetComponent<AudioSource>();
        SetNextPatrolPoint();
    }

    private void OnEnable()
    {
        turtle = FindAnyObjectByType<TurtlePettingTrigger>();
        souce = GetComponent<AudioSource>();
    }

    private void Update()
    {
        currentTarget = GetPrimaryTarget();

        float dist = currentTarget != null
            ? Vector2.Distance(tr.position, CurrentTargetPosition())
            : Mathf.Infinity;

        switch (currentState)
        {
            case State.Patrol:
                PatrolUpdate(dist);
                break;

            case State.Chase:
                if (currentTarget == null)
                {
                    ChangeState(State.Patrol);
                    break;
                }
                ChaseUpdate(dist);
                break;

            case State.Shoot:
                if (currentTarget == null)
                {
                    ChangeState(State.Patrol);
                    break;
                }
                ShootUpdate(dist);
                break;
        }
    }


    // ---------------------------------------------------------
    //                Target Selection
    // ---------------------------------------------------------
    private Transform GetPrimaryTarget()
    {
        Transform playerTarget = IsPlayerValidTarget()
            ? player.transform
            : null;

        Transform turtleTarget = (turtle != null && turtle.isDead == false)
            ? turtle.transform
            : null;

        if (playerTarget == null && turtleTarget == null)
        {
            currentTargetType = TargetType.None;
            return null;
        }

        bool prioritizeTurtle = true;

        if (prioritizeTurtle && turtleTarget != null)
        {
            currentTargetType = TargetType.Turtle;
            return turtleTarget;
        }

        float distPlayer = playerTarget != null ? Vector2.Distance(tr.position, playerTarget.position) : Mathf.Infinity;
        float distTurtle = turtleTarget != null ? Vector2.Distance(tr.position, turtleTarget.position) : Mathf.Infinity;

        if (distTurtle < distPlayer)
        {
            currentTargetType = TargetType.Turtle;
            return turtleTarget;
        }
        else
        {
            currentTargetType = TargetType.Player;
            return playerTarget;
        }
    }

    private Vector3 CurrentTargetPosition()
    {
        if (currentTarget == null)
            return tr.position;

        Vector3 pos;

        if (currentTarget.childCount > 0)
            pos = currentTarget.GetChild(0).position;
        else
            pos = currentTarget.position;

        if (currentTargetType == TargetType.Turtle)
            pos.y -= 0.3f;

        return pos;
    }


    // ---------------------------------------------------------
    //                      STATES
    // ---------------------------------------------------------
    private void PatrolUpdate(float dist)
    {
        if (walkingPoints.Length == 0) return;
        if (currentPointIndex < 0 || currentPointIndex >= walkingPoints.Length)
            SetNextPatrolPoint();

        MoveTowards(walkingPoints[currentPointIndex].position);

        if (Vector2.Distance(tr.position, walkingPoints[currentPointIndex].position) < 0.1f)
            SetNextPatrolPoint();

        if (dist <= playerSoldierDis)
            ChangeState(State.Shoot);
        else if (dist <= playerSoldierDis * 2f)
            ChangeState(State.Chase);
    }

    private void ChaseUpdate(float dist)
    {
        MoveTowards(CurrentTargetPosition());

        if (dist <= playerSoldierDis)
            ChangeState(State.Shoot);
        else if (dist > playerSoldierDis * 2.5f)
            ChangeState(State.Patrol);
    }

    private void ShootUpdate(float dist)
    {
        // PlayWalkSound(false);

        FlipDirection(CurrentTargetPosition());
        ShootGun();

        if (dist > playerSoldierDis)
            ChangeState(State.Chase);
    }


    // ---------------------------------------------------------
    //                      UTIL
    // ---------------------------------------------------------
    private void ChangeState(State newState)
    {
        if (currentState == newState) return;

        currentState = newState;

        // Shoot 들어갈 때 단 한 번만 걷기 소리 OFF
        if (newState == State.Shoot)
        {
            PlayWalkSound(false);
            animator.SetBool(hashAttak, true);
            gunEffect.SetActive(true);
        }
        else
        {
            animator.SetBool(hashAttak, false);
            gunEffect.SetActive(false);
        }

        if (newState == State.Patrol)
            SetNextPatrolPoint();
    }

    private void SetNextPatrolPoint()
    {
        if (walkingPoints.Length == 0) return;
        currentPointIndex = Random.Range(0, walkingPoints.Length);
    }

    private void MoveTowards(Vector3 target)
    {
        FlipDirection(target);

        Vector3 before = tr.position;
        Vector3 dir = (target - tr.position).normalized;

        tr.position += dir * moveSpeed * Time.deltaTime;

        // ---- 이동 판정 완화 (정말 조금만 움직여도 이동으로 인정) ----
        float moved = (tr.position - before).magnitude;
        bool isMoving = moved > 0.00001f;

        // 이동 상태일 때만 WalkSound 재생
        PlayWalkSound(isMoving);
    }

    private void FlipDirection(Vector3 target)
    {
        tr.localScale = new Vector3(
            (target.x > tr.position.x ? -1f : 1f),
            tr.localScale.y,
            tr.localScale.z
        );
    }

    private void ShootGun()
    {
        Vector2 origin = gunPivot.position;
        Vector2 target = CurrentTargetPosition();
        Vector2 dir = (target - origin).normalized;

        lastShootDir = dir;

        Debug.DrawRay(origin, dir * 5f, Color.red);

        gun.SetDirection(lastShootDir);
    }

    private bool IsPlayerValidTarget()
    {
        if (player == null) return false;
        if (player.isDead) return false;
        if (player.killedBySoldier) return false;
        return true;
    }

    // ---------------------------------------------------------
    //                WALK SOUND CONTROLLER
    // ---------------------------------------------------------
    private void PlayWalkSound(bool isMoving)
    {
        if (souce == null) return;

        if (isMoving)
        {
            if (!souce.isPlaying)
            {
                souce.loop = true;
                souce.Play();
            }
        }
        else
        {
            if (souce.isPlaying)
                souce.Stop();
        }
    }
}
