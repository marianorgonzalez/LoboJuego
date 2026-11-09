using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Componente para manejar movimiento y colisiones con RigidBodies kinematicos.
/// </summary>
public class KinematicMover : MonoBehaviour
{
    [SerializeField] Rigidbody2D rb2d;
    [SerializeField] Collider2D bodyCollider;
    /// <summary>
    /// La distancia minima a la que quedas de un objeto cuando chocas contra el.
    /// Si no existe una distancia minima, el redondeo de floats puede hacer que
    /// las fisicas flasheen que estas dentro del otro objeto.
    /// </summary>
    [SerializeField] float skinWidth = 0.01f;
    [SerializeField] int maxIterations = 3;
    [SerializeField] LayerMask blockingLayers;
    ContactFilter2D filter;
    List<RaycastHit2D> collisionResults;
    /// <summary>
    /// IMPORTANTE: ESTA LISTA SE INVALIDA SI VOLVES A LLAMAR MoveAndSlide
    /// </summary>
    List<Collider2D> objectsHit = new();
    const float MAX_SLOPE_ANGLE = 45f;
    static readonly float MAX_SLOPE_COSINE = Mathf.Cos(MAX_SLOPE_ANGLE * Mathf.Deg2Rad);

    void Awake()
    {
        filter = new ContactFilter2D();
        filter.SetLayerMask(blockingLayers);
        filter.useTriggers = false;
        collisionResults = new();
    }

    public struct MoveResult
    {
        public Vector2 requestedOffset, appliedOffset;
        /// <summary>
        /// IMPORTANTE: ESTA LISTA SE INVALIDA SI VOLVES A LLAMAR MoveAndSlide
        /// </summary>
        public List<Collider2D> objectsHit;
        public List<RaycastHit2D> hits;
        public bool Collided => objectsHit != null && objectsHit.Count > 0;
        public bool Grounded => hits.Any(x => Vector2.Dot(x.normal, Vector2.up) >= MAX_SLOPE_COSINE);
        public bool HitCeiling => hits.Any(x => Vector2.Dot(x.normal, Vector2.down) >= MAX_SLOPE_COSINE);
    }

    public MoveResult MoveAndSlide(Vector2 offset)
    {
        Vector2 position = rb2d.position;
        Vector2 remaining = offset;
        objectsHit.Clear();
        MoveResult result = new();
        result.hits = new();
        for (int i = 0; i < maxIterations && remaining != Vector2.zero; i++)
        {
            Vector2 moveDir = remaining.normalized;
            bodyCollider.Cast(position, rb2d.rotation, moveDir, filter,
             collisionResults, remaining.magnitude + skinWidth);
            RaycastHit2D? closestCollision = null;
            foreach (var collision in collisionResults)
            {
                // Ignoramos colisiones cuya normal apunta en la misma direccion que nos movemos.
                // Osea, si estas atascado dentro de un objecto, y te moves para salir, esto lo permite
                // sin que se cuente como colision.
                if (Vector2.Dot(moveDir, collision.normal) >= 0) 
                    continue;
                if (closestCollision is null || collision.distance < closestCollision.Value.distance)
                    closestCollision = collision;
            }
            if (closestCollision is null)
            {
                position += remaining;
                break;
            }
            result.hits.Add(closestCollision.Value);
            if (!objectsHit.Contains(closestCollision.Value.collider))
                objectsHit.Add(closestCollision.Value.collider);
            // Si la distancia es menor a skinWidth, entonces evita que sea negativa
            float normalDot = Mathf.Abs(Vector2.Dot(moveDir, closestCollision.Value.normal));
            float travel = Mathf.Max(0, closestCollision.Value.distance - skinWidth / normalDot);
            position += moveDir * travel;
            remaining = moveDir * (remaining.magnitude - travel);
            // Con la siguiente matematica mistica de vectores, haces que el movimiento que te haya quedado
            // se haga perpendicular a la superficie con la que chocaste, haciendo que te deslices sobre ella.
            remaining -= Vector2.Dot(remaining, closestCollision.Value.normal) * closestCollision.Value.normal;
        }
        result.objectsHit = objectsHit;
        result.requestedOffset = offset;
        result.appliedOffset = position - rb2d.position;
        rb2d.MovePosition(position);
        return result;
    }
}
