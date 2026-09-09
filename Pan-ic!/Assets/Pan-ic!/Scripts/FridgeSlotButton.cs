using UnityEngine;

public class FridgeSlotButton : MonoBehaviour
{
    [Header("Configurações do Ingrediente")]
    [Tooltip("Prefab do ingrediente que ESTE botão entrega.")]
    [SerializeField] private GameObject ingredientPrefab;

    [Header("Referência ao Player")]
    [Tooltip("Arraste o Player da Hierarchy para cá.")]
    [SerializeField] private PlayerController player;

    public void OnClickSelectIngredient()
    {
        Debug.Log("[Geladeira] Botão clicado: " + gameObject.name);

        // Verifica se o Player foi configurado
        if (player == null)
        {
            Debug.LogWarning("[Geladeira] Player não foi configurado neste botão!");
            return;
        }

        // Verifica se o prefab foi configurado
        if (ingredientPrefab == null)
        {
            Debug.LogWarning(
                "[Geladeira] Nenhum Ingredient Prefab foi configurado no botão "
                + gameObject.name
            );
            return;
        }

        // O jogador só pode carregar um ingrediente por vez
        if (player.HasItemInHand())
        {
            Debug.Log("[Geladeira] O jogador já está segurando um item!");
            return;
        }

        // Cria o ingrediente correspondente ao botão clicado
        GameObject newItemObj = Instantiate(ingredientPrefab);

        // Procura o componente Item no prefab
        Item itemScript = newItemObj.GetComponent<Item>();

        if (itemScript == null)
        {
            Debug.LogWarning(
                "[Geladeira] O prefab " +
                ingredientPrefab.name +
                " não possui o componente Item!"
            );

            Destroy(newItemObj);
            return;
        }

        // Coloca o ingrediente diretamente na mão
        player.GiveItemToHand(itemScript);

        Debug.Log(
            "[Geladeira] Ingrediente escolhido: " +
            itemScript.itemName
        );
    }
}
