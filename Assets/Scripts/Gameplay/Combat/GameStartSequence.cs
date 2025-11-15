using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class GameStartSequence : MonoBehaviour
{
    Animator anim;
    public TextMeshProUGUI titleText;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private async void Start()
    {
        GameMatchManager.Instance.SetGameState(false);

        while (GameMatchManager.Instance.currentLevel == null || LoadingScreen.Instance == null || LoadingScreen.Instance.IsLoading())
        {
            await Task.Yield();
        }

        anim.SetTrigger("Start");
        titleText.text = GameMatchManager.Instance.currentLevel.LevelName;
    }

    public void StartGame()
    {
        GameMatchManager.Instance.SetGameState(true);
    }

}
