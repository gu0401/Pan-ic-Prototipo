using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class SlimeThief : MonoBehaviour
{
    [Header("Referencias da cena")]
    [SerializeField] private PlayerController player;
    [SerializeField] private Transform lootPoint;
    [SerializeField] private Transform escapePoint;
    [Tooltip("Somente paredes, mesas e bancadas. Exclua jogador, slime e ingredientes.")]
    [SerializeField] private LayerMask obstacles;
    [Tooltip("Objetos vazios no chao livre, nas esquinas e passagens da cozinha.")]
    [SerializeField] private Transform[] waypoints = new Transform[0];

    [Header("Ajustes")]
    [SerializeField, Min(0.1f)] private float speed = 2.5f;
    [SerializeField, Min(0.1f)] private float escapeSpeed = 3f;
    [SerializeField, Min(0.1f)] private float detectionRange = 8f;
    [SerializeField, Min(0.1f)] private float stealRange = 1.1f;
    [SerializeField, Min(0.1f)] private float recoveryRange = 1.5f;
    [SerializeField, Min(0.1f)] private float cooldown = 3f;

    private Rigidbody2D body;
    private CircleCollider2D shape;
    private Item stolenItem;
    private float readyAt;
    private float nextPathAt;
    private readonly List<Vector2> route = new List<Vector2>();
    private float Radius => shape.radius * Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y)) + 0.03f;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        shape = GetComponent<CircleCollider2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        shape.isTrigger = true;
        if (player == null || lootPoint == null || escapePoint == null || obstacles.value == 0)
        {
            Debug.LogError("[Slime] Preencha Player, Loot Point, Escape Point e Obstacles no Inspector.", this);
            enabled = false;
        }
    }

    private void FixedUpdate()
    {
        if (player == null || Time.time < readyAt) return;
        bool fleeing = stolenItem != null;
        Vector2 target = fleeing ? (Vector2)escapePoint.position : (Vector2)player.transform.position;
        float distance = Vector2.Distance(body.position, target);

        if (!fleeing && distance > detectionRange) { route.Clear(); return; }
        if (!fleeing && distance <= stealRange && ClearLine(body.position, target))
        {
            readyAt = Time.time + 0.4f;
            if (player.TryStealIngredient(lootPoint, out stolenItem))
            {
                route.Clear();
                nextPathAt = 0;
                Debug.Log("[Slime] Roubou " + stolenItem.itemName);
            }
            return;
        }
        if (fleeing && distance <= 0.15f)
        {
            // No prototipo, deixa o ingrediente no refugio em vez de destrui-lo.
            DropLoot();
            readyAt = Time.time + cooldown;
            return;
        }
        if (Time.time >= nextPathAt)
        {
            BuildRoute(target);
            nextPathAt = Time.time + 0.35f;
        }
        if (route.Count == 0) return;
        Vector2 next = Vector2.MoveTowards(body.position, route[0],
            (fleeing ? escapeSpeed : speed) * Time.fixedDeltaTime);
        if (ClearPath(body.position, next)) body.MovePosition(next);
        else nextPathAt = 0;
        if (Vector2.Distance(next, route[0]) < 0.02f) route.RemoveAt(0);
    }

    public bool TryRecover(PlayerController requester)
    {
        if (!enabled || requester != player || stolenItem == null || requester.HasItemInHand()) return false;
        if (Vector2.Distance(body.position, requester.transform.position) > recoveryRange ||
            !ClearLine(body.position, requester.transform.position)) return false;
        Item recovered = stolenItem;
        stolenItem = null;
        requester.GiveItemToHand(recovered);
        readyAt = Time.time + cooldown;
        route.Clear();
        nextPathAt = 0;
        Debug.Log("[Slime] Ingrediente recuperado!");
        return true;
    }

    private bool ClearLine(Vector2 a, Vector2 b)
    {
        return Physics2D.Linecast(a, b, obstacles).collider == null;
    }

    private bool ClearPath(Vector2 a, Vector2 b)
    {
        Vector2 delta = b - a;
        if (Physics2D.OverlapCircle(a, Radius, obstacles) != null) return false;
        return Physics2D.CircleCast(a, Radius, delta.normalized, delta.magnitude, obstacles).collider == null;
    }

    // Grafo pequeno: conecta pontos que possuem passagem livre para o corpo inteiro.
    // A busca em largura encontra uma rota; nao exige pacote de navegacao.
    private void BuildRoute(Vector2 target)
    {
        route.Clear();
        var points = new List<Vector2> { body.position };
        foreach (Transform point in waypoints)
            if (point != null) points.Add(point.position);
        points.Add(target);
        int goal = points.Count - 1;
        var previous = new int[points.Count];
        for (int i = 0; i < previous.Length; i++) previous[i] = -1;
        previous[0] = 0;
        var queue = new Queue<int>();
        queue.Enqueue(0);
        while (queue.Count > 0 && previous[goal] == -1)
        {
            int from = queue.Dequeue();
            for (int to = 1; to < points.Count; to++)
            {
                if (previous[to] != -1 || !ClearPath(points[from], points[to])) continue;
                previous[to] = from;
                queue.Enqueue(to);
            }
        }
        if (previous[goal] == -1) return;
        for (int node = goal; node != 0; node = previous[node]) route.Add(points[node]);
        route.Reverse();
    }

    private void DropLoot()
    {
        if (stolenItem == null) return;
        stolenItem.OnDrop(transform.position);
        stolenItem = null;
        route.Clear();
        nextPathAt = 0;
    }

    private void OnDisable() { DropLoot(); }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stealRange);
        Gizmos.color = Color.cyan;
        foreach (Transform point in waypoints)
            if (point != null) Gizmos.DrawWireSphere(point.position, 0.12f);
    }
}
