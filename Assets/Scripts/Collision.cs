using UnityEngine;

public class Collision : MonoBehaviour
{
    [SerializeField] private LayerMask whatIsCollision;
    private float bounceFactor = 0f;
    public float weight = 1f;

    [Header("Checks")]
    [SerializeField] private Vector2 collisionSize = new Vector2(1f, 1f);
    public Vector2 collisionOffset = Vector2.zero;
    [SerializeField] private Vector2Int collisionPrec = new Vector2Int(3, 3);
    [SerializeField] private Vector2 collisionEdges = new Vector2(0.1f, 0.1f);

    private Vector2 velocity = new Vector2();
    private bool isGrounded = false;

    // Properties
    public Vector2 Velocity { get { return velocity; } set { velocity = value; } }
    public float BounceFactor { get { return bounceFactor; } set { bounceFactor = value; } }
    public bool IsGrounded { get { return isGrounded; } set { isGrounded = value; } }

    public float OffsetY { get { return collisionOffset.y; } }

    public void Awake()
    {
        if(TryGetComponent<BoxCollider2D>(out BoxCollider2D col))
        {
            col.size = collisionSize;
            col.offset = collisionOffset;
        }
    }

    public void Collide()
    {
        isGrounded = false;

        Vector2 curPos = transform.position;
        Vector2 curVel = velocity * Time.fixedDeltaTime; // Move amount this update
        Vector2 velDir = curVel.normalized;
        float velMag = curVel.magnitude;

        // I am the best
        for (int i = 0; i < 4; i++)
            CheckSide(velDir, velMag, curPos, (i >= 2), (i % 2) * 2f - 1f);

        transform.position += (Vector3)velocity * Time.fixedDeltaTime;
    }

    void CheckSide(Vector2 velDir, float velMag, Vector2 curPos, bool horizontal, float multiplier = 1.0f)
    {
        RaycastHit2D colRes;
        bool collision = false;

        // Maybe a bit redundant but better 2 copy pastes instead of 4 ones
        if (horizontal)
        {
            collision = BoxCast(collisionOffset + curPos, collisionSize.y - collisionEdges.y * 2f, collisionSize.x / 2f, Vector2.right * multiplier, Vector2.down * multiplier, collisionPrec.y, whatIsCollision, out colRes, true);
            if (!collision && velDir.x * multiplier > 0f)
                collision = BoxCast(collisionOffset + curPos + (collisionSize.x / 2f * Vector2.right * multiplier), collisionSize.y - collisionEdges.y * 2f, velMag, velDir, Vector2.down * multiplier, collisionPrec.y, whatIsCollision, out colRes, true);

            if (collision)
            {
                transform.position = new Vector2(colRes.point.x - (collisionOffset.x - collisionSize.x * -multiplier / 2f), transform.position.y);
                if (velocity.x * multiplier >= 0)
                    velocity = new Vector2(-velocity.x * bounceFactor, velocity.y);
            }
        }
        else
        {
            collision = BoxCast(collisionOffset + curPos, collisionSize.x - collisionEdges.x * 2f, collisionSize.y / 2f, Vector2.up * multiplier, Vector2.right * multiplier, collisionPrec.x, whatIsCollision, out colRes, true);
            if (!collision && velDir.y * multiplier > 0f)
                collision = BoxCast(collisionOffset + curPos + (collisionSize.y / 2f * Vector2.up * multiplier), collisionSize.x - collisionEdges.x * 2f, velMag, velDir, Vector2.right * multiplier, collisionPrec.x, whatIsCollision, out colRes, true);

            if (collision)
            {
                transform.position = new Vector2(transform.position.x, colRes.point.y - (collisionOffset.y - collisionSize.y * -multiplier / 2f));
                if (velocity.y * multiplier >= 0)
                    velocity = new Vector2(velocity.x, -velocity.y * bounceFactor);

                if (multiplier < 0f)
                    isGrounded = true;
            }
        }

    }

    bool BoxCast(Vector2 origin, float width, float distance, Vector2 direction, Vector2 originDirection, int rayCount, LayerMask collisionMask, out RaycastHit2D result, bool visualize = false) // Good for skewed checks
    {
        result = new RaycastHit2D();
        bool hasHit = false;
        float closestHitDistance = Mathf.Infinity;

        if (visualize)
            Debug.DrawRay(origin - originDirection * (width / 2), originDirection * width, Color.white);

        // Calculate spacing between rays
        float spacing = width / (rayCount - 1f);

        for (int i = 0; i < rayCount; i++)
        {
            // Calculate start position for each ray
            Vector2 rayOrigin = origin + originDirection * (i * spacing - width / 2);

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, direction, distance, collisionMask);
            if (hit.collider != null)
            {
                if (visualize)
                    Debug.DrawRay(rayOrigin, direction * hit.distance, Color.red);

                if (hit.distance < closestHitDistance)
                {
                    closestHitDistance = hit.distance;
                    result = hit;
                    hasHit = true;
                }
            }
            else if (visualize)
            {
                Debug.DrawRay(rayOrigin, direction * distance, Color.green);
            }
        }

        return hasHit;
    }

    public void ChangeHitboxY(float height, float offset, float edge)
    {
        // Because in platformer games you tend to want to change the height of the player

        // Very unsafe way to make the collider stick to the ground
        transform.position -= Vector3.up * (collisionOffset.y - offset);

        // Change the stuff
        collisionSize.y = height;
        collisionOffset.y = offset;
        collisionEdges.y = edge;

        if (TryGetComponent<BoxCollider2D>(out BoxCollider2D col))
        {
            col.size = collisionSize;
            col.offset = collisionOffset;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(new Vector3(collisionOffset.x, collisionOffset.y, 0) + transform.position, new Vector3(collisionSize.x, collisionSize.y, 0.01f));
    }
}
