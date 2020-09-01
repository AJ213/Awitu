using UnityEngine;
using UnityEngine.SceneManagement;
public class DeathScreen : PauseMenu
{
    private void Start()
    {
        
    }
    protected override void Update()
    {
        if(PlayerHealth.playerDead)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Quit();
            }
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            AudioManager.instance.Stop("Music");
            Resume();
            SceneManager.LoadScene(0);
            AudioManager.instance.Play("Music");
        }    
        
    }
}
