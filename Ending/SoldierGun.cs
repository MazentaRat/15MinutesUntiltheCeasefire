using UnityEngine;

public class SoldierGun : MonoBehaviour
{
    public Transform gunPivot;

    private Vector2 shootDir = Vector2.right;
    private PoolManager pool;
    AudioSource audioSource;
    private void Awake()
    {
        pool = FindAnyObjectByType<PoolManager>();
        audioSource = GetComponent<AudioSource>();
    }

    public void SetDirection(Vector2 dir)
    {
        shootDir = dir.normalized;
    }

    // 애니메이션 이벤트에서 호출됨
    public void Fire()
    {
        if (pool == null) return;

        GameObject b = pool.GetBullet();
        b.transform.position = gunPivot.position;

        Bullet bulletScript = b.GetComponent<Bullet>();
        if (bulletScript != null)
            bulletScript.Init(shootDir);

        audioSource.Play();

        float angle = Mathf.Atan2(shootDir.y, shootDir.x) * Mathf.Rad2Deg;
        b.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
