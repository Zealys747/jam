using UnityEngine;
using UnityEngine.UI;

public class Turka : MonoBehaviour
{
    public GameObject waterMaterial;
    private Material material;

    private MaterialPropertyBlock propertyBlock;
    private Renderer targetRenderer;

    [Header("Текущие свойства в турке")]
    public float completion; // когда всё в норме - сюда считается процент, всего - 100
    public float currentCompletionSpeed; // скорость приготовления
    public float currentErrorCoef; // коэффициент ошибки
    public CoffeeState currentCoffeeState; // текущие условия приготовления
    public CoffeeState coffeeOrder;
    
    [Header("Ограничения")]
    public float minCompletionSpeed; // минимально возможная скорость готовки
    public float maxCompletionSpeed; // max скорость
    public float maxErrorCoef; // максимально возможная ошибка

    [Header("UI")]
    public Text beans;
    public Text water;
    public Text temp;


    void Start()
    {
        Renderer renderer = waterMaterial.GetComponent<Renderer>();
        if (renderer != null)
        {
            material = renderer.material;
        }
        else
        {
            Debug.LogError("На объекте waterMaterial нет Renderer компонента!");
        }

        currentCoffeeState = new CoffeeState();

    }

    private void Update()
    {
        currentCoffeeState.Info();
        //CheckCook()
        UpdateUI();
        UpdateWater();
    }
    private void UpdateUI()
    {
        beans.text = $"Бобы: \n{(int)(currentCoffeeState.beanPerCent * 100)}%";
        water.text = $"Вода: \n{(int)(currentCoffeeState.filledWaterPerCent * 100)}%";
        temp.text = $"Тепло: \n{(int)currentCoffeeState.temperature}°C";
    }
    public void UpdateWater()
    {
        /*// Вариант 1: Через MaterialPropertyBlock для Renderer
        if (targetRenderer != null)
        {
            propertyBlock.SetFloat("_Fill", currentCoffeeState.filledWaterPerCent);
            targetRenderer.SetPropertyBlock(propertyBlock);
        }*/

        material.SetFloat("_Fill", currentCoffeeState.filledWaterPerCent);
    }

    public void PourOut()
    {

    }

    private void FinishCook()
    {
        Debug.Log("Кофе приготовилось!");
        completion = 0;



        GetNewOrder();
    }
    private void GetNewOrder()
    {
        Debug.Log("Получаю новый заказ...");
    }
    private void CheckCook()
    {
        coffeeOrder.Compare(currentCoffeeState, out currentErrorCoef);

        if (currentErrorCoef > maxErrorCoef) // если больше погрешности - ничего не происходит
        {
            Debug.Log("Кофе не готовится...");
            return;
        }
        if (currentErrorCoef < maxErrorCoef)
        {
            currentCompletionSpeed = maxCompletionSpeed * ((maxErrorCoef - currentErrorCoef) / maxErrorCoef); // всегда <= 1

            if (currentCompletionSpeed < minCompletionSpeed)
            {
                currentCompletionSpeed = minCompletionSpeed;
            }

            completion += Time.deltaTime * currentCompletionSpeed;

            if (completion >= 100)
            {
                FinishCook();
            }
        }
    }
}
