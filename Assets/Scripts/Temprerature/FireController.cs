using UnityEngine;

/// <summary>
/// Скрипт кладется на префаб огня
/// </summary>
public class FireController : MonoBehaviour
{
    public TemperatureController temperatureController; 

    [Header("масштаб частиц")]
    public float minScale = 0.15f;
    public float maxScale = 1.5f;

    [Tooltip("множитель кол-ва партиклов при нулевой температуре")]
    public float minEmissionMultiplier = 0.05f;
    [Tooltip("множитель при максимальной температуре")]
    public float maxEmissionMultiplier = 1f;

    public float FirePower => _firePower;

    private float _firePower;
    private ParticleSystem[] _particles;
    private float[] _baseEmission;

    private void Awake()
    {
        _particles = GetComponentsInChildren<ParticleSystem>(true);
        _baseEmission = new float[_particles.Length];
        for (int i = 0; i < _particles.Length; i++)
            _baseEmission[i] = _particles[i].emission.rateOverTimeMultiplier;
    }

    private void Update()
    {
       
        if (temperatureController != null)
            _firePower = temperatureController.Temperature;

        ApplyToParticles();
    }

    private void ApplyToParticles()
    {
        float s = Mathf.Lerp(minScale, maxScale, _firePower);
        transform.localScale = Vector3.one * s;

        float emissionMultiplier = Mathf.Lerp(minEmissionMultiplier, maxEmissionMultiplier, _firePower);
        for (int i = 0; i < _particles.Length; i++)
        {
            var em = _particles[i].emission;
            em.rateOverTimeMultiplier = _baseEmission[i] * emissionMultiplier;
        }
    }
}