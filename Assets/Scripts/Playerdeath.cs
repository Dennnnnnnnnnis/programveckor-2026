using UnityEngine;

public class Playerdeath : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Death"))
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Player died");
        gameObject.SetActive(false); // eller restart
    }
}
