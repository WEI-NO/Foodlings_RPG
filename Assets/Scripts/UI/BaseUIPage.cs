using UnityEngine;

public class BaseUIPage : MonoBehaviour
{
    #region Base Class
    protected virtual void OnAwake() { }
    protected virtual void OnEnabled() { }
    protected virtual void OnStart() { }
    protected virtual void OnUpdate() { }
    protected virtual void OnFixedUpdate() { }
    protected virtual void OnDisabled() { }
    protected virtual void OnDestroyed() { }

    private void Awake()
    {
        anim = GetComponent<Animator>();
        OnAwake();
    }
    private void OnEnable()
    {
        OnEnabled();
    }
    private void Start()
    {
        if (closeOnStart)
        {
            DisableContent();
        }

        OnStart();
    }
    private void Update()
    {
        OnUpdate();
    }
    private void FixedUpdate()
    {
        OnFixedUpdate();
    }
    private void OnDisable()
    {
        OnDisabled();
    }
    private void OnDestroy()
    {
        OnDestroyed();
    }
    #endregion base class

    [Header("Components")]
    private Animator anim;

    [Header("Settings")]
    public int pageID = 0;
    public bool closeOnStart;
    public GameObject UIContainerGameObject;
    public bool currentState;
    public bool turnOnBlur;
    public bool hideMainHUD;

    [Header("Animation Settings")]
    public bool useAnimation;
    public string openTriggerName = "Open";
    public string closeTriggerName = "Close";

    // Disable content
    private void DisableContent()
    {
        if (UIContainerGameObject != null)
        {
            UIContainerGameObject.SetActive(false);
        }

        if (turnOnBlur)
        {
            BlurManager.Instance?.SetBlur(false);
        }

        if (hideMainHUD)
        {
            MainHUDController.Instance?.SetHidden(false);
        }
        OnContentDisabled();
    }

    // Overridable
    protected virtual void OnContentDisabled() { }

    // Enables content
    private void EnableContent()
    {
        if (UIContainerGameObject != null)
        {
            UIContainerGameObject.SetActive(true);
        }

        if (turnOnBlur)
        {
            BlurManager.Instance?.SetBlur(true);
        }

        if (hideMainHUD)
        {
            MainHUDController.Instance?.SetHidden(true);
        }
        OnContentEnabled();
    }

    // Overridable
    protected virtual void OnContentEnabled() { }

    // Able to set on and off
    public void SetActive(bool state)
    {
        currentState = state;

        if (useAnimation)
        {
            anim.SetTrigger(state ? openTriggerName : closeTriggerName);
            return;
        }

        if (currentState) EnableContent();
        else DisableContent();
    }

}
