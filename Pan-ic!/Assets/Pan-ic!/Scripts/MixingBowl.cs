using UnityEngine;

public class MixingBowl : MonoBehaviour
{
    [Header("Pontos dos ingredientes")]
    [SerializeField] private Transform ingredientPoint1;
    [SerializeField] private Transform ingredientPoint2;

    [Header("Resultado")]
    [SerializeField] private GameObject doughPrefab;
    [SerializeField] private Transform resultPoint;

    private Item ingredient1;
    private Item ingredient2;

    public bool HasIngredient()
    {
        return ingredient1 != null || ingredient2 != null;
    }

    public void AddIngredient(Item ingredient)
    {
        if (ingredient == null)
            return;

        // Primeiro ingrediente
        if (ingredient1 == null)
        {
            ingredient1 = ingredient;

            ingredient.transform.SetParent(ingredientPoint1);
            ingredient.transform.localPosition = Vector3.zero;

            Debug.Log("[Tigela] Primeiro ingrediente: " + ingredient.itemName);
            return;
        }

        // Segundo ingrediente
        if (ingredient2 == null)
        {
            ingredient2 = ingredient;

            ingredient.transform.SetParent(ingredientPoint2);
            ingredient.transform.localPosition = Vector3.zero;

            Debug.Log("[Tigela] Segundo ingrediente: " + ingredient.itemName);

            CheckRecipe();
            return;
        }

        Debug.Log("[Tigela] A tigela já está cheia!");
    }

    private void CheckRecipe()
    {
        if (ingredient1 == null || ingredient2 == null)
            return;

        Debug.Log(
            "[Tigela] Receita: " +
            ingredient1.itemType +
            " + " +
            ingredient2.itemType
        );

        bool correctRecipe =
            (ingredient1.itemType == Item.ItemType.Polvilho &&
             ingredient2.itemType == Item.ItemType.Queijo)
            ||
            (ingredient1.itemType == Item.ItemType.Queijo &&
             ingredient2.itemType == Item.ItemType.Polvilho);

        if (correctRecipe)
        {
            Debug.Log("[Tigela] RECEITA CORRETA! Criando massa...");
            MakeDough();
        }
        else
        {
            Debug.Log("[Tigela] RECEITA INCORRETA!");
        }
    }

    private void MakeDough()
    {
        Debug.Log("[Tigela] Receita correta! Criando massa...");

        // Guarda o ponto antes de destruir os ingredientes
        Vector3 spawnPosition = resultPoint.position;

        // Remove os ingredientes
        Destroy(ingredient1.gameObject);
        Destroy(ingredient2.gameObject);

        ingredient1 = null;
        ingredient2 = null;

        // Verifica se o prefab foi configurado
        if (doughPrefab == null)
        {
            Debug.LogError("[Tigela] O Dough Prefab NÃO foi configurado!");
            return;
        }

        // Verifica se o Result Point foi configurado
        if (resultPoint == null)
        {
            Debug.LogError("[Tigela] O Result Point NÃO foi configurado!");
            return;
        }

        // Cria a massa no Result Point
        GameObject newDough = Instantiate(
            doughPrefab,
            spawnPosition,
            Quaternion.identity
        );

        Debug.Log("[Tigela] Massa criada em: " + spawnPosition);
    }
}