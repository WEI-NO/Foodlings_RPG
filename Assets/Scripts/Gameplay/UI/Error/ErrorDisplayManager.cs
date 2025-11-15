using CustomLibrary.References;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErrorDisplayManager : MonoBehaviour
{
    public static ErrorDisplayManager Instance;

    [Header("References")]
    public ErrorDisplay displayPrefab;
    public Transform content;
    public Dictionary<string, ErrorDisplay> errorsOnScreen = new();

    private void Awake()
    {
        Initializer.SetInstance(this);
    }

    public static void ShowError(string message, float duration = 5.0f)
    {
        Instance.StartError(message, duration);
    }

    private void StartError(string message, float duration)
    {
        if (errorsOnScreen.TryGetValue(message, out var display))
        {
            display.IncrementCounter();
            display.ResetTimer();
        } else
        {
            var newDisplay = Instantiate(displayPrefab, content);
            newDisplay.Initialize(message, duration);
            newDisplay.OnEnd += OnEnd;
            errorsOnScreen.Add(message, newDisplay);
        }
    }

    private void OnEnd(string message)
    {
        errorsOnScreen.Remove(message);
    }
}
