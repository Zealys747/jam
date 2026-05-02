using UnityEngine;
using UnityEngine.InputSystem;

public class BagController : MonoBehaviour
{
    [Header("перемещения мешка")]
    public Transform pointDown;
    public Transform pointUp;

    public float moveSpeed = 5f;
    public float rotateSpeed = 5f;

    [Header("Задержка перед спавном")]
    public float spawnDelay = 0.4f;

    [Header("Префабы")]
    public GameObject[] beanPrefabs;

    [Header("Точка спавна")]
    public Transform spawnPoint;
    public float spawnRadius = 0.3f;

    [Header("Лимиты")]
    public float spawnRate = 6f;

    [Header("физика")]
    public float beanMass = 0.1f;
    public PhysicsMaterial beanMaterial;

    [Header("зона турки")]
    public Transform turkaTransform;
    public float turkaRadius = 0.5f;

    private bool _isLifted = false;
    private float _liftTimer = 0f;
    private bool _spawning = false;
    private float _spawnTimer = 0f;
    private Mouse _mouse;

    void Start()
    {
        _mouse = Mouse.current;

        if (pointDown != null)
        {
            transform.position = pointDown.position;
            transform.rotation = pointDown.rotation;
        }
    }

    void Update()
    {
        if (_mouse == null) return;

        // ЛКМ
        if (_mouse.leftButton.wasPressedThisFrame)
        {
            _isLifted = true;
            _liftTimer = 0f;
        }

        // -ЛКМ
        if (_mouse.leftButton.wasReleasedThisFrame)
        {
            _isLifted = false;
            _liftTimer = 0f;
            _spawning = false;
            _spawnTimer = 0f;
        }

     
        if (_isLifted && !_spawning)
        {
            _liftTimer += Time.deltaTime;
            if (_liftTimer >= spawnDelay)
                _spawning = true;
        }

   
        if (_spawning)
        {
            _spawnTimer += Time.deltaTime;
            float interval = 1f / spawnRate;

            while (_spawnTimer >= interval)
            {
                SpawnBean();
                _spawnTimer -= interval;
            }
        }

      
        Transform target = _isLifted ? pointUp : pointDown;
        if (target != null)
        {
            transform.position = Vector3.Lerp(transform.position, target.position, moveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, rotateSpeed * Time.deltaTime);
        }
    }

    private void SpawnBean()
    {
        if (beanPrefabs == null || beanPrefabs.Length == 0) return;
        if (spawnPoint == null) return;

        Vector2 rand = Random.insideUnitCircle * spawnRadius;
        Vector3 pos = spawnPoint.position + new Vector3(rand.x, 0f, rand.y);

        GameObject bean = Instantiate(
            beanPrefabs[Random.Range(0, beanPrefabs.Length)],
            pos,
            Random.rotation
        );

        MeshCollider col = bean.GetComponent<MeshCollider>();
        if (col != null)
        {
            col.convex = true;
            if (beanMaterial != null)
                col.material = beanMaterial;
        }

        Rigidbody rb = bean.AddComponent<Rigidbody>();
        rb.mass = beanMass;
        rb.angularDamping = 1f;

        // BeanLifetime следит за зоной турки
        BeanLifetime lifetime = bean.AddComponent<BeanLifetime>();
        lifetime.Init(turkaRadius, turkaTransform);

   
    }



    public void Clear()
    {
        foreach (var bean in FindObjectsByType<BeanLifetime>(FindObjectsSortMode.None))
            Destroy(bean.gameObject);
   
    }
}