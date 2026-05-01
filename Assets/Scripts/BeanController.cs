using UnityEngine;
using UnityEngine.InputSystem; // нужен для Mouse.current

public class BeanController : MonoBehaviour
{
    [Header("префабы")]
    public GameObject[] beanPrefabs;

    [Header("точка для спавна бобов")]
    public Transform spawnPoint;
    public float spawnRadius = 0.5f;

    [Header("лимиты")]
    public int maxBeans = 50;
    public float spawnRate = 6f;

    [Header("физика")]
    public float beanMass = 0.1f;
    public PhysicsMaterial beanMaterial;

    [Header("зона турки")]
    public float zoneRadius = 0.5f;

    public float BeanFill => Mathf.Clamp01((float)_beanCount / maxBeans);
    public int BeanCount => _beanCount;

    private int _beanCount;
    private float _spawnTimer;

    private void Update()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.isPressed && _beanCount < maxBeans)
        {
            _spawnTimer += Time.deltaTime;
            float spawnInterval = 1f / spawnRate;

            while (_spawnTimer >= spawnInterval && _beanCount < maxBeans)
            {
                SpawnBean();
                _spawnTimer -= spawnInterval;
            }
        }
        else
        {
            _spawnTimer = 0f;
        }
    }

    private void SpawnBean()
    {

        if (beanPrefabs == null || beanPrefabs.Length == 0) return;

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

        BeanLifetime lifetime = bean.AddComponent<BeanLifetime>();
        lifetime.Init(this, zoneRadius, spawnPoint);

        _beanCount++;
    }

    public void OnBeanDestroyed()
    {
        _beanCount = Mathf.Max(0, _beanCount - 1);
    }

    public void Clear()
    {
    
        foreach (var bean in FindObjectsByType<BeanLifetime>(FindObjectsSortMode.None))
            Destroy(bean.gameObject);

        _beanCount = 0;
    }
}