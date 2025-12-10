using UnityEngine;

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance;

    public enum GameMode
    {
        Play,
        Load
    }

    public GameMode currentMode = GameMode.Play;

}