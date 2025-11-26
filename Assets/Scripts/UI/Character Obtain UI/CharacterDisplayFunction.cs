using UnityEngine;

public class CharacterDisplayFunction : MonoBehaviour
{
    public Animator anim;

    public string openAnimation = "Open";
    public string closeAnimation = "Close";

    public bool active = false;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        active = false;
    }

    protected virtual void OnEnabled()
    {

    }

    protected virtual void OnDisabled()
    {

    }

    public void Enable()
    {
        anim.SetTrigger(openAnimation);
        active = true;
        OnEnabled();
    }

    public void Disable()
    {
        anim.SetTrigger(closeAnimation);
        active = false;
        OnDisabled();
    }

    public void Toggle()
    {
        if (active)
        {
            Disable();
        } else
        {
            Enable();
        }
    }
}
