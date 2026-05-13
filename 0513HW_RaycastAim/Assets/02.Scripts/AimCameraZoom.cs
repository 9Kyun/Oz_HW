using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class AimCameraZoom : MonoBehaviour
{
    [SerializeField] private PlayerInput m_playerInput;
    [SerializeField] private CinemachineCamera m_cinemachineCamera;

    [SerializeField] private float m_normalFov = 60.0f;
    [SerializeField] private float m_aimFov = 45.0f;
    [SerializeField] private float m_zoomSpeed = 10.0f;

    private InputAction _aim;
    private bool _isAiming;

    private void Awake()
    {
        if (m_playerInput == null)
        {
            m_playerInput = FindFirstObjectByType<PlayerInput>();
        }

        if (m_playerInput != null)
        {
            _aim = m_playerInput.actions.FindAction("Aim", false);
        }

        if (m_cinemachineCamera != null)
        {
            m_normalFov = m_cinemachineCamera.Lens.FieldOfView;
        }
    }

    private void OnEnable()
    {
        if (_aim != null)
        {
            _aim.performed += OnAimStarted;
            _aim.canceled += OnAimCanceled;
        }
    }

    private void OnDisable()
    {
        if (_aim != null)
        {
            _aim.performed -= OnAimStarted;
            _aim.canceled -= OnAimCanceled;
        }
    }

    private void Update()
    {
        if (m_cinemachineCamera == null)
        {
            return;
        }

        float targetFov = _isAiming ? m_aimFov : m_normalFov;

        m_cinemachineCamera.Lens.FieldOfView = Mathf.Lerp(
            m_cinemachineCamera.Lens.FieldOfView,
            targetFov,
            Time.deltaTime * m_zoomSpeed
        );
    }

    private void OnAimStarted(InputAction.CallbackContext context)
    {
        _isAiming = true;
    }

    private void OnAimCanceled(InputAction.CallbackContext context)
    {
        _isAiming = false;
    }
}