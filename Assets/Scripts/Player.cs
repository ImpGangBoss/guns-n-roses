using UnityEngine;
using UnityEngine.Pool;

public class Player : MonoBehaviour
{
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform spawnPoint;
    [SerializeField] Transform container;
    ObjectPool<GameObject> _pool;
    ThirdPersonMovement _movementController;

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
        _movementController = transform.GetComponent<ThirdPersonMovement>();
        if (_movementController == null)
            Debug.LogError("Movement controller wasn't found"); 

        _pool = new ObjectPool<GameObject>(() => Instantiate(bulletPrefab),
            bullet =>
            {
                bullet.transform.SetPositionAndRotation(spawnPoint.position, transform.rotation);
                bullet.transform.SetParent(container);
                bullet.gameObject.SetActive(true);
            },
            bullet =>
            {
                bullet.gameObject.SetActive(false);
            },
            bullet =>
            {
                Destroy(bullet.gameObject);
            }, false, 10, 100);
    }

    public void Shoot()
    {
        _pool.Get().GetComponent<Bullet>().Shoot();
    }

    public void ReleaseBullet(GameObject bullet)
    {
        _pool.Release(bullet);
    }

    public Vector3 GetPosition() => transform.position;
    public Vector3 GetMoveDirection() => _movementController.GetMoveDirection();
}

