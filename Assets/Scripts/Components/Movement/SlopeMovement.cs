using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Raygeas.PlayerController;

public class SlopeMovement : MonoBehaviour
{

    [SerializeField] private LayerMask groundLayer;
    private float Ray_Distance = 2.0f;      // 경사면 검사할 거리
    private float Max_SlopeAngle = 45.0f;   // 경사면 오르지 못하는 각도
    private float Min_SlopeAngle = 7.5f;    // 평지로 간주할 최소 경사각도
    private RaycastHit slopeHit;            // 경사면 충돌 결과

    public bool OnSlope()
    {
        Ray ray = new Ray(transform.position, Vector3.down);

        if (Physics.Raycast(ray, out slopeHit, Ray_Distance, groundLayer))
        {
            float angle = Vector3.Angle(slopeHit.normal, Vector3.up); // 레이로 쏴서 닿아진 면적과 계산

            if (angle != 0 && angle < Max_SlopeAngle && angle > Min_SlopeAngle)
                return true;
            else return false;
        }

        return false;
    }

    // 경사면에 따른 방향 조정
    public Vector3 AdjustDirecionToSlope(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (Application.isPlaying == false)
        {
            return;
        }

        // Forward
        {
            Gizmos.color = Color.yellow;
            Vector3 from = transform.position;
            Vector3 to = transform.forward * 2.0f + transform.position;
            Gizmos.DrawLine(from, to);
        }

        // Down
        {
            Gizmos.color = Color.red;
            Vector3 from = transform.position;
            Vector3 to = Vector3.down * Ray_Distance + transform.position;
            Gizmos.DrawLine(from, to);
        }
    }

#endif
}
