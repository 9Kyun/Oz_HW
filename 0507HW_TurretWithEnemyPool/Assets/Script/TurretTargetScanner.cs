using UnityEngine;

public class TurretTargetScanner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyPool _enemyPool;
    [SerializeField] private Transform _origin;

    [Header("Scan Settings")]
    [SerializeField] private float _scanRadius = 30f;

    public Transform CurrentTarget { get; private set; }

    private void Update()
    {
        FindNearestEnemy();
    }

    private void FindNearestEnemy()
    {
        CurrentTarget = null;

        if (_enemyPool == null || _origin == null) return;

        float scanRadiusSqr = _scanRadius * _scanRadius;
        float closestDistanceSqr = float.MaxValue;

        foreach (PoolableEnemy enemy in _enemyPool.ActiveEnemies)
        {
            if (enemy == null) continue;
            if (!enemy.gameObject.activeInHierarchy) continue;

            Transform aimPoint = enemy.AimPoint;

            float distanceSqr = (aimPoint.position - _origin.position).sqrMagnitude;

            if (distanceSqr > scanRadiusSqr) continue;

            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                CurrentTarget = aimPoint;
            }
        }
    }
}