using UnityEngine;

public class GameModeManager : MonoBehaviour
{
    public SaveManager saveManager;
    public static GameModeManager Instance;

    public enum GameMode
    {
        Play,
        Load
    }

    public GameMode currentMode = GameMode.Play;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}