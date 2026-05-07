using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float _speed = 12f;
    [SerializeField] private float _lifeTime = 3f;
    [SerializeField] private float _hitRadius = 0.2f;

    private float _lifeTimer;

    private void OnEnable()
    {
        _lifeTimer = _lifeTime;
    }

    private void Update()
    {
        MoveAndCheckHit();

        _lifeTimer -= Time.deltaTime;

        if (_lifeTimer <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void MoveAndCheckHit()
    {
        Vector3 moveDirection = transform.up;
        float moveDistance = _speed * Time.deltaTime;

        RaycastHit hit;

        bool hasHit = Physics.SphereCast(
            transform.position,
            _hitRadius,
            moveDirection,
            out hit,
            moveDistance,
            ~0,
            QueryTriggerInteraction.Collide
        );

        if (hasHit)
        {
            PoolableEnemy enemy = hit.collider.GetComponentInParent<PoolableEnemy>();

            if (enemy != null)
            {
                enemy.TakeHit();
                Destroy(gameObject);
                return;
            }
        }

        transform.position += moveDirection * moveDistance;
    }

    private void OnTriggerEnter(Collider other)
    {
        PoolableEnemy enemy = other.GetComponentInParent<PoolableEnemy>();

        if (enemy == null) return;

        enemy.TakeHit();
        Destroy(gameObject);
    }
}