using UnityEngine;

public class TurkaVisualSync : MonoBehaviour
{
    [Header("Визуал")]
    [SerializeField] private bool tiltTurkaVisual = true;

    private Transform _burnerRoot, _turka;
    private Rigidbody _rb;
    private Quaternion _initTurka, _initBurner;
    private bool _ready;
    private bool _syncEnabled;

    public void Init(Transform burnerRoot, Transform turka, bool syncEnabled)
    {
        _burnerRoot = burnerRoot;
        _turka = turka;
        _rb = turka != null ? turka.GetComponent<Rigidbody>() : null;
        _syncEnabled = syncEnabled;
        _ready = false;
    }

    public void UpdateVisual(Vector2 tilt)
    {
        if (!_syncEnabled || _burnerRoot == null || _turka == null || _turka == _burnerRoot) return;
        if (_rb != null && !_rb.isKinematic) return;

        if (!_ready)
        {
            _initTurka = _turka.rotation;
            _initBurner = _burnerRoot.rotation;
            _ready = true;
            return;
        }

        Quaternion delta = _burnerRoot.rotation * Quaternion.Inverse(_initBurner);
        _turka.rotation = delta * _initTurka;
    }
}
