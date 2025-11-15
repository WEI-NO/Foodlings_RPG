using TMPro;
using UnityEngine;

public class ErrorDisplay : MonoBehaviour
{
    [Header("Components")]
    public TextMeshProUGUI errorText;
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void Initialize(string message)
    {
        errorText.text = message;
    }

    public void End()
    {
        anim.SetTrigger("End");
    }
}
