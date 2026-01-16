using UnityEngine;

public class CreditsText : MonoBehaviour
{
    [Header("Scroll")]
    public float scrollSpeed = 30f;

    [Header("Tilt")]
    public float tiltAngle = 20f;

    [Header("Start Position")]
    public float startYOffset = -600f;

    private RectTransform rect;

    void Start()
    {
        rect = GetComponent<RectTransform>();

        rect.anchoredPosition = new Vector2(0, startYOffset);

        

        rect.localScale = Vector3.one;
    }

    void Update()
    {
        rect.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;
    }
}
