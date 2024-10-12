using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon_Trail_Collision : MonoBehaviour
{
    [SerializeField] private float capsuleRadius = 0.5f;  // Adjust the radius based on weapon thickness

    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;

    private List<GameObject> hitList = new List<GameObject>();

    private Melee melee; 
    private bool bActivate;
    private GameObject rootObject;

    public bool bDebug;
    public event Action<Collider> OnDamage;

    private void Awake()
    {
        
    }

    public void SetRootObject(GameObject rootObject)
    {
        this.rootObject  = rootObject;
    }

    private void LateUpdate()
    {
        if (bActivate == false)
            return;

        Create_CapsuleCollision();
    }


    public void OnActivate()
    {
        bActivate = true;

        hitList.Clear();
    }

    public void OnInactivate()
    {
        bActivate = false;

        hitList.Clear();
    }

    private void Create_CapsuleCollision()
    {
        if (startPoint == null || endPoint == null)
            return;

        Vector3 capsuleCenter = (startPoint.position - endPoint.position) / 2;

        float capsuleHeight = Vector3.Distance(startPoint.position, endPoint.position);

        Vector3 capsuleDirection = (endPoint.position - startPoint.position).normalized;

        if (bDebug)
        {
            // Optional: Visualization for debugging in Scene View (draw the capsule)
            Debug.DrawLine(startPoint.position, endPoint.position, Color.green, 3.0f); // Draw the trajectory line
            Debug.DrawRay(startPoint.position, Vector3.up * capsuleRadius, Color.green); // Draw radius at start
            Debug.DrawRay(endPoint.position, Vector3.up * capsuleRadius, Color.green);   // Draw radius at end
        }
        // Create the capsule using Physics.OverlapCapsule for hit detection
        Collider[] hitColliders = Physics.OverlapCapsule(startPoint.position, endPoint.position, capsuleRadius);

        // Loop through all the colliders hit by the capsule
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject == this.gameObject)
                continue;
            if (hitCollider.gameObject == rootObject)
                continue;
            if (hitCollider.gameObject.layer == LayerMask.NameToLayer("Weapon"))
                continue;
            if (hitList.Contains(hitCollider.gameObject))
                continue;

            Debug.Log($"Hit: {hitCollider.name}");
            hitList.Add(hitCollider.gameObject);
            OnDamage?.Invoke(hitCollider);
        }
    }

    //private void Create_CapsuleCollision()
    //{
    //    // Calculate the capsule center correctly as the midpoint between startPoint and endPoint
    //    Vector3 capsuleCenter = (startPoint.position + endPoint.position) / 2;

    //    // Calculate the capsule height
    //    float capsuleHeight = Vector3.Distance(startPoint.position, endPoint.position);

    //    // Normalize the direction for consistent usage
    //    Vector3 capsuleDirection = (endPoint.position - startPoint.position).normalized;

    //    // Set rotation angle and number of directions to check
    //    float angleStep = 15f; // 15 degrees
    //    int numberOfDirections = 3; // Total directions including the original direction

    //    // Store the rotated directions
    //    List<Vector3> directions = new List<Vector3>();

    //    // Add the original direction
    //    directions.Add(capsuleDirection);

    //    // Rotate by 15 degrees to create new directions
    //    for (int i = 1; i <= numberOfDirections; i++)
    //    {
    //        // Calculate the angle to rotate
    //        float angle = angleStep * i;

    //        // Rotate the direction vector
    //        Vector3 rotatedDirection = Quaternion.Euler(0, angle, 0) * capsuleDirection;

    //        // Add the rotated direction to the list
    //        directions.Add(rotatedDirection);
    //    }

    //    // Create the capsule using Physics.OverlapCapsule for hit detection
    //    foreach (var direction in directions)
    //    {
    //        if (bDebug)
    //        {
    //            // Optional: Visualization for debugging in Scene View (draw the capsule)
    //            //Debug.DrawLine(startPoint.position, startPoint.position + direction * capsuleHeight, Color.green, 3.0f); // Draw the trajectory line
    //            Debug.DrawLine(capsuleCenter, capsuleCenter + direction * capsuleHeight, Color.green, 3.0f); // Draw the trajectory line
    //            Debug.DrawRay(startPoint.position, Vector3.up * capsuleRadius, Color.green); // Draw radius at start
    //            Debug.DrawRay(endPoint.position, Vector3.up * capsuleRadius, Color.green);   // Draw radius at end
    //        }

    //        // Use the capsule center and direction to perform hit detection
    //        Collider[] hitColliders = Physics.OverlapCapsule(capsuleCenter, capsuleCenter + direction * capsuleHeight, capsuleRadius);

    //        // Loop through all the colliders hit by the capsule
    //        foreach (var hitCollider in hitColliders)
    //        {
    //            if (hitCollider.gameObject == this.gameObject)
    //                continue;
    //            if (hitCollider.gameObject == rootObject)
    //                continue;
    //            if (hitCollider.gameObject.layer == LayerMask.NameToLayer("Weapon"))
    //                continue;
    //            if (hitList.Contains(hitCollider.gameObject))
    //                continue;

    //            Debug.Log($"Hit: {hitCollider.name}");
    //            hitList.Add(hitCollider.gameObject);
    //            OnDamage?.Invoke(hitCollider);
    //        }
    //    }

    //}
}
