using UnityEngine;

public class Oven : MonoBehaviour
{
    [Header("Configurações")]
    [SerializeField] private Transform trayPoint;
    [SerializeField] private GameObject bakedBreadPrefab;
    [SerializeField] private float cookingTime = 3f;

    private BakingTray currentTray;
    private bool isCooking = false;

    public bool HasTray()
    {
        return currentTray != null;
    }

    public void AddTray(BakingTray tray)
    {
        if (tray == null)
            return;

        if (currentTray != null)
        {
            Debug.Log("[Forno] Já existe uma forma no forno!");
            return;
        }

        if (!tray.HasDough())
        {
            Debug.Log("[Forno] A forma está sem massa!");
            return;
        }

        currentTray = tray;

        tray.transform.SetParent(trayPoint);
        tray.transform.localPosition = Vector3.zero;

        AudioManager.Instance.PlaySFX(
        AudioManager.Instance.placeSound
        );
        Debug.Log("[Forno] Forma colocada no forno!");

        StartCoroutine(Bake());
    }

    private System.Collections.IEnumerator Bake()
    {
        isCooking = true;

        Debug.Log("[Forno] Assando...");

        yield return new WaitForSeconds(cookingTime);

        Debug.Log("[Forno] Pão de queijo pronto!");
        AudioManager.Instance.PlaySFX(
        AudioManager.Instance.ovenSound
        );
        if (currentTray != null)
        {
            Item dough = currentTray.GetDough();

            if (dough != null)
            {
                Destroy(dough.gameObject);
            }

            currentTray.RemoveDough();
        }

        if (bakedBreadPrefab != null && currentTray != null)
        {
            Instantiate(
                bakedBreadPrefab,
                currentTray.transform.position,
                Quaternion.identity
            );
        }

        isCooking = false;
    }
}