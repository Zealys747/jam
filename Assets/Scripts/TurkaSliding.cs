using UnityEngine;

public class TurkaSliding : MonoBehaviour
{
    [Header("Скольжение турки")]
    [SerializeField] private bool allowTurkaSlide = true;
    [SerializeField] private float turkaSlideForce = 4f;
    [SerializeField] private float turkaSlideDamping = 2.2f;
    [SerializeField] private float turkaCenteringForce = 8f;
    [SerializeField] private float turkaMaxSlideRadius = 0.22f;

    private Transform _burnerRoot, _turka;
    private Rigidbody _rb;
    private Vector2 _basePlanarOffset;
    private float _baseNormalOffset;
    private Vector2 _slideOffset, _slideVel;
    private bool _ready;

    public void Init(Transform burnerRoot, Transform turka, Transform space)
    {
        _burnerRoot = burnerRoot;
        _turka = turka;
        _rb = turka != null ? turka.GetComponent<Rigidbody>() : null;
        _ready = false;
    }

    public void UpdateSlide(float dt, Vector2 currentTilt)
    {
        if (!allowTurkaSlide || _burnerRoot == null || _turka == null) return;

        if (!_ready)
        {
            Vector3 localOffset = ToUnscaledLocalOffset(_turka.position);
            _basePlanarOffset = new Vector2(localOffset.x, localOffset.z);
            _baseNormalOffset = localOffset.y;
            _slideOffset = Vector2.zero;
            _slideVel = Vector2.zero;
            _ready = true;
            return;
        }

        if (_rb != null && !_rb.isKinematic)
            ApplyWithRB(dt, currentTilt);
        else
            ApplyWithoutRB(dt, currentTilt);
    }

    private void ApplyWithoutRB(float dt, Vector2 tilt)
    {
        Vector2 acc = new Vector2(tilt.y, tilt.x) * turkaSlideForce;
        acc -= _slideVel * turkaSlideDamping;
        acc -= _slideOffset * turkaCenteringForce;

        _slideVel += acc * dt;
        _slideOffset += _slideVel * dt;

        if (turkaMaxSlideRadius > 0f && _slideOffset.sqrMagnitude > turkaMaxSlideRadius * turkaMaxSlideRadius)
            _slideOffset = _slideOffset.normalized * turkaMaxSlideRadius;

        _turka.position = ToWorldPosition(_slideOffset);
    }

    private void ApplyWithRB(float dt, Vector2 tilt)
    {
        Vector2 slide = new Vector2(tilt.y, tilt.x) * turkaSlideForce;
        Vector3 localVel = _burnerRoot.InverseTransformDirection(_rb.linearVelocity);
        Vector2 localVelXZ = new Vector2(localVel.x, localVel.z);

        Vector3 currentLocal = ToUnscaledLocalOffset(_turka.position);
        Vector2 currentOffset = new Vector2(currentLocal.x - _basePlanarOffset.x, currentLocal.z - _basePlanarOffset.y);

        Vector2 damping = -localVelXZ * turkaSlideDamping;
        Vector2 centering = -currentOffset * turkaCenteringForce;

        if (turkaMaxSlideRadius > 0f && currentOffset.sqrMagnitude > turkaMaxSlideRadius * turkaMaxSlideRadius)
            centering *= 2f;

        Vector2 totalXZ = slide + damping + centering;
        Vector3 forceWorld = _burnerRoot.TransformDirection(new Vector3(totalXZ.x, 0f, totalXZ.y));
        _rb.AddForce(forceWorld, ForceMode.Acceleration);

        KeepBodyOnBurnerPlane(currentOffset);
    }

    private void KeepBodyOnBurnerPlane(Vector2 currentOffset)
    {
        Vector2 clampedOffset = currentOffset;

        if (turkaMaxSlideRadius > 0f && clampedOffset.sqrMagnitude > turkaMaxSlideRadius * turkaMaxSlideRadius)
            clampedOffset = clampedOffset.normalized * turkaMaxSlideRadius;

        _rb.MovePosition(ToWorldPosition(clampedOffset));

        Vector3 velocityLocal = _burnerRoot.InverseTransformDirection(_rb.linearVelocity);
        velocityLocal.y = 0f;
        _rb.linearVelocity = _burnerRoot.TransformDirection(velocityLocal);
    }

    private Vector3 ToUnscaledLocalOffset(Vector3 worldPosition)
    {
        return _burnerRoot.InverseTransformDirection(worldPosition - _burnerRoot.position);
    }

    private Vector3 ToWorldPosition(Vector2 slideOffset)
    {
        Vector3 localOffset = new Vector3(
            _basePlanarOffset.x + slideOffset.x,
            _baseNormalOffset,
            _basePlanarOffset.y + slideOffset.y
        );

        return _burnerRoot.position + _burnerRoot.TransformDirection(localOffset);
    }
}
