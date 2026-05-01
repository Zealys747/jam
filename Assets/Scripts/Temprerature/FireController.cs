using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// скрипт кладется на префаб огня
/// </summary>

public class FireController : MonoBehaviour
{
    public TemperatureController temperatureController;

    [Header("управление колесиком")]
    [Tooltip("насколько сильно колесо меняет мощность огня")]
    public float scrollSensitivity = 0.15f;
    [Tooltip("насколько быстро мощность огня плавно меняется")]
    public float decaySpeed = 1.5f;

    [Header("масштаб частиц чтобы еще прикольнее было")]
    public float minScale = 0.15f;
    public float maxScale = 1.5f;

   
    [Tooltip("множитель колва партиклов при нулевой мощности")]
    public float minEmissionMultiplier = 0.05f;

    [Tooltip("множитель при полной мощности")]
    public float maxEmissionMultiplier = 1f;
    
    
    public float displayEpsilon = 0.02f;
    
    public float FirePower => _firePower; // текущая мощность 

    private float _firePower;
    private float _targetFirePower;

    private ParticleSystem[] _particles;
    private float[] _baseEmission;

    private void Awake()
    {
        _particles = GetComponentsInChildren<ParticleSystem>(true);
        _baseEmission = new float[_particles.Length];

        // тут я типо сохраняю базовую скорость эмисии, чтобы на нее потом умножать
        for (int i = 0; i < _particles.Length; i++)
            _baseEmission[i] = _particles[i].emission.rateOverTimeMultiplier;
    }

    private void Update()
    {
        float scrollY = ReadScrollInput();
        // скролл - меняет мощность
        if (Mathf.Abs(scrollY) > 0.001f)
            _targetFirePower = Mathf.Clamp01(_targetFirePower + scrollY * scrollSensitivity);
        //  движение текущей мощности к целевой
        _firePower = Mathf.MoveTowards(_firePower, _targetFirePower, decaySpeed * Time.deltaTime);

        ApplyToParticles();
        // передача в контроллер
        if (temperatureController != null)
            temperatureController.FireInput = _firePower;
    }
    
    // чтение колеса
    private float ReadScrollInput()
    {
        if (Mouse.current == null)
            return 0f;

        // 120 одно деление для скролла
        return Mouse.current.scroll.ReadValue().y / 30f;
    }
    
    // логика изменения вида огня
    private void ApplyToParticles()
    {
        // тут я масштаб объекта меняю вместе с огнем
        float s = Mathf.Lerp(minScale, maxScale, _firePower);
        transform.localScale = Vector3.one * s;
        // тут эмиссия частиц зависит теперь от мощности
        float emissionMultiplier = Mathf.Lerp(minEmissionMultiplier, maxEmissionMultiplier, _firePower);
        // тут по сути новое кол-во частиц это базовое * силу огня
        for (int i = 0; i < _particles.Length; i++)
        {
            var em = _particles[i].emission;
            em.rateOverTimeMultiplier = _baseEmission[i] * emissionMultiplier;
        }
    }
}