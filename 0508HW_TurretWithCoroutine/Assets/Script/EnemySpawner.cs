using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyPool _enemyPool;
    [SerializeField] private Transform _turretTarget;

    [Header("Spawn Settings")]
    [SerializeField] private float _spawnRadius = 12f;
    [SerializeField] private float _spawnHeight = 0.5f;
    [SerializeField] private float _spawnInterval = 1.5f;
    [SerializeField] private int _maxActiveEnemies = 10;

    [Header("Enemy Settings")]
    [SerializeField] private float _enemyMoveSpeed = 2f;

    private Coroutine _spawnCoroutine;

    private void OnEnable()
    {
        _spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    private void OnDisable()
    {
        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
        }
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(_spawnInterval);

            if (_enemyPool == null || _turretTarget == null)
            {
                continue;
            }

            if (_enemyPool.ActiveEnemyCount >= _maxActiveEnemies)
            {
                continue;
            }

            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        Vector2 randomCircle = Random.insideUnitCircle.normalized;

        if (randomCircle == Vector2.zero)
        {
            randomCircle = Vector2.right;
        }

        Vector3 center = _turretTarget.position;

        Vector3 spawnPosition = center + new Vector3(
            randomCircle.x * _spawnRadius,
            _spawnHeight,
            randomCircle.y * _spawnRadius
        );

        Vector3 lookDirection = center - spawnPosition;
        lookDirection.y = 0f;

        Quaternion spawnRotation = Quaternion.identity;

        if (lookDirection.sqrMagnitude > 0.001f)
        {
            spawnRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
        }

        _enemyPool.GetEnemy(
            spawnPosition,
            spawnRotation,
            _turretTarget,
            _enemyMoveSpeed
        );
    }
}