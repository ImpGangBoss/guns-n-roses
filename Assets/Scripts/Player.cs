using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Player : MonoBehaviour
{
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform spawnPoint;
    [SerializeField] Transform container;
    private ObjectPool<GameObject> _pool;

    public static Player Instance { get; private set; }
    private void Awake() 
    {     
        if (Instance != null && Instance != this) 
            Destroy(this); 
        else 
            Instance = this; 
    }

    void Start()
    {
        _pool = new ObjectPool<GameObject>(() =>
            {
                return Instantiate(bulletPrefab);
            }, 
            
            (bullet) =>
            {
                bullet.transform.SetPositionAndRotation(spawnPoint.position, transform.rotation);
                bullet.transform.SetParent(container);
                bullet.gameObject.SetActive(true);
            }, 
            
            (bullet) =>
            {
                bullet.gameObject.SetActive(false);
            }, 
            
            (bullet) =>
            {
                Destroy(bullet.gameObject);
            }, false, 10, 100
        );
    }

    public void Shoot()
    {
        _pool.Get().GetComponent<Bullet>().Shoot();
    }

    public void ReleaseBullet(GameObject bullet)
    {
        _pool.Release(bullet);
    }
}

