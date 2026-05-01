using UnityEngine;

public class BurnerTiltController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform burnerRoot;
    [SerializeField] private Transform turka;
    [SerializeField] private Rigidbody turkaBody;
    [SerializeField] private BeanController beanController;
    [SerializeField] private string burnerObjectName = "Burner";
    [SerializeField] private string turkaObjectName = "TURKI4";

    [Header("Weight / Load")]
    [SerializeField] private bool includeTurkaMassInWeight = true;
    [SerializeField] private bool useTurkaRigidbodyMass = true;
    [SerializeField] private float turkaMass = 1.2f;
    [SerializeField] private bool includeBeanMassInWeight = true;
    [SerializeField] private float beanMassMultiplier = 1f;
    [SerializeField] private float weightToTilt = 1f;
    [SerializeField] private bool calibrateCenterAtStart = true;
    [SerializeField] private float deadZone = 0.005f;

    [Header("Tilt Dynamics")]
    [SerializeField] private float spring = 30f;
    [SerializeField] private float damping = 9f;
    [SerializeField] private float maxTiltAngle = 14f;

    [Header("External Forces")]
    [SerializeField] private bool includeCollisionImpulses = true;
    [SerializeField] private bool autoBindCollisionReporters = true;
    [SerializeField] private bool includeTurkaColliders = true;
    [SerializeField] private float impulseToTilt = 0.12f;
    [SerializeField] private float impulseRecoverySpeed = 30f;
    [SerializeField] private float maxImpulseTilt = 10f;

    [Header("Visual")]
    [SerializeField] private bool tiltTurkaVisual = true;

    private Quaternion _burnerInitialLocalRotation;
    private Quaternion _burnerInitialWorldRotation;
    private Quaternion _turkaInitialWorldRotation;
    private Transform _space;

    private Vector2 _currentTilt;
    private Vector2 _tiltVelocity;
    private Vector2 _impulseTilt;
    private Vector3 _baseLoadOffsetLocal;
    private int _lastBurnerRootId;
    private int _lastTurkaId;
    private bool _isReady;

    private void Awake()
    {
        ResolveReferences();
        InitializeState();
    }

    private void LateUpdate()
    {
        if (!EnsureReady())
            return;

        float dt = Time.deltaTime;
        if (dt <= 0f)
            return;

        Vector2 targetTilt = ComputeWeightTilt();
        targetTilt += ComputeImpulseTilt(dt);
        targetTilt.x = Mathf.Clamp(targetTilt.x, -maxTiltAngle, maxTiltAngle);
        targetTilt.y = Mathf.Clamp(targetTilt.y, -maxTiltAngle, maxTiltAngle);

        Vector2 tiltAcceleration = (targetTilt - _currentTilt) * spring - _tiltVelocity * damping;
        _tiltVelocity += tiltAcceleration * dt;
        _currentTilt += _tiltVelocity * dt;

        _currentTilt.x = Mathf.Clamp(_currentTilt.x, -maxTiltAngle, maxTiltAngle);
        _currentTilt.y = Mathf.Clamp(_currentTilt.y, -maxTiltAngle, maxTiltAngle);

        ApplyTilt();
    }

    private bool EnsureReady()
    {
        if (_isReady && burnerRoot != null && _space != null)
        {
            TryBindCollisionReporters();
            return true;
        }

        ResolveReferences();
        InitializeState();
        return _isReady;
    }

    private void ResolveReferences()
    {
        if (burnerRoot == null)
        {
            GameObject burnerObj = GameObject.Find(burnerObjectName);
            if (burnerObj != null)
                burnerRoot = burnerObj.transform;
        }

        if (turka == null)
        {
            GameObject turkaObj = GameObject.Find(turkaObjectName);
            if (turkaObj != null)
                turka = turkaObj.transform;
        }

        if (turkaBody == null && turka != null)
            turka.TryGetComponent(out turkaBody);

        if (beanController == null)
            beanController = FindAnyObjectByType<BeanController>();

        if (burnerRoot != null)
            _space = burnerRoot.parent != null ? burnerRoot.parent : burnerRoot;
    }

    private void InitializeState()
    {
        if (burnerRoot == null || _space == null)
        {
            _isReady = false;
            return;
        }

        _burnerInitialLocalRotation = burnerRoot.localRotation;
        _burnerInitialWorldRotation = burnerRoot.rotation;

        if (turka != null)
            _turkaInitialWorldRotation = turka.rotation;

        _baseLoadOffsetLocal = calibrateCenterAtStart
            ? ComputeLoadOffsetLocal(out _)
            : Vector3.zero;

        TryBindCollisionReporters(forceRebind: true);
        _isReady = true;
    }

    public void RegisterCollisionImpulse(Vector3 worldImpulse)
    {
        if (!includeCollisionImpulses)
            return;

        if (!EnsureReady())
            return;

        Vector3 localImpulse = WorldOffsetToLocalPlanar(worldImpulse);
        Vector2 planar = new Vector2(localImpulse.x, localImpulse.z) * impulseToTilt;
        Vector2 tiltDelta = new Vector2(-planar.y, planar.x);

        if (tiltDelta == Vector2.zero)
            return;

        _impulseTilt += tiltDelta;
        _impulseTilt = Vector2.ClampMagnitude(_impulseTilt, Mathf.Max(0f, maxImpulseTilt));
    }

    private Vector2 ComputeWeightTilt()
    {
        if (burnerRoot == null)
            return Vector2.zero;

        Vector3 offsetLocal = ComputeLoadOffsetLocal(out float totalMass) - _baseLoadOffsetLocal;

        if (totalMass <= Mathf.Epsilon)
            return Vector2.zero;

        if (offsetLocal.sqrMagnitude < deadZone * deadZone)
            return Vector2.zero;

        float weightForce = totalMass * Mathf.Abs(Physics.gravity.y);
        Vector2 lateral = new Vector2(offsetLocal.x, offsetLocal.z) * weightForce * weightToTilt;
        return new Vector2(-lateral.y, lateral.x);
    }

    private Vector3 ComputeLoadOffsetLocal(out float totalMass)
    {
        totalMass = 0f;
        Vector3 weightedPosition = Vector3.zero;

        if (includeTurkaMassInWeight && turka != null)
            AddMassPoint(turka.position, GetTurkaMass(), ref weightedPosition, ref totalMass);

        if (includeBeanMassInWeight && beanController != null)
        {
            var beans = beanController.ActiveBeanBodies;
            float maxDistance = beanController.zoneRadius;
            float maxDistanceSqr = maxDistance * maxDistance;
            Vector3 center = beanController.spawnPoint != null
                ? beanController.spawnPoint.position
                : (turka != null ? turka.position : burnerRoot.position);

            for (int i = 0; i < beans.Count; i++)
            {
                Rigidbody body = beans[i];
                if (body == null || !body.gameObject.activeInHierarchy)
                    continue;

                if (maxDistance > 0f)
                {
                    Vector3 offset = body.worldCenterOfMass - center;
                    Vector2 planarOffset = new Vector2(offset.x, offset.z);
                    if (planarOffset.sqrMagnitude > maxDistanceSqr)
                        continue;
                }

                float beanMass = Mathf.Max(0.0001f, body.mass * beanMassMultiplier);
                AddMassPoint(body.worldCenterOfMass, beanMass, ref weightedPosition, ref totalMass);
            }
        }

        if (totalMass <= Mathf.Epsilon)
            return Vector3.zero;

        Vector3 loadCenterWorld = weightedPosition / totalMass;
        return WorldOffsetToLocalPlanar(loadCenterWorld - burnerRoot.position);
    }

    private float GetTurkaMass()
    {
        if (useTurkaRigidbodyMass)
        {
            if (turkaBody == null && turka != null)
                turka.TryGetComponent(out turkaBody);

            if (turkaBody != null)
                return Mathf.Max(0.0001f, turkaBody.mass);
        }

        return Mathf.Max(0.0001f, turkaMass);
    }

    private Vector3 WorldOffsetToLocalPlanar(Vector3 worldOffset)
    {
        Vector3 local = _space.InverseTransformDirection(worldOffset);
        local.y = 0f;
        return local;
    }

    private Vector2 ComputeImpulseTilt(float dt)
    {
        if (!includeCollisionImpulses)
        {
            _impulseTilt = Vector2.zero;
            return Vector2.zero;
        }

        Vector2 result = _impulseTilt;
        float recoverStep = Mathf.Max(0f, impulseRecoverySpeed) * dt;
        _impulseTilt = Vector2.MoveTowards(_impulseTilt, Vector2.zero, recoverStep);
        return result;
    }

    private void TryBindCollisionReporters(bool forceRebind = false)
    {
        if (!autoBindCollisionReporters || burnerRoot == null)
            return;

        int burnerId = burnerRoot.GetInstanceID();
        int turkaId = turka != null ? turka.GetInstanceID() : 0;

        if (!forceRebind && burnerId == _lastBurnerRootId && turkaId == _lastTurkaId)
            return;

        BindReportersUnder(burnerRoot);

        if (includeTurkaColliders && turka != null && !turka.IsChildOf(burnerRoot))
            BindReportersUnder(turka);

        _lastBurnerRootId = burnerId;
        _lastTurkaId = turkaId;
    }

    private void BindReportersUnder(Transform root)
    {
        Collider[] colliders = root.GetComponentsInChildren<Collider>(includeInactive: true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider col = colliders[i];
            if (col == null || col.isTrigger)
                continue;

            BurnerCollisionForceReporter reporter = col.GetComponent<BurnerCollisionForceReporter>();
            if (reporter == null)
                reporter = col.gameObject.AddComponent<BurnerCollisionForceReporter>();

            reporter.Bind(this);
        }
    }

    private static void AddMassPoint(Vector3 worldPosition, float mass, ref Vector3 weightedPosition, ref float totalMass)
    {
        float clampedMass = Mathf.Max(0f, mass);
        if (clampedMass <= 0f)
            return;

        weightedPosition += worldPosition * clampedMass;
        totalMass += clampedMass;
    }

    private void ApplyTilt()
    {
        Quaternion tilt = Quaternion.Euler(_currentTilt.x, 0f, -_currentTilt.y);
        burnerRoot.localRotation = _burnerInitialLocalRotation * tilt;

        if (!tiltTurkaVisual || turka == null || turka == burnerRoot)
            return;

        if (turkaBody == null)
            turka.TryGetComponent(out turkaBody);

        if (turkaBody != null && !turkaBody.isKinematic)
            return;

        Quaternion burnerDeltaWorld = burnerRoot.rotation * Quaternion.Inverse(_burnerInitialWorldRotation);
        turka.rotation = burnerDeltaWorld * _turkaInitialWorldRotation;
    }
}
