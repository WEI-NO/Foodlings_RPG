using CustomLibrary.References;
using System.Collections;
using UnityEngine;

public class ErrorDisplayManager : MonoBehaviour
{
    public static ErrorDisplayManager Instance;

    [Header("References")]
    public ErrorDisplay displayPrefab;
    public Transform content;

    private void Awake()
    {
        Initializer.SetInstance(this);
    }

    public static void ShowError(string message, float duration = 5.0f)
    {
        Instance.StartCoroutine(Instance.StartError(message, duration));
    }

    private IEnumerator StartError(string message, float duration)
    {
        var newDisplay = Instantiate(displayPrefab, content);
        newDisplay.Initialize(message);
        yield return new WaitForSeconds(duration);
        newDisplay.End();
    }
}
