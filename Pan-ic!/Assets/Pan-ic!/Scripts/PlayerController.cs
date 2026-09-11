using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Pontos de Interação")]
    public Transform holdPoint;

    [Header("Configurações de Interação")]
    [SerializeField] private float interactRadius = 1f;
    [SerializeField] private LayerMask interactableLayer;

    private Rigidbody2D rigidbody2D;
    private Collider2D collider2D;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveInput;
    private Item currentItem;

    private void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        collider2D = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnInteract(InputValue value)
    {
        if (!value.isPressed)
            return;

        print("Botão Interagir Pressionado!");

        // Recuperar o ingrediente tem prioridade quando a mao esta vazia.
        if (currentItem == null)
        {
            foreach (Collider2D nearby in Physics2D.OverlapCircleAll(transform.position, 1.5f))
            {
                SlimeThief slime = nearby.GetComponentInParent<SlimeThief>();
                if (slime != null && slime.TryRecover(this)) return;
            }
        }
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            holdPoint.position,
            interactRadius,
            interactableLayer
        );

        FridgeUI fridgeFound = null;
        MixingBowl bowlFound = null;
        BakingTray trayFound = null;
        Item itemFound = null;
        Oven ovenFound = null;
        Plate plateFound = null;
        TrashBin trashFound = null;

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject)
                continue;

            FridgeUI fridge = hit.GetComponent<FridgeUI>();

            if (fridge != null)
            {
                fridgeFound = fridge;
                break;
            }

            MixingBowl bowl = hit.GetComponent<MixingBowl>();

            if (bowl != null)
            {
                bowlFound = bowl;
            }

            BakingTray tray = hit.GetComponent<BakingTray>();

            if (tray != null)
            {
                trayFound = tray;
            }

            Item item = hit.GetComponent<Item>();

            if (item != null)
            {
                itemFound = item;
            }

            Oven oven = hit.GetComponent<Oven>();

            if (oven != null)
            {
                ovenFound = oven;
            }

            Plate plate = hit.GetComponent<Plate>();

            if (plate != null)
            {
                plateFound = plate;
            }

            TrashBin trash = hit.GetComponent<TrashBin>();

            if (trash != null)
            {
                trashFound = trash;
            }
        }

        // GELADEIRA
        if (fridgeFound != null)
        {
            fridgeFound.ToggleFridge();
            return;
        }

        // ==========================================
        // MÃO VAZIA
        // ==========================================

        if (currentItem == null)
        {
            if (itemFound != null)
            {
                print("Pegando: " + itemFound.itemName);

                currentItem = itemFound;
                currentItem.OnPickUp(holdPoint);

                return;
            }

            print("Nenhum item encontrado!");
            return;
        }

        // ==========================================
        // MASSA → FORMA
        // ==========================================

        if (trayFound != null &&
            currentItem.itemType == Item.ItemType.Massa)
        {
            if (!trayFound.HasDough())
            {
                trayFound.AddDough(currentItem);
                currentItem = null;

                print("Massa colocada na forma!");

                return;
            }
        }

        // ==========================================
        // INGREDIENTES → TIGELA
        // ==========================================

        if (bowlFound != null)
        {
            // A forma nunca pode entrar na tigela
            if (currentItem.itemType == Item.ItemType.Forma)
            {
                AudioManager.Instance.PlaySFX(
                AudioManager.Instance.errorSound
                );
                print("A forma não pode ser colocada na tigela!");
                return;
            }

            // A massa também não volta para a tigela
            if (currentItem.itemType == Item.ItemType.Massa)
            {
                AudioManager.Instance.PlaySFX(
                AudioManager.Instance.errorSound
                );
                print("Essa massa já está pronta para ir para a forma!");
                return;
            }

            // Só ingredientes podem entrar na tigela
            if (currentItem.itemType == Item.ItemType.Polvilho ||
                currentItem.itemType == Item.ItemType.Queijo)
            {
                AudioManager.Instance.PlaySFX(
                AudioManager.Instance.placeSound
                );
                print("Colocando " + currentItem.itemName + " na tigela...");

                bowlFound.AddIngredient(currentItem);
                currentItem = null;

                return;
            }

            print("Esse item não pode ser colocado na tigela!");
            return;
        }

        // ==========================================
        // FORMA → FORNO
        // ==========================================

        if (ovenFound != null &&
            currentItem != null &&
            currentItem.itemType == Item.ItemType.Forma)
        {
            BakingTray tray = currentItem.GetComponent<BakingTray>();

            if (tray != null)
            {
                print("Colocando a forma no forno...");

                ovenFound.AddTray(tray);

                currentItem = null;

                return;
            }
        }

        // ==========================================
        // PÃO DE QUEIJO → PRATO
        // ==========================================

        if (plateFound != null)
        {
            if (currentItem.itemType == Item.ItemType.PaoDeQueijo)
            {
                if (!plateFound.HasFood())
                {
                    print("Colocando pão de queijo no prato...");

                    plateFound.AddFood(currentItem);
                    currentItem = null;

                    return;
                }
            }

            print("O prato só aceita pão de queijo!");
            return;
        }

        // ==========================================
        // ITEM → LIXO
        // ==========================================

        if (trashFound != null && currentItem != null)
        {
            print("Jogando item no lixo...");

            trashFound.AddItem(currentItem);

            currentItem = null;

            return;
        }


        // ==========================================
        // SOLTAR ITEM
        // ==========================================

        print("Soltando item...");

        currentItem.OnDrop(holdPoint.position);
        currentItem = null;
    }

    private void FixedUpdate()
    {
        Vector2 targetPosition =
            rigidbody2D.position +
            moveInput * moveSpeed * Time.fixedDeltaTime;

        Camera mainCam = Camera.main;

        Vector3 minBounds =
            mainCam.ViewportToWorldPoint(new Vector3(0, 0, 0));

        Vector3 maxBounds =
            mainCam.ViewportToWorldPoint(new Vector3(1, 1, 0));

        float spriteHalfWidth = spriteRenderer.bounds.extents.x;
        float spriteHalfHeight = spriteRenderer.bounds.extents.y;

        targetPosition.x = Mathf.Clamp(
            targetPosition.x,
            minBounds.x + spriteHalfWidth,
            maxBounds.x - spriteHalfWidth
        );

        targetPosition.y = Mathf.Clamp(
            targetPosition.y,
            minBounds.y + spriteHalfHeight,
            maxBounds.y - spriteHalfHeight
        );

        rigidbody2D.MovePosition(targetPosition);
    }

    private void OnDrawGizmosSelected()
    {
        if (holdPoint != null)
        {
            Gizmos.color = Color.yellow;

            Gizmos.DrawWireSphere(
                holdPoint.position,
                interactRadius
            );
        }
    }

    // Transfere o objeto existente e libera a mao do jogador.
    public bool TryStealIngredient(Transform destination, out Item stolen)
    {
        stolen = null;
        if (destination == null || currentItem == null) return false;
        if (currentItem.itemType != Item.ItemType.Queijo &&
            currentItem.itemType != Item.ItemType.Polvilho) return false;
        stolen = currentItem;
        currentItem = null;
        stolen.OnPickUp(destination);
        return true;
    }
    public bool HasItemInHand()
    {
        return currentItem != null;
    }

    public void GiveItemToHand(Item newItem)
    {
        if (newItem == null)
            return;

        currentItem = newItem;
        currentItem.OnPickUp(holdPoint);
    }
}
