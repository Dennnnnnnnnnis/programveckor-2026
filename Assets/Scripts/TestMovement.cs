using UnityEngine;

public class TestMovement : MonoBehaviour
{
    public float speed = 5f;

    Rigidbody2D rb;
    float moveX;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        
        moveX = Input.GetAxisRaw("Horizontal");
    }

    void FixedUpdate()
    {
        
        rb.linearVelocity = new Vector2(moveX * speed, rb.linearVelocity.y);
    }
}
