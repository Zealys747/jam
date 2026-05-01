using UnityEngine;

public class BeanLifetime : MonoBehaviour
{
    private BeanController _beanController;
    private float _turkaRadius;
    private Transform _turkaTransform;
    private Rigidbody _body;

    private const float CheckDelay = 1.2f;
    private float _alive;
    private bool _isInitialized;
    private bool _destroyReported;

    public bool IsInitialized => _isInitialized;
    public BeanController Owner => _beanController;

    public void Init(BeanController beanController, float turkaRadius, Transform turkaTransform, Rigidbody body)
    {
        _beanController = beanController;
        _turkaRadius = turkaRadius;
        _turkaTransform = turkaTransform;
        _body = body;
        _alive = 0f;
        _destroyReported = false;
        _isInitialized = _beanController != null && _turkaTransform != null;
    }

    private void Update()
    {
        if (!_isInitialized)
            return;

        _alive += Time.deltaTime;
        if (_alive < CheckDelay || _turkaTransform == null)
            return;

        if (Vector3.Distance(transform.position, _turkaTransform.position) > _turkaRadius)
        {
            NotifyDestroyed();
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        NotifyDestroyed();
    }

    private void NotifyDestroyed()
    {
        if (!_isInitialized || _destroyReported || _beanController == null)
            return;

        _destroyReported = true;
        _beanController.OnBeanDestroyed(_body);
    }

}
