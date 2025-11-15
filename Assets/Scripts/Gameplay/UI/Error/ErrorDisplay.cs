using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class ErrorDisplay : MonoBehaviour
{
    [Header("Components")]
    public TextMeshProUGUI errorText;
    private Animator anim;

    public float timer = 0.0f;
    public float duration;
    public int counter = 1;
    private string message;
    public Action<string> OnEnd;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        counter = 1;
    }

    public void Initialize(string message, float duration)
    {
        this.message = message;
        errorText.text = message;
        this.duration = duration;

        StartCoroutine(AutoClose());
    }

    private IEnumerator AutoClose()
    {
        while (timer <= duration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        End();
    }

    public void ResetTimer()
    {
        timer = 0.0f;
    }

    public void IncrementCounter()
    {
        counter++;
        errorText.text = $"{message} x{counter}";
    }

    public void End()
    {
        OnEnd?.Invoke(message);
        anim.SetTrigger("End");
    }
}
