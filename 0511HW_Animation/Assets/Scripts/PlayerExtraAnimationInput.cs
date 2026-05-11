using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerExtraAnimationInput : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    private void Update()
    {
        if (animator == null) return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            animator.ResetTrigger("Interact");
            animator.SetTrigger("Attack");
        }

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            animator.ResetTrigger("Attack");
            animator.SetTrigger("Interact");
        }
    }
}