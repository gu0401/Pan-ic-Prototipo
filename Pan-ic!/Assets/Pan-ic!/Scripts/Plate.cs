using UnityEngine;

public class Plate : MonoBehaviour
{
    [Header("Ponto onde a comida ficará")]
    [SerializeField] private Transform foodPoint;

    private Item food;

    public bool HasFood()
    {
        return food != null;
    }

    public void AddFood(Item newFood)
    {
        if (newFood == null)
            return;

        if (food != null)
        {
            Debug.Log("[Prato] O prato já possui comida!");
            return;
        }

        // Só aceita pão de queijo
        if (newFood.itemType != Item.ItemType.PaoDeQueijo)
        {
            Debug.Log("[Prato] Esse item não pode ser colocado no prato!");
            return;
        }

        food = newFood;

        food.transform.SetParent(foodPoint);
        food.transform.localPosition = Vector3.zero;

        Debug.Log("[Prato] Pão de queijo colocado no prato!");
    }
}