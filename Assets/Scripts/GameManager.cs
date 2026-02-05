using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        STANDARD
    };

    public GameState state = GameState.STANDARD;

    private PauseMenu pause;

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
            DontDestroyOnLoad(gameObject);
        }

        pause = GetComponent<PauseMenu>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TogglePause()
    {
        if (pause.isPaused)
            pause.ResumeGame();
        else
            pause.PauseGame();
    }

    public void LoadLevel(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitLevel()
    {
        Destroy(Camera.main.gameObject);
        Destroy(FindAnyObjectByType<TetherManager>().gameObject);
        PlayerController[] players = Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        for (int i = 0; i < players.Length; i++)
            Destroy(players[i].gameObject);
        Canvas[] can = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        for (int i = 0; i < can.Length; i++)
            Destroy(can[i].gameObject);
        SceneManager.LoadScene(0);
    }
}
