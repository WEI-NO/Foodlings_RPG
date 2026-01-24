using UnityEngine;

public class GameResultUI : MonoBehaviour
{
    public Animator winScreenAnim;
    public Animator loseScreenAnim;

    private void Start()
    {
        GameMatchManager.Instance.OnFriendlyWin += Win;
        GameMatchManager.Instance.OnEnemyWin += Lose;

        winScreenAnim.gameObject.SetActive(true);
        loseScreenAnim.gameObject.SetActive(true);
    }

    private void Win()
    {
        winScreenAnim.SetTrigger("Start");
    }

    private void Lose()
    {
        loseScreenAnim.SetTrigger("Start");
    }

    public void ReturnToOverworld()
    {
        SceneTransitor.Instance.TransitionTo(SceneType.GameScene);
    }
}
