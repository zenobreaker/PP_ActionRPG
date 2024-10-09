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

        // Optional: Visualization for debugging in Scene View (draw the capsule)
        Debug.DrawLine(startPoint.position, endPoint.position, Color.green, 3.0f); // Draw the trajectory line
        Debug.DrawRay(startPoint.position, Vector3.up * capsuleRadius, Color.green); // Draw radius at start
        Debug.DrawRay(endPoint.position, Vector3.up * capsuleRadius, Color.green);   // Draw radius at end

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
            if(hitList.Contains(hitCollider.gameObject))
                continue;

            Debug.Log($"Hit: {hitCollider.name}");
            hitList.Add(hitCollider.gameObject);
            OnDamage?.Invoke(hitCollider);
        }
    }
}
