using UnityEngine;

public class BurnerImpulseHandler : MonoBehaviour
{
    [Header("Внешние силы")]
    private bool includeCollisionImpulses = true;
    private bool autoBindCollisionReporters = true;
    private bool includeTurkaColliders = true;
    private float impulseToTilt = 0.12f;
    private float impulseRecoverySpeed = 30f;
    private float maxImpulseTilt = 10f;

    private BurnerTiltController _ctrl;
    private Transform _burnerRoot, _turka;
    private Vector2 _impulseTilt;
    private bool _init;

    public void Init(BurnerTiltController ctrl, Transform burnerRoot, Transform turka, bool syncVisual)
    {
        _ctrl = ctrl;
        _burnerRoot = burnerRoot;
        _turka = turka;
        _impulseTilt = Vector2.zero;
        _init = true;
        if (autoBindCollisionReporters) BindReporters();
    }

    public void AddImpulse(Vector3 worldImpulse)
    {
        if (!_init || !includeCollisionImpulses) return;
        if (_burnerRoot == null) return;

        Transform space = _burnerRoot.parent != null ? _burnerRoot.parent : _burnerRoot;
        Vector2 planar = space.WorldToLocalPlanar(worldImpulse) * impulseToTilt;
        Vector2 delta = new Vector2(-planar.y, planar.x);
        _impulseTilt += delta;
        _impulseTilt = Vector2.ClampMagnitude(_impulseTilt, maxImpulseTilt);
    }

    public Vector2 GetAndDecay(float dt)
    {
        if (!_init || !includeCollisionImpulses) return Vector2.zero;
        Vector2 res = _impulseTilt;
        _impulseTilt = Vector2.MoveTowards(_impulseTilt, Vector2.zero, impulseRecoverySpeed * dt);
        return res;
    }

    private void BindReporters()
    {
        if (_burnerRoot == null) return;
        BindUnder(_burnerRoot);
        if (includeTurkaColliders && _turka != null && !_turka.IsChildOf(_burnerRoot))
            BindUnder(_turka);
    }

    private void BindUnder(Transform root)
    {
        var cols = root.GetComponentsInChildren<Collider>(true);
        foreach (var c in cols)
        {
            if (c == null || c.isTrigger) continue;
            var r = c.GetComponent<BurnerCollisionForceReporter>();
            if (r == null) r = c.gameObject.AddComponent<BurnerCollisionForceReporter>();
            r.Bind(_ctrl);
        }
    }
}
