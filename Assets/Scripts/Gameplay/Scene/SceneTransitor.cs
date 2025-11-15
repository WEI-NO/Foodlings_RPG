using CustomLibrary.References;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class SceneTransitor : MonoBehaviour
{
    public static SceneTransitor Instance;

    [Header("Settings")]
    [Tooltip("The scene that will never be unloaded (e.g. holds global managers, UI, etc).")]
    public string persistentSceneName = "Persistent Scene";

    [Tooltip("Optional: assign a loading screen object (can be a canvas or prefab).")]
    public LoadingScreen loadingScreen;

    public bool isTransitioning = false;

    // 🟢 Event Actions
    public static Action<string> OnSceneTransitionStarted;
    public static Action<string> OnSceneTransitionCompleted;

    public float LoadInitialDelay = 2.0f;

    private void Awake()
    {
        Initializer.SetInstance(this);
        DontDestroyOnLoad(gameObject);

    }

    /// <summary>
    /// Call this to transition to another scene.
    /// Example: SceneTransitor.Instance.TransitionTo("Overworld");
    /// </summary>
    public void TransitionTo(string sceneName)
    {

        if (isTransitioning)
        {
            Debug.LogWarning("SceneTransitor: Transition already in progress.");
            return;
        }

        StartCoroutine(PerformTransition(sceneName));
    }

    private IEnumerator PerformTransition(string sceneName)
    {

        isTransitioning = true;
        OnSceneTransitionStarted?.Invoke(sceneName); // Notify subscribers that transition has begun

        EventSystem oldSystem = FindFirstObjectByType<EventSystem>();
        if (oldSystem) oldSystem.gameObject.SetActive(false);

        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        loadOp.allowSceneActivation = false;

        loadingScreen.AddLoadingHandle(loadOp);
        yield return new WaitForSeconds(LoadInitialDelay);

        while (loadOp.progress < 0.90f)
        {
            yield return null;
        }

        // --- Step 3: Activate new scene
        loadOp.allowSceneActivation = true;
        yield return new WaitUntil(() => loadOp.isDone);

        // --- Step 4: Unload other non-persistent scenes
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (s.name != persistentSceneName && s.name != sceneName)
            {
                AsyncOperation unload = SceneManager.UnloadSceneAsync(s);
                while (!unload.isDone)
                    yield return null;
            }
        }

        // --- Step 5: Set the new scene active
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));

        yield return new WaitForSeconds(1.0f);
        isTransitioning = false;
        Debug.Log($"[SceneTransitor] Transitioned to scene: {sceneName}");

        OnSceneTransitionCompleted?.Invoke(sceneName); // 🔔 Notify subscribers transition finished
    }
}
