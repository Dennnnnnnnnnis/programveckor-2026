using UnityEngine;

[DefaultExecutionOrder(-1)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        STANDARD
    };

    public GameState state = GameState.STANDARD;

    [HideInInspector] public Inputs input;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        input = new Inputs();
        input.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
