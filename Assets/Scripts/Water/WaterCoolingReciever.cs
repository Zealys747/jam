using UnityEngine;

public class WaterCoolingReciever : MonoBehaviour
{
    public TemperatureController temperatureController;

    public float coolingPerParticle = 0.1f;
    public float maxCooling = 2f;
    private float _coolingThisFrame = 0f;

    private void Update()
    {
        if (temperatureController != null)
        {
            temperatureController.waterCooling = _coolingThisFrame;
        }

        if (_coolingThisFrame > 0)
        {
            Debug.Log("ОООООО ЕБАТЬ ЛЬЕТСЯ ВОДИЦА");
        }
        _coolingThisFrame = 0f;
        
    }

    private void OnParticleCollision(GameObject other)
    {
        _coolingThisFrame = Mathf.Min(_coolingThisFrame + coolingPerParticle, maxCooling);
    }
    
}
