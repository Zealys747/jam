using UnityEngine;

public class WaterCoolingReciever : MonoBehaviour
{
    public TemperatureController temperatureController;

    [Range(0f, 1f)] public float waterLevel = 0f; // текущий уровень воды, сюдаа
    public float fillRate = 0.2f; // скорость наполнения в сек
    
    public float coolingWhilePouring = 1f;
    private bool _isPouring = false;

    private void Update()
    {
        
        if (_isPouring)
            waterLevel = Mathf.Clamp01(waterLevel + fillRate * Time.deltaTime);

        
        if (temperatureController != null)
            temperatureController.waterCooling = _isPouring ? coolingWhilePouring : 0f;
    }

    public void StartPouring() =>  _isPouring = true;
    public void StopPouring() =>  _isPouring = false;
    
}
