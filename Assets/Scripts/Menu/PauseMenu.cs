using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    [SerializeField] protected GameObject pauseScreen = default;
    [SerializeField] protected SettingsMenu settings = default;

    void Start()
    {
        Pause();
    }

    protected virtual void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && PlayerHealth.playerDead == false)
        {
            if(GameIsPaused)
            {
                settings.SaveSettings();
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public virtual void Resume()
    {
        pauseScreen.SetActive(false);
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        GameIsPaused = false;
        AudioManager.instance.UnPause("Music");
    }

    public void Pause()
    {
        pauseScreen.SetActive(true);
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        GameIsPaused = true;
        AudioManager.instance.Pause("Music");
    }

    public void Quit()
    {
        settings.SaveSettings();
        Application.Quit();
    }
}
