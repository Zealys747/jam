using UnityEngine;

public class BurnerTiltController : MonoBehaviour
{
    private Vector2 _currentTilt;
    private Vector2 _currentTargetTilt;
    private float _currentTiltTimer;

    [Tooltip("Сила, с которой игрой наклоняет выгорелку")]
    [SerializeField] private float _playerTiltStrength = 1f;

    [Tooltip("Сила, с которой выгорелка наконяется сама")]
    [SerializeField] private float _randomTiltStrength = 1f;

    [Tooltip("Объект выгорелки для наклонения")]
    [SerializeField] Transform _burnerRoot;
    [Tooltip("Объект турки для наклонения")]
    [SerializeField] Transform _turkiRoot;
    
    [Tooltip("Инвертировать наклон выгорелки вправо/влево")]
    [SerializeField] private bool _invertRightAxisTilt = false;

    [Tooltip("Инвертировать наклон выгорелки вперёд/назад")]
    [SerializeField] private bool _invertForwardAxisTilt = true;

    [Tooltip("Максимальное время, которое выгорелка может наклоняться в одну сторону")]
    [SerializeField] private Vector2 _tiltMaxTimer = new Vector2(5,7);

    [Tooltip("Максимальная сила наклона турки в секунду")]
    [SerializeField] private float _maxTurkiTilt = 2f;
    [Tooltip("Сила наклона турки")]
    [SerializeField] private float _turkiTiltStrength = 1f;
    [Tooltip("Инвертировать наклон турки вправо/влево")]
    [SerializeField] private bool _invertRightAxisTurkaTilt = false;

    [Tooltip("Инвертировать наклон турки вперёд/назад")]
    [SerializeField] private bool _invertForwardAxisTurkaTilt = false;

    [Tooltip("Визульный поворот турки")]
    [SerializeField] private float visualTurkiRotationTilt = 1f;


    [Tooltip("Максимальная граница, после которой турка падает")]
    [SerializeField] private float _maxTurkiOffset = 0.2f;


    [Tooltip("RigidBody турки")]
    [SerializeField] private Rigidbody _TurkiRigidbody;


    private bool _processVisuals = true;

    private float _TurkiOffsetProgress;
    private Quaternion _basicBurnerRotation;

    private Quaternion _basicTurkiRotation;
    private Vector3 _basicTurkiPosition;
    private Vector3 _turkiPositionOffset;

    void Awake()
    {
        _TurkiRigidbody.useGravity = false;
        _TurkiRigidbody.Sleep();
    }

    void Start()
    {
        _basicBurnerRotation = _burnerRoot.rotation;
        _basicTurkiRotation = _turkiRoot.rotation;
        _basicTurkiPosition = _turkiRoot.position;

        AssignRandomTiltAndTimer();
    }

    void Update()
    {
        Vector2 input = ProcessInput();
        ProcessTilt();
        if (_processVisuals ) ProcessVisual();

        _currentTilt += input * _playerTiltStrength * Time.deltaTime;

        Debug.Log(_TurkiOffsetProgress);
    }

    void ProcessTilt()
    {

        _currentTilt += _currentTargetTilt * _randomTiltStrength * Time.deltaTime;
        _currentTiltTimer -= Time.deltaTime;

        var turkiOffsetTarget = Vector2.ClampMagnitude(_currentTilt, _maxTurkiTilt) / 100 * _turkiTiltStrength;
        if (_invertRightAxisTurkaTilt) turkiOffsetTarget.x *= -1;
        if (_invertForwardAxisTurkaTilt) turkiOffsetTarget.y *= -1;

        _turkiPositionOffset += new Vector3(turkiOffsetTarget.x, 0f, turkiOffsetTarget.y);

        _TurkiOffsetProgress = _turkiPositionOffset.sqrMagnitude / _maxTurkiOffset;

        if (_turkiPositionOffset.sqrMagnitude > _maxTurkiOffset)
        {
            ProcessTurkiFall();
        }

        if (_currentTiltTimer <= 0)
        {
            AssignRandomTiltAndTimer();
            return;
        }
    }
    
    void ProcessTurkiFall()
    {
        _processVisuals = false;
        _TurkiRigidbody.useGravity = true;
        _TurkiRigidbody.WakeUp();
    }

    void ProcessVisual()
    {
        _burnerRoot.rotation = Quaternion.Euler(_basicBurnerRotation.eulerAngles + new Vector3(_currentTilt.x, 0, _currentTilt.y));

        _turkiRoot.rotation = Quaternion.Euler(_basicTurkiRotation.eulerAngles + new Vector3(_turkiPositionOffset.x, 0, _turkiPositionOffset.y) * visualTurkiRotationTilt);
        _turkiRoot.position = _basicTurkiPosition + _turkiPositionOffset;
    }

    Vector2 ProcessInput()
    {
        float x = 0;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            x = 1;
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            x = -1;
        }

        float y = 0;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            y = 1;
        }
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            y = -1;
        }

        if (_invertRightAxisTilt)
        {
            x *= -1;
        }

        if (_invertForwardAxisTilt)
        {
            y *= -1;
        }

        return new Vector2(x, y);
    }

    void AssignRandomTiltAndTimer()
    {
        _currentTargetTilt = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        _currentTiltTimer = Random.Range(_tiltMaxTimer.x, _tiltMaxTimer.y);
        Debug.Log($"Assigned Tilt: {_currentTargetTilt}");
    }
}
