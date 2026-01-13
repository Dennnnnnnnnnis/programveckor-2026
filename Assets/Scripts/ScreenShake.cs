using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    [Header("Shake Settings")]
    public float intensity = 0.1f;   // Hur stark skakningen är
    public float speed = 0.2f;       // Hur långsam/mjuk den är

    private float seedX;
    private float seedY;
    private Vector3 startLocalPos;

    void Start()
    {
        startLocalPos = transform.localPosition;
        seedX = Random.Range(0f, 100f);
        seedY = Random.Range(0f, 100f);
    }

    void LateUpdate()
    {
        float x = (Mathf.PerlinNoise(seedX, Time.time * speed) - 0.5f) * intensity;
        float y = (Mathf.PerlinNoise(seedY, Time.time * speed) - 0.5f) * intensity;

        transform.localPosition = new Vector3(
            startLocalPos.x + x,
            startLocalPos.y + y,
            startLocalPos.z
        );
    }
}
