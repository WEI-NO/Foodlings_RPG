using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    
    public void StartGame()
    {
        SceneTransitor.Instance.TransitionTo(SceneType.GameScene);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
