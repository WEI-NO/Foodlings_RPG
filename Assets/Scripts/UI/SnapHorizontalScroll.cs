using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class SnapHorizontalScroll : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    [Header("Snapping")]
    [Tooltip("How fast the scroll view snaps to the nearest item.")]
    public float snapSpeed = 10f;

    private ScrollRect scrollRect;
    private RectTransform content;

    private bool isDragging = false;

    // Normalized positions for each page (0..1)
    public float[] pagePositions;
    public int pageCount;
    private float targetPosition;       // target horizontalNormalizedPosition
    private int currentPageIndex = 0;   // tracked page index

    private void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        content = scrollRect.content;

        // Optional but recommended so inertia doesn't fight the snapping:
        scrollRect.inertia = false;

        //SetupPages();
    }

    private async void OnEnable()
    {
        await Task.Yield();
        SetupPages();
    }

    /// <summary>
    /// Precompute normalized positions for each page.
    /// </summary>
    public void SetupPages()
    {
        if (content == null)
            return;

        pageCount = content.childCount;

        print("Setting up:");
        foreach (Transform i in content)
        {
            print(i.name);
        }


        if (pageCount <= 0)
        {
            pagePositions = null;
            currentPageIndex = 0;
            return;
        }

        pagePositions = new float[pageCount];

        if (pageCount == 1)
        {
            pagePositions[0] = 0f;
        }
        else
        {
            // Evenly distribute from 0 to 1
            for (int i = 0; i < pageCount; i++)
            {
                pagePositions[i] = (float)i / (pageCount - 1);
            }
        }

        // Initialize to whatever position the ScrollRect currently has
        float current = scrollRect.horizontalNormalizedPosition;
        currentPageIndex = FindClosestPageIndex(current);
        targetPosition = pagePositions[currentPageIndex];
    }

    private void Update()
    {
        if (pagePositions == null || pageCount == 0)
            return;

        if (isDragging)
            return;

        // Smoothly move towards the target normalized position
        float current = scrollRect.horizontalNormalizedPosition;
        float newPos = Mathf.Lerp(current, targetPosition, Time.deltaTime * snapSpeed);
        scrollRect.horizontalNormalizedPosition = newPos;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        if (pagePositions == null || pageCount == 0)
            return;

        float current = scrollRect.horizontalNormalizedPosition;
        currentPageIndex = FindClosestPageIndex(current);
        targetPosition = pagePositions[currentPageIndex];
    }

    /// <summary>
    /// Find the index of the page whose normalized position is closest
    /// to the given value.
    /// </summary>
    private int FindClosestPageIndex(float position)
    {
        int closestIndex = 0;
        float smallestDistance = Mathf.Abs(position - pagePositions[0]);

        for (int i = 1; i < pageCount; i++)
        {
            float distance = Mathf.Abs(position - pagePositions[i]);
            if (distance < smallestDistance)
            {
                smallestDistance = distance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    // -----------------------------------------------------------
    // Public functions for button-based panning
    // -----------------------------------------------------------

    /// <summary>
    /// Pan one page to the right (next page), if possible.
    /// Intended to be called from a UI Button.
    /// </summary>
    public void PanRightOnePage()
    {
        if (pagePositions == null || pageCount == 0)
            return;

        if (!CanPanRight())
            return;

        currentPageIndex = Mathf.Clamp(currentPageIndex + 1, 0, pageCount - 1);
        targetPosition = pagePositions[currentPageIndex];
        isDragging = false;
    }

    /// <summary>
    /// Pan one page to the left (previous page), if possible.
    /// Intended to be called from a UI Button.
    /// </summary>
    public void PanLeftOnePage()
    {
        if (pagePositions == null || pageCount == 0)
            return;

        if (!CanPanLeft())
            return;

        currentPageIndex = Mathf.Clamp(currentPageIndex - 1, 0, pageCount - 1);
        targetPosition = pagePositions[currentPageIndex];
        isDragging = false;
    }

    /// <summary>
    /// Returns true if there is a page to the right of the current one.
    /// </summary>
    public bool CanPanRight()
    {
        return pagePositions != null &&
               pageCount > 0 &&
               currentPageIndex < pageCount - 1;
    }

    /// <summary>
    /// Returns true if there is a page to the left of the current one.
    /// </summary>
    public bool CanPanLeft()
    {
        return pagePositions != null &&
               pageCount > 0 &&
               currentPageIndex > 0;
    }
}
