using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{

    public void PlayGame(int level)
    {
        SceneManager.LoadSceneAsync($"Level{level}");
        UIManager.Instance.musicSource.Stop();
        UIManager.Instance.PlayMusic("Level");
    }
    public void GoTemplate()
    {
        SceneManager.LoadSceneAsync("Templatelvl");
        UIManager.Instance.musicSource.Stop();
        UIManager.Instance.PlayMusic("Level");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
