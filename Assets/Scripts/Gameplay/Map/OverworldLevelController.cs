using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;   // ✅ Needed to detect UI hover

[RequireComponent(typeof(Collider2D))]
public class OverworldLevelController : MonoBehaviour
{
    [Header("Components")]
    public Animator anim;
    public TextMeshProUGUI levelText;

    [Header("Visual Feedback")]
    private SpriteRenderer sprite;
    private bool isHovered = false;

    [Header("Level Settings")]
    public Level level;
    public bool unlockRegionUponCompletion = false;
    public List<int> unlockRegionIndex = new();
    public int regionIndex = 0;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        var region = GetComponentInParent<Region>();
        level = region != null ? region.assignedLevel : null;
    }

    private void Start()
    {
        if (levelText && level) levelText.text = level.LevelName;
    }

    // ===== Helper =====
    /// <summary>
    /// Returns true if pointer/touch is over any UI element.
    /// </summary>
    private bool PointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

        // Mouse
        if (EventSystem.current.IsPointerOverGameObject())
            return true;

        // Touch
        for (int i = 0; i < Input.touchCount; i++)
        {
            if (EventSystem.current.IsPointerOverGameObject(Input.touches[i].fingerId))
                return true;
        }
        return false;
    }

    // ===== Mouse Events =====
    private void OnMouseEnter()
    {
        if (PointerOverUI()) return;  
        isHovered = true;
        anim?.SetBool("Hover", true);
    }

    private void OnMouseOver()
    {
        // If pointer moved over UI, undo hover state
        if (PointerOverUI())
        {
            if (isHovered)
            {
                isHovered = false;
                anim?.SetBool("Hover", false);
            }
        }
    }

    private void OnMouseExit()
    {
        if (PointerOverUI()) return;   // exit is safe, but harmless to skip
        isHovered = false;
        anim?.SetBool("Hover", false);
    }

    private void OnMouseDown()
    {
        if (PointerOverUI()) return;
        anim?.ResetTrigger("Release");
        anim?.SetTrigger("Click");

    }


    private void OnMouseUp()
    {
        if (PointerOverUI()) return; 
        anim?.SetTrigger("Release");

        // Only trigger game logic when not over UI
        if (!PlayerParty.Instance.HasTeamMembers())
        {
            ErrorDisplayManager.ShowError("Please assign at least one foodling to your party!");
            return;
        }

        GameMatchManager.SetLevel(level, unlockRegionUponCompletion, unlockRegionIndex);
        OverworldData.Instance.inprogressLevelIndex = regionIndex;
        SceneTransitor.Instance.TransitionTo("Game Scene");
    }
}
