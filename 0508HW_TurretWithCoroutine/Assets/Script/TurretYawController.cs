using UnityEngine;
using System.Collections;

public class TurretYawController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _yawPivot;
    [SerializeField] private Transform _pitchPivot;
    [SerializeField] private Transform _muzzlePoint;
    [SerializeField] private Transform _target;
    [SerializeField] private TurretTargetScanner _targetScanner;

    [Header("Yaw Settings")]
    [SerializeField] private float _yawRotateSpeed = 90f;
    [SerializeField] private float _yawAngleOffset = 0f;

    [Header("Pitch Settings")]
    [SerializeField] private float _pitchRotateSpeed = 90f;
    [SerializeField] private float _minPitch = -45f;
    [SerializeField] private float _maxPitch = 20f;
    [SerializeField] private float _pitchAngleOffset = 0f;
    [SerializeField] private bool _invertPitch = false;

    [Header("Aim Settings")]
    [SerializeField] private float _fireAngleThreshold = 5f;

    [Header("Fire Settings")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private float _fireInterval = 0.5f;

    public bool IsAimed { get; private set; }

    private Coroutine _fireCoroutine;

    private void OnEnable()
    {
        _fireCoroutine = StartCoroutine(FireRoutine());
    }

    private void OnDisable()
    {
        if (_fireCoroutine != null)
        {
            StopCoroutine(_fireCoroutine);
            _fireCoroutine = null;
        }
    }

    private void Update()
    {
        Transform currentTarget = GetCurrentTarget();

        if (_yawPivot == null ||
            _pitchPivot == null ||
            _muzzlePoint == null ||
            currentTarget == null)
        {
            IsAimed = false;
            return;
        }

        RotateYawToTarget(currentTarget);
        RotatePitchToTarget(currentTarget);
        CheckAim(currentTarget);
    }

    private Transform GetCurrentTarget()
    {
        if (_targetScanner != null && _targetScanner.CurrentTarget != null)
        {
            return _targetScanner.CurrentTarget;
        }

        return _target;
    }

    private void RotateYawToTarget(Transform target)
    {
        Vector3 directionToTarget = target.position - _yawPivot.position;

        directionToTarget.y = 0f;

        if (directionToTarget.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget, Vector3.up);

        targetRotation *= Quaternion.Euler(0f, _yawAngleOffset, 0f);

        _yawPivot.rotation = Quaternion.RotateTowards(
            _yawPivot.rotation,
            targetRotation,
            _yawRotateSpeed * Time.deltaTime
        );
    }

    private void RotatePitchToTarget(Transform target)
    {
        Vector3 directionToTarget = target.position - _pitchPivot.position;

        float height = directionToTarget.y;

        Vector3 flatDirection = directionToTarget;
        flatDirection.y = 0f;

        float horizontalDistance = flatDirection.magnitude;

        if (horizontalDistance < 0.001f) return;

        float pitchAngle = -Mathf.Atan2(height, horizontalDistance) * Mathf.Rad2Deg;

        if (_invertPitch)
        {
            pitchAngle = -pitchAngle;
        }

        pitchAngle += _pitchAngleOffset;
        pitchAngle = Mathf.Clamp(pitchAngle, _minPitch, _maxPitch);

        Quaternion targetLocalRotation = Quaternion.Euler(pitchAngle, 0f, 0f);

        _pitchPivot.localRotation = Quaternion.RotateTowards(
            _pitchPivot.localRotation,
            targetLocalRotation,
            _pitchRotateSpeed * Time.deltaTime
        );
    }

    private void CheckAim(Transform target)
    {
        Vector3 directionToTarget = target.position - _muzzlePoint.position;

        if (directionToTarget.sqrMagnitude < 0.001f)
        {
            IsAimed = false;
            return;
        }

        float angle = Vector3.Angle(_muzzlePoint.forward, directionToTarget.normalized);

        IsAimed = angle <= _fireAngleThreshold;
    }

    private void FireProjectile()
    {
        Instantiate(
            _projectilePrefab,
            _muzzlePoint.position,
            _muzzlePoint.rotation * Quaternion.Euler(90f, 0f, 0f)
        );
    }

    private IEnumerator FireRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(_fireInterval);

            if (IsAimed && _projectilePrefab != null)
            {
                FireProjectile();
            }
        }
    }
}