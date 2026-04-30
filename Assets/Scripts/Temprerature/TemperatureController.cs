using UnityEngine;
/// <summary>
/// скрипт кладется на gameobject любой как контроллер
/// </summary>
public class TemperatureController : MonoBehaviour
{
    [Header("Зона")]
    [Range(0f, 1f)] public float zoneMin = 0.5f;
    [Range(0f, 1f)] public float zoneMax = 0.8f;

    [Tooltip("сила нагрева")]
    public float heatSpeed = 0.4f;

    [Tooltip("остывание (пропорционально текущей температуре)")]
    public float coolSpeed = 0.009f;

    [Tooltip("дополнительное медленное остывание, когда огня нет")]
    public float idleCooling = 0.009f;

    [Tooltip("охлаждение водой")]
    public float waterCooling = 0f; // 0..1

    [Tooltip("сила охлаждения")]
    public float waterCoolSpeed = 0.5f;

    [Header("UI")]
    public RectTransform sliderTrack;   // фон слайдера
    public RectTransform needle;        // стрелка
    public RectTransform zoneRect;      // зелёная зона 
    
    
    public float Temperature => _temp; // это текущая температура
    public bool IsInZone => _temp >= zoneMin && _temp <= zoneMax; // это находится ли температура в зоне

    [HideInInspector] public float FireInput;   // 0..1

    private float _temp;

    private void Start()
    {
        UpdateZone();
    }

    private void Update()
    {
        float heating = FireInput * heatSpeed;
        float cooling = _temp * coolSpeed;

        // когда нагрев = 0 чтобы темпа падала
        if (FireInput <= 0f)
            cooling += idleCooling;
        // TODO : сделать охлаждение водой, да и в целом воду
        float delta = heating - cooling - waterCooling * waterCoolSpeed;

        _temp = Mathf.Clamp01(_temp + delta * Time.deltaTime);

        UpdateNeedle();
    }
    // движение стрелки, те обновление позиции
    private void UpdateNeedle()
    {
        if (needle == null || sliderTrack == null) return;
        float h = sliderTrack.rect.height;
        needle.anchoredPosition = new Vector2(needle.anchoredPosition.x, _temp * h);
    }

    // это метод чтобы менять зону
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
