using UnityEngine;

public class TrashBin : MonoBehaviour
{
    public void AddItem(Item item)
    {
        if (item == null)
            return;

        Debug.Log("[Lixo] Jogando fora: " + item.itemName);

        AudioManager.Instance.PlaySFX(
        AudioManager.Instance.trashSound
        );
        Destroy(item.gameObject);
    }
}