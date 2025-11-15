using System.Collections.Generic;
using UnityEngine;

public abstract class BaseSceneHandler : MonoBehaviour
{
    #region Base Template
    protected virtual void OnEnabled() { }
    protected virtual void OnAwake() { }
    protected virtual void OnStart() { }
    protected virtual void OnUpdate() { }
    protected virtual void OnFixedUpdate() { }
    protected virtual void OnLateUpdate() { }
    protected virtual void OnDestroyed() { }

    private void OnEnable()
    {
        OnEnabled();
    }

    private void Awake()
    {
        OnAwake();
    }

    private void Start()
    {
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

    private void LateUpdate()
    {
        OnLateUpdate();
    }

    private void OnDestroy()
    {
        OnDestroyed();   
    }

    #endregion

    public abstract void SceneInitialization();

}

