using UnityEngine;

public class BeanLifetime : MonoBehaviour
{
    public BeanController _beanController;
    private float _turkaRadius;
    private Transform _turkaTransform;

    private float checkDelay = 1.2f;
    private float _alive;

    public void Init(BeanController beanController, float turkaRadius, Transform turkaTransform)
    {
        _beanController = beanController;
        _turkaRadius = turkaRadius;
        _turkaTransform = turkaTransform;
    }

    private void Update()
    {
        _alive += Time.deltaTime;
        if (_alive < checkDelay || _turkaTransform == null) return;

        if (Vector3.Distance(transform.position, _turkaTransform.position) > _turkaRadius)
        {
            Destroy(gameObject);
            _beanController.OnBeanDestroyed();
        }
    }

    private void OnDestroy()
    {
        _beanController?.OnBeanDestroyed();
    }

}
