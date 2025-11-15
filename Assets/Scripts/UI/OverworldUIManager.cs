using System.Collections.Generic;
using UnityEngine;

public class OverworldUIManager : MonoBehaviour
{
    [Header("Components")]
    public Dictionary<int, BaseUIPage> pages = new();

    private void Awake()
    {
        foreach (Transform t in transform)
        {
            if (t == null) continue;
            var page = t.GetComponent<BaseUIPage>();
            if (page == null) continue;

            pages.Add(page.pageID, page);
        }
    }

    public void OpenPageID(int id)
    {
        if (pages.TryGetValue(id, out var page)) 
        {
            page.SetActive(true);
        }
    }

    public void ClosePageID(int id)
    {
        if (pages.TryGetValue(id, out var page))
        {
            page.SetActive(false);
        }
    }
}
