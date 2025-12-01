using UnityEngine;
using System.Collections;
public class Bullet : MonoBehaviour
{
    public float speed = 12f;
    public float lifeTime = 1.5f;

    private Vector2 dir;
    private PoolManager pool;
    private float timer;

    private void Awake()
    {
        pool = FindAnyObjectByType<PoolManager>();
    }

    public void Init(Vector2 direction)
    {
        dir = direction.normalized;
        timer = 0f;
    }

    private void Update()
    {
        transform.position += (Vector3)dir * speed * Time.deltaTime;

        timer += Time.deltaTime;
        if (timer >= lifeTime)
            ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (pool != null)
            pool.ReturnBullet(gameObject);
        else
            gameObject.SetActive(false);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerStatus ps = collision.gameObject.GetComponent<PlayerStatus>();
        if (ps != null)
        {
            if (ps.killedBySoldier) return;

            ps.killedBySoldier = true;
            ps.playerAnimator.SetBool(ps.hashDead,true);
            ps.blood.SetActive(true);

            StartCoroutine(Ending(ps));
        }

        TurtlePettingTrigger turtle = collision.GetComponent<TurtlePettingTrigger>();
        if (turtle != null)
        {
            turtle.isDead = true;
        }
    }
    IEnumerator Ending(PlayerStatus ps)
    {
        yield return new WaitForSeconds(3f);
        ps.Killed();
    }
}