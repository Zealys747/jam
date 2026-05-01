using UnityEngine;

public class BeanLifetime : MonoBehaviour
{

    public float _turkaRadius;
    public Transform _turkaTransform;

    private float _checkDelay = 1.2f;
    private float _alive = 0f;

    private bool _landed = false;

    public void Init(float turkaRadius, Transform turkaTransform)
    {

        _turkaRadius = turkaRadius;
        _turkaTransform = turkaTransform;
    }

    private void Update()
    {
        if (_landed || _turkaTransform == null) return;

        _alive += Time.deltaTime;

        if (_alive < _checkDelay) return;

        _landed = true;

        bool inTurka = Vector3.Distance(transform.position, _turkaTransform.position) <= _turkaRadius;

        if (inTurka)
        {
            Debug.Log("Bean in turka, destroy");
        }

        else
        {
            Debug.Log("Bean outside turka, destroy");
        }

        Destroy(gameObject);
    }


}
