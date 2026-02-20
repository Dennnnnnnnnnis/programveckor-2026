using UnityEngine;
using UnityEngine.UIElements;

public class Parallax : MonoBehaviour
{
    private Vector2 length, startpos;
    private Transform cam;
    public float parallaxEffect;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = Camera.main.transform;
        startpos = transform.position;
        length = GetComponent<SpriteRenderer>().bounds.size;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 dist = (cam.position * parallaxEffect);

        float temp = cam.position.x * (1 - parallaxEffect);

        if (temp > startpos.x + length.x)
        {
            startpos += length * Vector2.right;
        }
        else if (temp < startpos.x - length.x)
        {
            startpos -= length * Vector2.right;
        }

        transform.position = new Vector3(startpos.x + dist.x, startpos.y + dist.y, transform.position.z);
    }
}
