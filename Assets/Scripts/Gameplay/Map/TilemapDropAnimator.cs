using CustomLibrary.References;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapDropAnimator : MonoBehaviour
{
    public static TilemapDropAnimator Instance;

    [Header("References")]
    public Tilemap tilemap;

    [Header("Animation")]
    [Tooltip("How high above the cell (in world units) the tile starts falling from.")]
    public float dropHeight = 2f;
    [Tooltip("Time it takes for a tile to settle.")]
    public float dropDuration = 0.25f;
    [Tooltip("Motion curve (0→1). Use an ease-out bounce for a nice thunk.")]
    public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Tooltip("Optional overshoot/bounce distance at impact (0 = none).")]
    public float impactBounce = 0.08f;

    private readonly HashSet<Vector3Int> animating = new HashSet<Vector3Int>();

    private void Awake()
    {
        Initializer.SetInstance(this);
    }

    /// <summary>
    /// Place a tile and animate it dropping into its cell.
    /// </summary>
    public void PlaceTileWithDrop(Vector3Int cell, TileBase tile)
    {
        if (!tilemap) return;

        tilemap.SetTile(cell, tile);
        tilemap.SetTileFlags(cell, TileFlags.None);

        var start = Matrix4x4.TRS(new Vector3(0, dropHeight, 0), Quaternion.identity, Vector3.one);
        tilemap.SetTransformMatrix(cell, start);

        if (!animating.Contains(cell))
            StartCoroutine(AnimateDrop(cell));
    }

    /// <summary>
    /// Convenience: place a list with a small stagger delay (nice cascading effect).
    /// </summary>
    public void PlaceTilesWithDropStagger(IEnumerable<(Vector3Int cell, TileBase tile)> items, float perTileDelay = 0.03f)
    {
        StartCoroutine(Co_Stagger(items, perTileDelay));
    }

    private IEnumerator Co_Stagger(IEnumerable<(Vector3Int cell, TileBase tile)> items, float delay)
    {
        foreach (var (cell, tile) in items)
        {
            PlaceTileWithDrop(cell, tile);
            if (delay > 0) yield return new WaitForSeconds(delay);
        }
    }

    /// <summary>
    /// Instantly places tiles without any animation.
    /// </summary>
    public void PlaceTilesInstant(IEnumerable<(Vector3Int cell, TileBase tile)> items)
    {
        if (!tilemap) return;

        foreach (var (cell, tile) in items)
        {
            tilemap.SetTile(cell, tile);
            tilemap.SetTileFlags(cell, TileFlags.None);
            tilemap.SetTransformMatrix(cell, Matrix4x4.identity);
            tilemap.SetColor(cell, Color.white);
        }
    }

    private IEnumerator AnimateDrop(Vector3Int cell)
    {
        animating.Add(cell);

        float t = 0f;
        float dur = Mathf.Max(0.0001f, dropDuration);
        float startY = dropHeight;
        float bounce = Mathf.Max(0f, impactBounce);

        // Fade from 0 → 1
        tilemap.SetColor(cell, new Color(1, 1, 1, 0));

        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            float u = Mathf.Clamp01(t);

            // Drop motion
            float y = Mathf.Lerp(startY, 0f, curve.Evaluate(u));
            if (bounce > 0f && u > 0.9f)
            {
                float k = (u - 0.9f) / 0.1f;
                y += Mathf.Sin(k * Mathf.PI) * bounce * (1f - k);
            }

            // Fade opacity (ease in)
            float alpha = Mathf.SmoothStep(0f, 1f, u);
            tilemap.SetColor(cell, new Color(1, 1, 1, alpha));

            // Apply transform
            var m = Matrix4x4.TRS(new Vector3(0, y, 0), Quaternion.identity, Vector3.one);
            tilemap.SetTransformMatrix(cell, m);

            yield return null;
        }

        // Finalize
        tilemap.SetTransformMatrix(cell, Matrix4x4.identity);
        tilemap.SetColor(cell, Color.white);
        animating.Remove(cell);
    }
}
