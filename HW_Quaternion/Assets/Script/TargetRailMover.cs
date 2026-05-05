using UnityEngine;

public class TargetRailMover : MonoBehaviour
{
    [Header("Move Settings")]
    [SerializeField] private float _rotateSpeed = 30f;

    private void Update()
    {
        transform.Rotate(Vector3.up, _rotateSpeed * Time.deltaTime);
    }
}