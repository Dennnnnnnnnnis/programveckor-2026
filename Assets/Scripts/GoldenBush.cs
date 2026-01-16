using UnityEngine;

public class GoldenBush : MonoBehaviour
{
    GameManager gm;
    public string sceneName;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        gm = GameManager.Instance;
    }
}
