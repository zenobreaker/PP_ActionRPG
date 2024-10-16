using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Raygeas.PlayerController;

public class FootIK : MonoBehaviour
{
    [SerializeField, Range(0, 1)] private float distance = 0;
    [SerializeField, Range(0, 1)] private float weight = 1;

    //[SerializeField] private LayerMask groundLayers;

    private CapsuleCollider capsule;

    private Animator playerAnimator;

    #region Foot IK
    private Transform leftFootTransform = null;
    private Transform rightFootTransform = null;

    private Transform leftFootOrientationReference = null;
    private Transform rightFootOrientationReference = null;

    private Vector3 initialForwardVector = new Vector3();

    public float _LengthFromHeelToToes
    {
        get { return lengthFromHeelToToes; }
    }

    public float _RaySphereRadius
    {
        get { return raySphereRadius; }
    }

    public float _LeftFootProjectedAngle
    {
        get { return leftFootProjectedAngle; }
    }

    public float _RightFootProjectedAngle
    {
        get { return rightFootProjectedAngle; }
    }

    public Vector3 _LeftFootIKPositionTarget
    {
        get
        {
            if (Application.isPlaying == true)
            {
                return leftFootIKPositionTarget;
            }
            else
            {
                // This is being done because the IK target only gets updated during playmode
                return new Vector3(0, GetAnkleHeight() + _WorldHeightOffset, 0);
            }
        }
    }

    public Vector3 _RightFootIKPositionTarget
    {
        get
        {
            if (Application.isPlaying == true)
            {
                return rightFootIKPositionTarget;
            }
            else
            {
                // This is being done because the IK target only gets updated during playmode
                return new Vector3(0, GetAnkleHeight() + _WorldHeightOffset, 0);
            }
        }
    }

    public float _AnkleHeightOffset
    {
        get
        {
            return ankleHeightOffset;
        }
    }

    public float _WorldHeightOffset
    {
        get
        {
            if (giveWorldHeightOffset == true)
            {
                return worldHeightOffset;
            }
            else
            {
                return 0;
            }
        }
    }


    [BigHeader("Foot Properties")]

    [SerializeField]
    [Range(0, 0.25f)]
    private float lengthFromHeelToToes = 0.1f;
    [SerializeField]
    [Range(0, 60)]
    private float maxRotationAngle = 45;
    [SerializeField]
    [Range(-0.05f, 0.125f)]
    private float ankleHeightOffset = 0;

    [BigHeader("IK Porperties")]

    [SerializeField] private bool enableIKPositioing = true;
    [SerializeField] private bool enableIKRotating = true;
    [SerializeField]
    [Range(0, 1)] private float globalWeight = 1;
    [SerializeField]
    [Range(0, 1)] private float leftFootWeight = 1;
    [SerializeField]
    [Range(0, 1)] private float rightFootWeight = 1;
    [SerializeField]
    [Range(0, 0.1f)] private float smmothTime = 0.075f;


    [BigHeader("Ray Properties")]

    [SerializeField]
    [Range(0.05f, 0.1f)]
    private float raySphereRadius = 0.05f;
    [SerializeField]
    [Range(0.1f, 2)]
    private float rayCastRange = 2;
    [SerializeField]
    private LayerMask groundLayers = new LayerMask();
    [SerializeField]
    private bool ignoreTriggers = true;

    [BigHeader("Raycast Start Heights")]

    [SerializeField]
    [Range(0.1f, 1.0f)] private float leftFootRayStartHeight = 0.5f;
    [SerializeField]
    [Range(0.1f, 1.0f)] private float rightFootRayStartHeight = 0.5f;

    [BigHeader("Advanced")]

    [SerializeField]
    private bool enableFootLifting = true;
    //[ShowIf("enableFootLifting")]
    [SerializeField]
    private float floorRange = 0;
    [SerializeField]
    private bool enableBodyPositioning = true;
    //[ShowIf("enableBodyPositioning")]
    [SerializeField]
    private float crouchRange = 0.25f;
    //[ShowIf("enableBodyPositioning")]
    [SerializeField]
    private float stretchRange = 0;
    [SerializeField]
    private bool giveWorldHeightOffset = false;
    //[ShowIf("giveWorldHeightOffset")]
    [SerializeField]
    private float worldHeightOffset = 0;

    private RaycastHit leftFootRayHitInfo = new RaycastHit();
    private RaycastHit rightFootRayHitInfo = new RaycastHit();

    private float leftFootRayHitHeight = 0;
    private float rightFootRayHitHeight = 0;

    private Vector3 leftFootRayStartPosition = new Vector3();
    private Vector3 rightFootRayStartPosition = new Vector3();

    private Vector3 leftFootDirectionVector = new Vector3();
    private Vector3 rightFootDirectionVector = new Vector3();

    private Vector3 leftFootProjectionVector = new Vector3();
    private Vector3 rightFootProjectionVector = new Vector3();

    private float leftFootProjectedAngle = 0;
    private float rightFootProjectedAngle = 0;

    private Vector3 leftFootRayHitProjectionVector = new Vector3();
    private Vector3 rightFootRayHitProjectionVector = new Vector3();

    private float leftFootRayHitProjectedAngle = 0;
    private float rightFootRayHitProjectedAngle = 0;

    private float leftFootHeightOffset = 0;
    private float rightFootHeightOffset = 0;

    private Vector3 leftFootIKPositionBuffer = new Vector3();
    private Vector3 rightFootIKPositionBuffer = new Vector3();

    private Vector3 leftFootIKPositionTarget = new Vector3();
    private Vector3 rightFootIKPositionTarget = new Vector3();

    private float leftFootHeightLerpVelocity = 0;
    private float rightFootHeightLerpVelocity = 0;

    private Vector3 leftFootIKRotationBuffer = new Vector3();
    private Vector3 rightFootIKRotationBuffer = new Vector3();

    private Vector3 leftFootIKRotationTarget = new Vector3();
    private Vector3 rightFootIKRotationTarget = new Vector3();

    private Vector3 leftFootRotationLerpVelocity = new Vector3();
    private Vector3 rightFootRotationLerpVelocity = new Vector3();

    private GUIStyle helperTextStyle = null;

    #endregion
    private void Awake()
    {
        capsule = GetComponent<CapsuleCollider>();
        playerAnimator = GetComponent<Animator>();
    }

    private void LateUpdate()
    {
        
    }
    private void OnAnimatorIK(int layerIndex)
    {

        Test_IK();
        //Vector3 position = transform.position;
        //position.y += capsule.center.y;

        ////레이를 쏜다
        //Ray ray = new Ray(position, Vector3.down * 2.0f);
        //Debug.DrawRay(ray.origin, ray.direction * (distance + 1), Color.green);
        //RaycastHit hit;
        //if (Physics.Raycast(ray, out hit, capsule.center.y, layerMask))
        //{
        //    float distance = hit.distance - capsule.center.y;
        //    position = transform.position;
        //    position.y -= distance;
        //    transform.position = position;
        //    //Debug.Log($"Foot IK {position}");

        //    // 얼마만큼에 가중치 => 0이면 원점 1이면 조정한 위치 떨어질땐 0에 가깝게 붙을땐 1로 
        //    SetFootIK(AvatarIKGoal.LeftFoot, distance);
        //    SetFootIK(AvatarIKGoal.RightFoot, distance);
        //}

    }

    private void InitializeVariables()
    {
        playerAnimator = GetComponent<Animator>();

        leftFootTransform = playerAnimator.GetBoneTransform(HumanBodyBones.LeftFoot);
        rightFootTransform = playerAnimator.GetBoneTransform(HumanBodyBones.RightFoot);

        // This is for faster development iteration purposes
        if (groundLayers.value == 0)
        {
            groundLayers = LayerMask.GetMask("Default");
        }

        // This is needed in order to wrangle with quaternions to get the final direction vector of each foot later
        initialForwardVector = transform.forward;

        // Initial value is given to make the first frames of lerping look natural, rotations should not need these
        leftFootIKPositionBuffer.y = transform.position.y + GetAnkleHeight();
        rightFootIKPositionBuffer.y = transform.position.y + GetAnkleHeight();

        // This is being done here due to internal unity reasons
        helperTextStyle = new GUIStyle()
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        helperTextStyle.normal.textColor = Color.yellow;
    }



    private void Test_IK()
    {
        Vector3 position = transform.position;
        position.y += capsule.center.y / 2;

        Ray ray = new Ray(position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, capsule.center.y / 2 + 0.5f, groundLayers) == true)
        {
            position = transform.position;

            float currentBaseY = transform.position.y;
            float detectedBaseY = hit.point.y;

            float gap = currentBaseY - detectedBaseY;

            position.y -= gap;
            transform.position = position;
        }

        SetFootIK(AvatarIKGoal.LeftFoot, distance);
        SetFootIK(AvatarIKGoal.RightFoot, distance);
    }


    private void SetFootIK(AvatarIKGoal goal, float adjsut)
    {
        playerAnimator.SetIKPositionWeight(goal, weight);
        playerAnimator.SetIKRotationWeight(goal, weight);


        Ray ray = new Ray(playerAnimator.GetIKPosition(goal) + Vector3.up, Vector3.down);
        Debug.DrawRay(ray.origin, ray.direction * (distance + 1), Color.green);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, distance + 1, groundLayers))  // 나중엔 여길 레이어 마스크로 처리한다. 
        {
            Vector3 foot = hit.point;
            foot.y += distance - adjsut;

            playerAnimator.SetIKPosition(goal, foot);
            playerAnimator.SetIKRotation(goal, Quaternion.LookRotation(transform.forward, hit.normal));

        }
    }

    private float GetAnkleHeight()
    {
        return raySphereRadius + _AnkleHeightOffset;
    }



    private void SetHandIK(AvatarIKGoal goal, float adjsut)
    {
        playerAnimator.SetIKPositionWeight(goal, weight);
        playerAnimator.SetIKRotationWeight(goal, weight);


        Ray ray = new Ray(playerAnimator.GetIKPosition(goal) + Vector3.up, Vector3.down);
        Debug.DrawRay(ray.origin, ray.direction * (distance + 1), Color.green);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, distance + 1, groundLayers))  // 나중엔 여길 레이어 마스크로 처리한다. 
        {
            Vector3 foot = hit.point;
            foot.y += distance - adjsut;

            playerAnimator.SetIKPosition(goal, foot);
            playerAnimator.SetIKRotation(goal, Quaternion.LookRotation(transform.forward, hit.normal));

        }
    }


    /////////////////////////////////////////////////////////////////////////////


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Debug draw function relies on objects that are dynamically located during runtime
        if (Application.isPlaying == false)
        {
            return;
        }

        /* Left Foot */

        if (leftFootRayHitInfo.collider != null)
        {
            Handles.color = Color.yellow;

            // Just note that the normal vector of RayCastHit object is used here
            Handles.DrawWireDisc(
                leftFootRayHitInfo.point,
                leftFootRayHitInfo.normal,
                0.1f);
            Handles.DrawDottedLine(
                leftFootTransform.position,
                leftFootTransform.position + leftFootRayHitInfo.normal,
                2);

            // Just note that the orientation of the parent transform is used here
            Handles.color = Color.green;
            Handles.DrawWireDisc(
                leftFootRayHitInfo.point,
                transform.up, 0.25f);

            Gizmos.color = Color.green;

            Gizmos.DrawSphere(
                leftFootRayHitInfo.point + transform.up * raySphereRadius,
                raySphereRadius);
        }
        else
        {
            Gizmos.color = Color.red;
        }

        if (leftFootProjectedAngle > 0)
        {
            Handles.color = Color.blue;
        }
        else
        {
            Handles.color = Color.red;
        }

        // Foot height correction related debug draws
        Handles.DrawWireDisc(
            leftFootTransform.position,
            leftFootOrientationReference.rotation * transform.up,
            0.15f);
        Handles.DrawSolidArc(
            leftFootTransform.position,
            Vector3.Cross(leftFootDirectionVector, leftFootProjectionVector) * -1,
            leftFootProjectionVector,
            // Abs is needed here because the cross product will deal with axis direction
            Mathf.Abs(leftFootProjectedAngle),
            0.25f);
        Handles.DrawDottedLine(
            leftFootTransform.position,
            leftFootTransform.position + leftFootDirectionVector.normalized,
            2);

        // SphereCast related debug draws
        Gizmos.DrawWireSphere(
            leftFootRayStartPosition,
            0.1f);
        Gizmos.DrawLine(
            leftFootRayStartPosition,
            leftFootRayStartPosition - rayCastRange * Vector3.up);

        // Indicator text
        Handles.Label(leftFootTransform.position, "L", helperTextStyle);

        /* Right foot */

        if (rightFootRayHitInfo.collider != null)
        {
            Handles.color = Color.yellow;

            // Just note that the normal vector of RayCastHit object is used here
            Handles.DrawWireDisc(
                rightFootRayHitInfo.point,
                rightFootRayHitInfo.normal,
                0.1f);
            Handles.DrawDottedLine(
                rightFootTransform.position,
                rightFootTransform.position + rightFootRayHitInfo.normal,
                2);

            // Just note that the orientation of the parent transform is used here
            Handles.color = Color.green;
            Handles.DrawWireDisc(
                rightFootRayHitInfo.point,
                transform.up, 0.25f);

            Gizmos.color = Color.green;

            Gizmos.DrawSphere(
                rightFootRayHitInfo.point + transform.up * raySphereRadius,
                raySphereRadius);
        }
        else
        {
            Gizmos.color = Color.red;
        }

        if (rightFootProjectedAngle > 0)
        {
            Handles.color = Color.blue;
        }
        else
        {
            Handles.color = Color.red;
        }

        // Foot height correction related debug draws
        Handles.DrawWireDisc(
            rightFootTransform.position,
            rightFootOrientationReference.rotation * transform.up,
            0.15f);
        Handles.DrawSolidArc(
            rightFootTransform.position,
            Vector3.Cross(rightFootDirectionVector, rightFootProjectionVector) * -1,
            rightFootProjectionVector,
            // Abs is needed here because the cross product will deal with axis direction
            Mathf.Abs(rightFootProjectedAngle),
            0.25f);
        Handles.DrawDottedLine(
            rightFootTransform.position,
            rightFootTransform.position + rightFootDirectionVector.normalized,
            2);

        // SphereCast related debug draws
        Gizmos.DrawWireSphere(
            rightFootRayStartPosition,
            0.1f);
        Gizmos.DrawLine(
            rightFootRayStartPosition,
            rightFootRayStartPosition - rayCastRange * Vector3.up);

        // Indicator text
        Handles.Label(rightFootTransform.position, "R", helperTextStyle);
    }
#endif
}





[AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
public class ShowIfAttribute : PropertyAttribute
{
    public string _BaseCondition
    {
        get { return mBaseCondition; }
    }

    private string mBaseCondition = String.Empty;

    public ShowIfAttribute(string baseCondition)
    {
        mBaseCondition = baseCondition;
    }
}



[AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
public class BigHeaderAttribute : PropertyAttribute
{
    public string _Text
    {
        get { return mText; }
    }

    private string mText = String.Empty;

    public BigHeaderAttribute(string text)
    {
        mText = text;
    }
}


