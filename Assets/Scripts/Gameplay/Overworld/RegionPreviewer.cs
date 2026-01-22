#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteAlways]
public class RegionPreviewer : MonoBehaviour
{
    //[Header("References")]
    //public RegionManager regionManager;

    //[Header("Preview Settings")]
    //public bool previewAllRegions = false;

    //[Tooltip("Optional parent for the spawned preview regions.")]
    //public Transform previewParent;

    //// Track spawned preview instances
    //private readonly List<GameObject> spawnedPreviews = new List<GameObject>();

    //// Used to detect changes in region setup (list + overworldLocation)
    //private int lastConfigHash = 0;

    //private void OnEnable()
    //{
    //    if (Application.isPlaying)
    //        return;

    //    if (!regionManager)
    //    {
    //        regionManager = RegionManager.Instance;
    //    }

    //    if (previewAllRegions)
    //    {
    //        RebuildPreview();
    //    }
    //}

    //private void OnDisable()
    //{
    //    if (Application.isPlaying)
    //        return;

    //    ClearPreview();
    //}

    //private void OnValidate()
    //{
    //    if (Application.isPlaying)
    //        return;

    //    if (!regionManager)
    //    {
    //        regionManager = RegionManager.Instance;
    //    }

    //    // Any inspector change on this component should refresh preview
    //    if (previewAllRegions)
    //    {
    //        RebuildPreview();
    //    }
    //    else
    //    {
    //        ClearPreview();
    //    }
    //}

    //private void Update()
    //{
    //    // Only run this “change detection” in editor, not in play mode
    //    if (Application.isPlaying || !previewAllRegions)
    //        return;

    //    if (!regionManager || regionManager.regionPrefabs == null)
    //        return;

    //    int currentHash = ComputeConfigHash();

    //    if (currentHash != lastConfigHash)
    //    {
    //        RebuildPreview();
    //    }
    //}

    ///// <summary>
    ///// Completely rebuilds preview instances based on current RegionManager setup.
    ///// </summary>
    //private void RebuildPreview()
    //{
    //    ClearPreview();

    //    if (!regionManager || regionManager.regionPrefabs == null)
    //    {
    //        Debug.LogWarning("[RegionPreviewer] No RegionManager or regionPrefabs set.");
    //        lastConfigHash = 0;
    //        return;
    //    }

    //    foreach (var lp in regionManager.regionPrefabs)
    //    {
    //        if (lp.regionPrefab == null)
    //            continue;

    //        var regionComponent = lp.regionPrefab.GetComponent<Region>();
    //        if (!regionComponent)
    //        {
    //            Debug.LogWarning($"[RegionPreviewer] regionPrefab '{lp.regionPrefab.name}' has no Region component.");
    //            continue;
    //        }

    //        Vector3 worldPos = new Vector3(
    //            regionComponent.overworldLocation.x,
    //            regionComponent.overworldLocation.y,
    //            0f
    //        );

    //        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(lp.regionPrefab);
    //        instance.transform.position = worldPos;

    //        if (previewParent != null)
    //        {
    //            instance.transform.SetParent(previewParent, true);
    //        }

    //        instance.name = $"RegionPreview_{lp.levelID}_{lp.regionPrefab.name}";
    //        instance.hideFlags = HideFlags.DontSave;

    //        spawnedPreviews.Add(instance);
    //    }

    //    lastConfigHash = ComputeConfigHash();
    //}

    ///// <summary>
    ///// Destroys all preview instances.
    ///// </summary>
    //private void ClearPreview()
    //{
    //    for (int i = spawnedPreviews.Count - 1; i >= 0; i--)
    //    {
    //        var go = spawnedPreviews[i];
    //        if (go != null)
    //        {
    //            // Use delayCall to avoid destroying objects while iterating editor events
    //            EditorApplication.delayCall += () =>
    //            {
    //                if (go != null)
    //                {
    //                    DestroyImmediate(go);
    //                }
    //            };
    //        }
    //    }

    //    spawnedPreviews.Clear();
    //}

    ///// <summary>
    ///// Builds a hash representing the current configuration of regionPrefabs
    ///// and their overworldLocation values, so we can detect changes.
    ///// </summary>
    //private int ComputeConfigHash()
    //{
    //    if (!regionManager || regionManager.regionPrefabs == null)
    //        return 0;

    //    unchecked
    //    {
    //        int hash = 17;
    //        hash = hash * 23 + regionManager.regionPrefabs.Count;

    //        for (int i = 0; i < regionManager.regionPrefabs.Count; i++)
    //        {
    //            var lp = regionManager.regionPrefabs[i];
    //            hash = hash * 23 + lp.levelID.GetHashCode();

    //            if (lp.regionPrefab != null)
    //            {
    //                var region = lp.regionPrefab.GetComponent<Region>();
    //                if (region != null)
    //                {
    //                    hash = hash * 23 + region.overworldLocation.x.GetHashCode();
    //                    hash = hash * 23 + region.overworldLocation.y.GetHashCode();
    //                }

    //                hash = hash * 23 + lp.regionPrefab.GetInstanceID();
    //            }
    //            else
    //            {
    //                hash = hash * 23 + 0;
    //            }
    //        }

    //        return hash;
    //    }
    //}
}
#endif
