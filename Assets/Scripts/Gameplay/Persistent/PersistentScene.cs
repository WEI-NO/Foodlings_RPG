using UnityEngine;
using UnityEngine.SceneManagement;

public static class PersistentScene
{
    // The name of the scene to ensure is loaded.
    private const string RequiredSceneName = "Persistent Scene"; // change to your target scene name
    private const bool LoadAdditively = true;

    // Called automatically before the first scene starts.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureSceneLoaded()
    {
        bool sceneLoaded = false;

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (s.name == RequiredSceneName)
            {
                sceneLoaded = true;
                break;
            }
        }

        if (!sceneLoaded)
        {
            Debug.Log($"[SceneAutoLoader] Scene '{RequiredSceneName}' not loaded. Loading now...");
            LoadSceneMode mode = LoadAdditively ? LoadSceneMode.Additive : LoadSceneMode.Single;
            SceneManager.LoadSceneAsync(RequiredSceneName, mode);
        }
        else
        {
            Debug.Log($"[SceneAutoLoader] Scene '{RequiredSceneName}' already loaded.");
        }
    }
}
