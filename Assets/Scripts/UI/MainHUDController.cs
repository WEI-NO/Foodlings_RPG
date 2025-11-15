using CustomLibrary.References;
using UnityEngine;

public class MainHUDController : MonoBehaviour
{
    public static MainHUDController Instance;

    private Animator anim;

    private void Awake()
    {
        Initializer.SetInstance(this);

        anim = GetComponent<Animator>();
    }

    public void SetHidden(bool hidden)
    {
        if (hidden)
        {
            anim.SetTrigger("Hide");
            anim.ResetTrigger("Show");
        }
        else
        {
            anim.SetTrigger("Show");
            anim.ResetTrigger("Hide");
        }
    }
}
