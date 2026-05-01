using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class BurnerCollisionForceReporter : MonoBehaviour
{
    [SerializeField] private float minImpulse = 0.05f;
    [SerializeField] private float stayImpulseMultiplier = 0.15f;

    private BurnerTiltController _target;

    public void Bind(BurnerTiltController target)
    {
        _target = target;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Report(collision.impulse);
    }

    private void OnCollisionStay(Collision collision)
    {
        Report(collision.impulse * stayImpulseMultiplier);
    }

    private void Report(Vector3 impulse)
    {
        if (_target == null)
            return;

        if (impulse.sqrMagnitude < minImpulse * minImpulse)
            return;

        _target.RegisterCollisionImpulse(impulse);
    }
}
