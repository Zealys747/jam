using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BurnerCollisionForceReporter : MonoBehaviour
{
    private BurnerTiltController _ctrl;
    private Collider _col;

    public void Bind(BurnerTiltController controller) => _ctrl = controller;

    private void Awake() => _col = GetComponent<Collider>();

    private void OnCollisionEnter(Collision collision) => Report(collision);
    private void OnCollisionStay(Collision collision) => Report(collision);

    private void Report(Collision collision)
    {
        if (_ctrl == null || _col == null || _col.isTrigger) return;
        float mass = collision.rigidbody != null ? collision.rigidbody.mass : 1f;
        _ctrl.RegisterCollisionImpulse(mass * collision.relativeVelocity);
    }
}