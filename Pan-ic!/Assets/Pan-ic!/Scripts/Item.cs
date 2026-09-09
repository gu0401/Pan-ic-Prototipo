using UnityEngine;

public class Item : MonoBehaviour
{
    public enum ItemType
    {
        Nenhum,
        Polvilho,
        Queijo,
        Massa,
        Forma
    }

    [Header("Identificação do Item")]
    public string itemName = "Plate";

    [Header("Tipo do Item")]
    public ItemType itemType = ItemType.Nenhum;

    private Rigidbody2D rigidbody2D;
    private Collider2D collider2D;
    private Vector3 worldScale;

    private void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        collider2D = GetComponent<Collider2D>();

        worldScale = transform.lossyScale;
    }

    public void OnPickUp(Transform holdPoint)
    {
        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;

        if (holdPoint.lossyScale.x != 0 && holdPoint.lossyScale.y != 0)
        {
            transform.localScale = new Vector3(
                worldScale.x / holdPoint.lossyScale.x,
                worldScale.y / holdPoint.lossyScale.y,
                worldScale.z / holdPoint.lossyScale.z
            );
        }

        if (rigidbody2D != null)
            rigidbody2D.bodyType = RigidbodyType2D.Kinematic;

        if (collider2D != null)
            collider2D.enabled = false;
    }

    public void OnDrop(Vector3 dropPosition)
    {
        transform.SetParent(null);
        transform.position = dropPosition;

        transform.localScale = worldScale;

        if (rigidbody2D != null)
            rigidbody2D.bodyType = RigidbodyType2D.Kinematic;

        if (collider2D != null)
            collider2D.enabled = true;
    }
}