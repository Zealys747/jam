using UnityEngine;

public class CoffeeState : MonoBehaviour
{
    public float beanPerCent;
    public float filledWaterPerCent;
    public float temperature;

    public CoffeeState()
    {
        beanPerCent = 0;
        filledWaterPerCent = 0;
        temperature = 0;
    }

    public void Compare(CoffeeState state, out float coef)
    {
        coef = 0;

        coef += Mathf.Abs(beanPerCent - state.beanPerCent);
        coef += Mathf.Abs(filledWaterPerCent - state.filledWaterPerCent);
        coef += Mathf.Abs(temperature - state.temperature);

        Debug.Log($"текущая погрешность равна: {coef}");
    }

    public void Info()
    {
        Debug.Log($"Бобы: {beanPerCent} Вода: {filledWaterPerCent} Температура: {temperature}");
    }
}
