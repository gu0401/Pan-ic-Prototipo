using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        AudioManager.Instance.PlaySFX(
        AudioManager.Instance.buttonSound
        );
        SceneManager.LoadScene("GameScene");
    }

    public void QuitGame()
    {
        AudioManager.Instance.PlaySFX(
        AudioManager.Instance.buttonSound
        );
        Application.Quit();
        Debug.Log("Saindo do jogo...");
    }
}