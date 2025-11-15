using CustomLibrary.References;
using JetBrains.Annotations;
using System.Collections;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance;
    public Animator anim;

    private AsyncOperation operation;
    private float extraWaitTime;
    public float animationWaitTime;
    public bool newHandle = false;
    public bool loading = false;

    private void Awake()
    {
        Initializer.SetInstance(this);
        anim = GetComponent<Animator>();
        StartCoroutine(StartLoadingHandle());
    }

    public void AddLoadingHandle(AsyncOperation operation)
    {
        loading = true;
        this.operation = operation;
        newHandle = true;
        StartCoroutine(StartLoadingHandle());
    }

    private IEnumerator StartLoadingHandle()
    {
        if (operation == null)
        {
            loading = false;
            newHandle = false;
            yield break;
        }

        anim.SetTrigger("Start");
        yield return new WaitForSeconds(animationWaitTime);

        while(!operation.isDone)
        {
            yield return null;
        }
        
        yield return new WaitForSeconds(extraWaitTime);

        anim.SetTrigger("End");
        loading = false;
    }

    public bool IsLoading()
    {
        return loading;
    }
}
