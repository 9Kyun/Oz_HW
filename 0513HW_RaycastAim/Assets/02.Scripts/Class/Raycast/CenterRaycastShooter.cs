using UnityEngine;
using UnityEngine.InputSystem;

public class CenterRaycastShooter : MonoBehaviour
{
    [Header("Camera / Aim")]
    [SerializeField] private Camera m_cam;
    [SerializeField] private Transform m_muzzlePoint;
    [SerializeField] private Transform m_turnRoot;

    [Header("Raycast")]
    [SerializeField] private LayerMask m_targetMask;
    [SerializeField] private LayerMask m_obstacleMask;
    [SerializeField] private float m_maxDistance = 100.0f;

    [Header("Aim")]
    [SerializeField] private bool m_fireOnlyWhileAiming = true;
    [SerializeField] private float m_turnSpeed = 15.0f;

    [Header("Animation")]
    [SerializeField] private Animator m_animator;
    [SerializeField] private string m_aimBoolName = "IsAiming";
    [SerializeField] private string m_fireTriggerName = "Fire";

    [Header("Debug")]
    [SerializeField] private bool m_drawDebugRay = true;
    [SerializeField] private bool m_logAimInput = false;

    private PlayerInput _playerInput;
    private InputAction _aimAction;

    private bool _isAiming;

    private int _aimBoolHash;
    private int _fireTriggerHash;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();

        if (_playerInput != null)
        {
            _aimAction = _playerInput.actions.FindAction("Aim", false);
        }

        if (m_cam == null)
        {
            m_cam = Camera.main;
        }

        if (m_muzzlePoint == null)
        {
            m_muzzlePoint = transform;
        }

        if (m_turnRoot == null)
        {
            m_turnRoot = transform;
        }

        if (m_animator == null)
        {
            m_animator = GetComponent<Animator>();

            if (m_animator == null)
            {
                m_animator = GetComponentInChildren<Animator>();
            }
        }

        _aimBoolHash = Animator.StringToHash(m_aimBoolName);
        _fireTriggerHash = Animator.StringToHash(m_fireTriggerName);

        SetAiming(false);
    }

    private void Update()
    {
        UpdateAimStateFromInputAction();

        if (_isAiming)
        {
            RotatePlayerTowardAimPoint();
        }
    }

    private void UpdateAimStateFromInputAction()
    {
        if (_aimAction == null)
        {
            return;
        }

        bool aimHeld = _aimAction.IsPressed();

        if (_isAiming != aimHeld)
        {
            SetAiming(aimHeld);
        }
    }

    // Player Input의 Send Messages 방식에서 자동 호출됨
    public void OnAim(InputValue value)
    {
        bool isPressed = value.isPressed;

        if (m_logAimInput)
        {
            Debug.Log($"[Aim Input] {isPressed}");
        }

        SetAiming(isPressed);
    }

    // Player Input의 Send Messages 방식에서 자동 호출됨
    public void OnFire(InputValue value)
    {
        if (!value.isPressed)
        {
            return;
        }

        if (m_fireOnlyWhileAiming && !_isAiming)
        {
            return;
        }

        if (m_animator != null)
        {
            m_animator.SetTrigger(_fireTriggerHash);
        }

        TryFire();
    }

    private void SetAiming(bool isAiming)
    {
        if (_isAiming == isAiming)
        {
            return;
        }

        _isAiming = isAiming;

        if (m_animator != null)
        {
            m_animator.SetBool(_aimBoolHash, _isAiming);
        }

        if (m_logAimInput)
        {
            Debug.Log($"[Set Aiming] {_isAiming}");
        }
    }

    private void TryFire()
    {
        if (!TryGetCameraAimPoint(out Vector3 aimPoint))
        {
            return;
        }

        Vector3 origin = m_muzzlePoint.position;
        Vector3 direction = aimPoint - origin;

        if (direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        float distance = Mathf.Min(direction.magnitude, m_maxDistance);
        direction.Normalize();

        int shotMask = m_targetMask.value | m_obstacleMask.value;

        if (Physics.Raycast(origin, direction, out RaycastHit shotHit, distance, shotMask, QueryTriggerInteraction.Ignore))
        {
            if (IsInLayerMask(shotHit.collider.gameObject.layer, m_targetMask))
            {
                Debug.Log($"[Fire Success] Hit Target : {shotHit.collider.name}");

                if (m_drawDebugRay)
                {
                    Debug.DrawLine(origin, shotHit.point, Color.green, 1.0f);
                }

                // 데미지 처리 예시
                // shotHit.collider.GetComponent<EnemyHealth>()?.TakeDamage(1);
            }
            else
            {
                Debug.Log($"[Fire Failed] Blocked by : {shotHit.collider.name}");

                if (m_drawDebugRay)
                {
                    Debug.DrawLine(origin, shotHit.point, Color.red, 1.0f);
                }
            }
        }
        else
        {
            Debug.Log("[Fire Miss] Nothing hit");

            if (m_drawDebugRay)
            {
                Debug.DrawLine(origin, origin + direction * distance, Color.yellow, 1.0f);
            }
        }
    }

    private bool TryGetCameraAimPoint(out Vector3 aimPoint)
    {
        aimPoint = default;

        if (m_cam == null)
        {
            m_cam = Camera.main;

            if (m_cam == null)
            {
                return false;
            }
        }

        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Ray cameraRay = m_cam.ScreenPointToRay(screenCenter);

        int aimMask = m_targetMask.value | m_obstacleMask.value;

        if (Physics.Raycast(cameraRay, out RaycastHit hit, m_maxDistance, aimMask, QueryTriggerInteraction.Ignore))
        {
            aimPoint = hit.point;

            if (m_drawDebugRay)
            {
                Debug.DrawLine(cameraRay.origin, hit.point, Color.cyan, 1.0f);
            }

            return true;
        }

        aimPoint = cameraRay.origin + cameraRay.direction * m_maxDistance;

        if (m_drawDebugRay)
        {
            Debug.DrawLine(cameraRay.origin, aimPoint, Color.blue, 1.0f);
        }

        return true;
    }

    private void RotatePlayerTowardAimPoint()
    {
        if (!TryGetCameraAimPoint(out Vector3 aimPoint))
        {
            return;
        }

        Vector3 direction = aimPoint - m_turnRoot.position;
        direction.y = 0.0f;

        if (direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);

        m_turnRoot.rotation = Quaternion.Slerp(
            m_turnRoot.rotation,
            targetRotation,
            Time.deltaTime * m_turnSpeed
        );
    }

    private bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }
}