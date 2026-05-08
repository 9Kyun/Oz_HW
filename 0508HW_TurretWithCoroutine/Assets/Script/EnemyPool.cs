using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private PoolableEnemy _enemyPrefab;
    [SerializeField] private int _initialPoolSize = 10;
    [SerializeField] private bool _expandIfNeeded = true;

    private readonly Queue<PoolableEnemy> _pool = new Queue<PoolableEnemy>();
    private readonly List<PoolableEnemy> _activeEnemies = new List<PoolableEnemy>();

    public IReadOnlyList<PoolableEnemy> ActiveEnemies => _activeEnemies;
    public int ActiveEnemyCount => _activeEnemies.Count;

    private void Awake()
    {
        for (int i = 0; i < _initialPoolSize; i++)
        {
            CreateEnemy();
        }
    }

    private PoolableEnemy CreateEnemy()
    {
        PoolableEnemy enemy = Instantiate(_enemyPrefab, transform);
        enemy.gameObject.SetActive(false);
        _pool.Enqueue(enemy);
        return enemy;
    }

    public PoolableEnemy GetEnemy(Vector3 position, Quaternion rotation, Transform moveTarget, float moveSpeed)
    {
        if (_pool.Count <= 0)
        {
            if (!_expandIfNeeded)
            {
                return null;
            }

            CreateEnemy();
        }

        PoolableEnemy enemy = _pool.Dequeue();

        enemy.transform.SetPositionAndRotation(position, rotation);
        enemy.Initialize(this, moveTarget, moveSpeed);

        if (!_activeEnemies.Contains(enemy))
        {
            _activeEnemies.Add(enemy);
        }

        return enemy;
    }

    public void ReturnEnemy(PoolableEnemy enemy)
    {
        if (enemy == null) return;
        if (!enemy.gameObject.activeSelf) return;

        enemy.gameObject.SetActive(false);

        _activeEnemies.Remove(enemy);
        _pool.Enqueue(enemy);
    }
}