using UnityEngine;

public class PoolableEnemy : MonoBehaviour
{
    [Header("Move Settings")]
    [SerializeField] private float _defaultMoveSpeed = 2f;
    [SerializeField] private float _stopDistance = 0.8f;

    [Header("Aim")]
    [SerializeField] private Transform _aimPoint;

    private EnemyPool _pool;
    private Transform _moveTarget;
    private float _moveSpeed;

    public Transform AimPoint => _aimPoint != null ? _aimPoint : transform;

    public void Initialize(EnemyPool pool, Transform moveTarget, float moveSpeed)
    {
        _pool = pool;
        _moveTarget = moveTarget;
        _moveSpeed = moveSpeed > 0f ? moveSpeed : _defaultMoveSpeed;

        gameObject.SetActive(true);
    }

    private void Update()
    {
        MoveToTarget();
    }

    private void MoveToTarget()
    {
        if (_moveTarget == null) return;

        Vector3 direction = _moveTarget.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= _stopDistance * _stopDistance)
        {
            ReturnToPool();
            return;
        }

        Vector3 moveDirection = direction.normalized;

        transform.position += moveDirection * _moveSpeed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(moveDirection, Vector3.up);
    }

    public void TakeHit()
    {
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (_pool != null)
        {
            _pool.ReturnEnemy(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}