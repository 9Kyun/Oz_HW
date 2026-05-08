using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float _speed = 12f;
    [SerializeField] private float _lifeTime = 3f;
    [SerializeField] private float _hitRadius = 0.2f;

    private Coroutine _lifeCoroutine;

    private void OnEnable()
    {
        _lifeCoroutine = StartCoroutine(LifeRoutine());
    }

    private void OnDisable()
    {
        if (_lifeCoroutine != null)
        {
            StopCoroutine(_lifeCoroutine);
            _lifeCoroutine = null;
        }
    }

    private IEnumerator LifeRoutine()
    {
        yield return new WaitForSeconds(_lifeTime);

        Destroy(gameObject);
    }

    private void Update()
    {
        MoveAndCheckHit();
    }

    private void MoveAndCheckHit()
    {
        Vector3 moveDirection = transform.up;
        float moveDistance = _speed * Time.deltaTime;

        bool hasHit = Physics.SphereCast(
            transform.position,
            _hitRadius,
            moveDirection,
            out RaycastHit hit,
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