using UnityEngine;
using UnityEngine.InputSystem;


public class movmentToTestParalax : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }
    public Rigidbody2D rb;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
        {
            rb.AddForce(new Vector2(4f, 0f), ForceMode2D.Force);
        }

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
        {
            rb.AddForce(new Vector2(-4f, 0f), ForceMode2D.Force);
        }

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
        {
            rb.AddForce(new Vector2(0f, 4f), ForceMode2D.Force);
        }

        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
        {
            rb.AddForce(new Vector2(0f, -4f), ForceMode2D.Force);
        }

    }
}
