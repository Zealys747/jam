using UnityEngine;
using System.Collections.Generic;

public class BurnerLoadCalculator : MonoBehaviour
{
    [Header("Вес / нагрузка")]
    private bool includeTurkaMassInWeight = true;
    private bool useTurkaRigidbodyMass = true;
    [SerializeField] private float turkaMass = 1.2f;
    private bool includeBeanMassInWeight = true;
    private float beanMassMultiplier = 1f;
    private float weightToTilt = 1f;
    private float deadZone = 0.005f;

    private const float MinMass = 0.0001f;
    private Transform _burnerRoot, _turka, _space;
    private Rigidbody _turkaBody;
    private BeanController _beanCtrl;
    private Vector2 _baseLoadOffset;

    public void Init(Transform burnerRoot, Transform turka, bool calibrate, Transform space)
    {
        _burnerRoot = burnerRoot;
        _turka = turka;
        _space = space != null ? space : burnerRoot;
        if (turka != null) _turkaBody = turka.GetComponent<Rigidbody>();
        _beanCtrl = FindAnyObjectByType<BeanController>();
        if (calibrate) _baseLoadOffset = ComputeLoadOffset(out _);
    }

    public Vector2 CalculateTilt()
    {
        if (_burnerRoot == null) return Vector2.zero;
        Vector2 offset = ComputeLoadOffset(out float mass) - _baseLoadOffset;
        if (mass <= Mathf.Epsilon || offset.sqrMagnitude < deadZone * deadZone) return Vector2.zero;

        float force = mass * Mathf.Abs(Physics.gravity.y);
        Vector2 lat = offset * force * weightToTilt;
        return new Vector2(-lat.y, lat.x);
    }

    private Vector2 ComputeLoadOffset(out float totalMass)
    {
        totalMass = 0f;
        Vector3 weighted = Vector3.zero;

        if (includeTurkaMassInWeight && _turka != null)
            AddMass(_turka.position, GetTurkaMass(), ref weighted, ref totalMass);

        if (includeBeanMassInWeight && _beanCtrl != null)
        {
            var beans = _beanCtrl.ActiveBeanBodies;
            float maxDistSqr = _beanCtrl.zoneRadius * _beanCtrl.zoneRadius;
            Vector3 center = _beanCtrl.spawnPoint != null ? _beanCtrl.spawnPoint.position : (_turka != null ? _turka.position : _burnerRoot.position);

            foreach (var b in beans)
            {
                if (b == null || !b.gameObject.activeInHierarchy) continue;
                if (_beanCtrl.zoneRadius > 0f)
                {
                    Vector2 p = new Vector2(b.worldCenterOfMass.x - center.x, b.worldCenterOfMass.z - center.z);
                    if (p.sqrMagnitude > maxDistSqr) continue;
                }
                AddMass(b.worldCenterOfMass, Mathf.Max(MinMass, b.mass * beanMassMultiplier), ref weighted, ref totalMass);
            }
        }

        if (totalMass <= Mathf.Epsilon || _space == null) return Vector2.zero;
        Vector3 worldCenter = weighted / totalMass;
        return _space.WorldToLocalPlanar(worldCenter - _burnerRoot.position);
    }

    private float GetTurkaMass()
    {
        if (useTurkaRigidbodyMass && _turkaBody != null) return Mathf.Max(MinMass, _turkaBody.mass);
        return Mathf.Max(MinMass, turkaMass);
    }

    private static void AddMass(Vector3 pos, float mass, ref Vector3 w, ref float t)
    {
        if (mass <= 0f) return;
        w += pos * mass; t += mass;
    }
}
