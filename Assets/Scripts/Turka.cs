using UnityEngine;

public class Turka : MonoBehaviour
{
    [SerializeField] private Material material; // его менять буду, потомушо шейдер там для заполнения

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

    void Start()
    {
        currentCoffeeState = new CoffeeState();
    }

    private void Update()
    {
        currentCoffeeState.Info();
        //CheckCook()
    }
    
    public void FillTurka()
    {
        /*if (filledWaterPerCent > 1)
        {
            filledWaterPerCent = 1;
        }*/
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
