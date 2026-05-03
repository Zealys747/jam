using UnityEngine;
using UnityEngine.InputSystem;

public class VaseLift : MonoBehaviour
{
    public Transform pointUp;
    public Transform pointDown;
    public float moveSpeed = 5f;
    public float rotateSpeed = 5f;
    public GameObject waterParticles;

    [Header("Задержка партиклов")]
    public float particleDelay = 0.4f;
    
    public WaterCoolingReciever waterCoolingReciever;
    
    public TemperatureController temperatureController; // чтобы удобно выводить и контролить темпу + с воды ыххы
    
    
    private bool _isLifted = false;
    private Mouse _mouse;
    private float _liftTimer = 0f;
    private bool _particlesActive = false;

    void Start()
    {
        _mouse = Mouse.current;

        if (pointDown != null)
        {
            transform.position = pointDown.position;
            transform.rotation = pointDown.rotation;
        }

        if (waterParticles != null)
            waterParticles.SetActive(false);
    }

    void Update()
    {
        if (_mouse == null) return;

        if (_mouse.rightButton.wasPressedThisFrame)
        {
            _isLifted = true;
            _liftTimer = 0f; 
        }

        if (_mouse.rightButton.wasReleasedThisFrame)
        {
            _isLifted = false;
            _liftTimer = 0f;

            if (waterParticles != null && _particlesActive)
            {
                
                waterParticles.SetActive(false);
                _particlesActive = false;
            }
            waterCoolingReciever?.StopPouring();
        }

        if (_isLifted && !_particlesActive)
        {
            _liftTimer += Time.deltaTime;

            if (_liftTimer >= particleDelay)
            {
                if (waterParticles != null)
                    waterParticles.SetActive(true);
                _particlesActive = true;
                
                waterCoolingReciever?.StartPouring();
            }
        }

        Transform target = _isLifted ? pointUp : pointDown;

        if (target != null)
        {
            transform.position = Vector3.Lerp(transform.position, target.position, moveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, rotateSpeed * Time.deltaTime);
        }
    }
}