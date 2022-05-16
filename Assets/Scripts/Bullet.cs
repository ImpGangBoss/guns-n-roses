using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float bulletSpeed = 2f;
    [SerializeField] float damage = 1f;

    void Start()
    {

    }

    private void OnCollisionEnter(Collision other) 
    {;
        Player.Instance.ReleaseBullet(gameObject);
    }

    public void Shoot()
    {
        var rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        rb.AddForce(transform.forward * bulletSpeed, ForceMode.Impulse);
    }
}
