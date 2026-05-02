using UnityEngine;

public class BurnerTiltPhysics : MonoBehaviour
{
    [Header("Динамика наклона")]
    [SerializeField] private float spring = 30f;
    [SerializeField] private float damping = 9f;

    private Quaternion _initialLocalRot;
    private Vector2 _tilt, _vel;
    private bool _init;

    public Quaternion InitialLocalRotation => _initialLocalRot;

    public void Init(Transform root)
    {
        _initialLocalRot = root.localRotation;
        _tilt = Vector2.zero;
        _vel = Vector2.zero;
        _init = true;
    }

    public Vector2 Step(Vector2 target, float dt)
    {
        if (!_init) return _tilt;
        Vector2 acc = (target - _tilt) * spring - _vel * damping;
        _vel += acc * dt;
        _tilt += _vel * dt;
        return _tilt;
    }
}