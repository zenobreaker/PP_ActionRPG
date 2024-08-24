using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float force = 1000.0f;
    [SerializeField] private float life = 10.0f;

    private new Rigidbody rigidbody;
    private new Collider collider;

    [SerializeField]
    private GameObject hitPrefab;

    public GameObject HitPrefab { set => hitPrefab = value; }

    public event Action<Collider, Collider, Vector3> OnProjectileHit;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
    }

    private void Start()
    {
        Destroy(gameObject, life);

        rigidbody.AddForce(transform.forward * force);
    }


    private void OnTriggerEnter(Collider other)
    {
        OnProjectileHit?.Invoke(collider, other, transform.position);

        Destroy(gameObject);

    }

}
