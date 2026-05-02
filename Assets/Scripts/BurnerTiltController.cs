using UnityEngine;

[RequireComponent(typeof(BurnerLoadCalculator))]
[RequireComponent(typeof(BurnerTiltPhysics))]
[RequireComponent(typeof(BurnerPlayerStabilizer))]
[RequireComponent(typeof(BurnerImpulseHandler))]
public class BurnerTiltController : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private Transform burnerRoot;
    [SerializeField] private Transform turka;
    [SerializeField] private BurnerLoadCalculator loadCalc;
    [SerializeField] private BurnerTiltPhysics tiltPhysics;
    [SerializeField] private BurnerPlayerStabilizer stabilizer;
    [SerializeField] private BurnerImpulseHandler impulseHandler;
    [SerializeField] private TurkaSliding turkaSliding;
    [SerializeField] private TurkaVisualSync turkaVisualSync;

    [Header("Настройки")]
    [SerializeField] private string burnerObjectName = "Burner";
    [SerializeField] private string turkaObjectName = "TURKI4";
    [SerializeField] private bool autoAddMissingComponents = true;
    [SerializeField] private float maxTiltAngle = 14f;
    [SerializeField] private bool calibrateCenterAtStart = true;
    [SerializeField] private bool tiltTurkaVisual = true;

    private Transform _space;
    private bool _isReady;

    private void Awake()
    {
        ResolveReferences();
        InitComponents();
    }

    private void LateUpdate()
    {
        if (!_isReady || burnerRoot == null || tiltPhysics == null) return;
        if (Time.deltaTime <= 0f) return;

        Vector2 target = Vector2.zero;
        if (loadCalc != null) target += loadCalc.CalculateTilt();
        if (stabilizer != null) target += stabilizer.CalculateTilt(Time.deltaTime);
        if (impulseHandler != null) target += impulseHandler.GetAndDecay(Time.deltaTime);
        target = ClampTilt(target);

        Vector2 result = tiltPhysics.Step(target, Time.deltaTime);
        result = ClampTilt(result);

        ApplyRotation(result);
        turkaSliding?.UpdateSlide(Time.deltaTime, result);
        turkaVisualSync?.UpdateVisual(result);
    }

    private void ResolveReferences()
    {
        if (burnerRoot == null)
            burnerRoot = FindNamedTransform(burnerObjectName);

        if (burnerRoot == null)
            burnerRoot = transform;

        _space = burnerRoot.parent != null ? burnerRoot.parent : burnerRoot;

        if (turka == null)
            turka = FindNamedTransform(turkaObjectName);

        if (loadCalc == null) loadCalc = GetOrAddComponent<BurnerLoadCalculator>();
        if (tiltPhysics == null) tiltPhysics = GetOrAddComponent<BurnerTiltPhysics>();
        if (stabilizer == null) stabilizer = GetOrAddComponent<BurnerPlayerStabilizer>();
        if (impulseHandler == null) impulseHandler = GetOrAddComponent<BurnerImpulseHandler>();

        if (turka != null)
        {
            if (turkaSliding == null) turkaSliding = GetOrAddComponent<TurkaSliding>(turka.gameObject);
            if (turkaVisualSync == null) turkaVisualSync = GetOrAddComponent<TurkaVisualSync>(turka.gameObject);
        }
    }

    private void InitComponents()
    {
        loadCalc?.Init(burnerRoot, turka, calibrateCenterAtStart, _space);
        tiltPhysics?.Init(burnerRoot);
        stabilizer?.Init();
        impulseHandler?.Init(this, burnerRoot, turka, tiltTurkaVisual);
        turkaSliding?.Init(burnerRoot, turka, _space);
        turkaVisualSync?.Init(burnerRoot, turka, tiltTurkaVisual);
        _isReady = burnerRoot != null && tiltPhysics != null;
    }

    private void ApplyRotation(Vector2 tilt)
    {
        Quaternion q = Quaternion.Euler(tilt.x, 0f, tilt.y);
        burnerRoot.localRotation = tiltPhysics.InitialLocalRotation * q;
    }

    private Vector2 ClampTilt(Vector2 v) => Vector2.ClampMagnitude(v, maxTiltAngle);

    public void RegisterCollisionImpulse(Vector3 worldImpulse) => impulseHandler?.AddImpulse(worldImpulse);

    private T GetOrAddComponent<T>() where T : Component
    {
        return GetOrAddComponent<T>(gameObject);
    }

    private T GetOrAddComponent<T>(GameObject target) where T : Component
    {
        if (target == null) return null;

        T component = target.GetComponent<T>();
        if (component == null && autoAddMissingComponents)
            component = target.AddComponent<T>();

        return component;
    }

    private static Transform FindNamedTransform(string objectName)
    {
        if (string.IsNullOrWhiteSpace(objectName)) return null;

        Transform firstMatch = null;
        Transform[] transforms = FindObjectsByType<Transform>(FindObjectsSortMode.None);
        foreach (Transform candidate in transforms)
        {
            if (candidate == null || candidate.name != objectName) continue;

            firstMatch ??= candidate;
            if (candidate.parent == null)
                return candidate;
        }

        return firstMatch;
    }
}
