using UnityEngine;

public class BakingTray : MonoBehaviour
{
    [Header("Ponto da massa")]
    [SerializeField] private Transform doughPoint;

    private Item dough;

    public bool HasDough()
    {
        return dough != null;
    }

    public void AddDough(Item newDough)
    {
        if (newDough == null)
            return;

        if (dough != null)
        {
            Debug.Log("[Forma] A forma já possui massa!");
            return;
        }

        dough = newDough;

        dough.transform.SetParent(doughPoint);
        dough.transform.localPosition = Vector3.zero;

        Debug.Log("[Forma] Massa colocada na forma!");
    }
}