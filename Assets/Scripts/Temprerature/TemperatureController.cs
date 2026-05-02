using UnityEngine;
using UnityEngine.InputSystem;

public class TemperatureController : MonoBehaviour
{
    [Header("Зона")]
    [Range(0f, 1f)] public float zoneMin = 0.5f;
    [Range(0f, 1f)] public float zoneMax = 0.8f;

    [Header("скролл")]
    public float scrollSensitivity = 0.08f;

    [Tooltip("насколько быстро гасится скорость, когда скролл остановился")]
    public float friction = 3.5f;


    public float maxSpeed = 1.2f;

    [Tooltip("скорость естественного остывания когда темпа выше нуля")]
    public float coolSpeed = 0.02f;

    public float waterCooling = 0f;

    public float waterCoolSpeed = 0.5f;

    [Header("UI")]
    public RectTransform sliderTrack;
    public RectTransform needle;
    public RectTransform zoneRect;

    public float Temperature => _temp;
    public bool IsInZone => _temp >= zoneMin && _temp <= zoneMax;

    [HideInInspector] public float FireInput;

    private float _temp;
    private float _tempVelocity;

    private void Start()
    {
        UpdateZone();
    }

    private void Update()
    {
        HandleScroll();
        ApplyCooling();
        UpdateTemperaturePhysics();
        UpdateNeedle();

        FireInput = _temp;
        
        Debug.Log(waterCooling);
    }

    private void HandleScroll()
    {
        if (Mouse.current == null) return;

        float scrollY = Mouse.current.scroll.ReadValue().y / 30f;

        if (Mathf.Abs(scrollY) > 0.001f)
        {
            // колесо добавляет импульс скорости
            _tempVelocity += scrollY * scrollSensitivity;
            _tempVelocity = Mathf.Clamp(_tempVelocity, -maxSpeed, maxSpeed);
        }
    }

    private void ApplyCooling()
    {
        _tempVelocity -= coolSpeed * Time.deltaTime;
        
        if (waterCooling >0f)
        {
            _temp -=waterCooling *  Time.deltaTime * waterCoolSpeed;
            _temp = Mathf.Clamp01(_temp);
        }
    }

    private void UpdateTemperaturePhysics()
    {
        // если игрок не крутит колесо, скорость постепенно падает
        float frictionFactor = Mathf.Exp(-friction * Time.deltaTime);
        _tempVelocity *= frictionFactor;

        _temp += _tempVelocity * Time.deltaTime;
        _temp = Mathf.Clamp01(_temp);

        // чтобы не упиралось в границы бесконечно
        if (_temp <= 0f && _tempVelocity < 0f) _tempVelocity = 0f;
        if (_temp >= 1f && _tempVelocity > 0f) _tempVelocity = 0f;
    }

    private void UpdateNeedle()
    {
        if (needle == null || sliderTrack == null) return;

        float h = sliderTrack.rect.height;
        needle.anchoredPosition = new Vector2(needle.anchoredPosition.x, _temp * h);
    }

    public void UpdateZone()
    {
        if (zoneRect == null) return;
        zoneRect.anchorMin = new Vector2(0f, zoneMin);
        zoneRect.anchorMax = new Vector2(1f, zoneMax);
        zoneRect.offsetMin = zoneRect.offsetMax = Vector2.zero;
    }

    private void OnValidate()
    {
        UpdateZone();
    }
}