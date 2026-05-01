using UnityEngine;

public class WaterController : MonoBehaviour
{
    [Header("Water Fill")]
    [Range(0f, 1f)]
    [SerializeField] private float fill01 = 0f;

    [Tooltip("Максимальная масса воды при полном заполнении (кг).")]
    [SerializeField] private float maxWaterMass = 0.45f;

    public float Fill01 => fill01;
    public float CurrentMass => Mathf.Clamp01(fill01) * Mathf.Max(0f, maxWaterMass);

    public void SetFill(float normalizedFill)
    {
        fill01 = Mathf.Clamp01(normalizedFill);
    }

    public void AddFill(float normalizedDelta)
    {
        SetFill(fill01 + normalizedDelta);
    }
}
