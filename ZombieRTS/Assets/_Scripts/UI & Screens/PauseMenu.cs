using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject PausePanel;
    public static bool isPaused = false;
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused) { PausePanel.SetActive(true); isPaused = true; }
            else { PausePanel.SetActive(false); isPaused = false; }
        }
    }
    public void Resume()
    {
        PausePanel.SetActive(false);
    }
    public void ReturnToMenu()
    {
        SceneManager.LoadScene(0);
    }
    public void Quit()
    {
        Application.Quit();
    }
}
