using CustomLibrary.References;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public enum LevelStoneState
{
    Default,
    Highlight,
    Pressed,
    Release
}

[RequireComponent(typeof(Collider2D))]
public class LevelStone : MonoBehaviour
{
    protected bool isHovered;
    protected bool isPressed;
    protected Collider2D col;

    [Header("UI References")]
    public Animator speechbubbleAnim;
    public TextMeshProUGUI chapterText;
    public TextMeshProUGUI levelNameText;
    public TextMeshProUGUI enemyLevelText;

    [Header("World View")]
    public Sprite[] spriteStages;
    public SpriteRenderer sr;

    public LevelDefinition levelDef;

    protected virtual void Awake()
    {
        col = GetComponent<Collider2D>();
    }

    protected virtual void Update()
    {
        // Block all interaction when hovering UI
        if (IsPointerOverUI())
        {
            ClearHover();
            return;
        }

        HandleHover();
        HandleInput();
    }

    public void Init(RegionName region, int levelIndex)
    {
        levelDef = new LevelDefinition(region, levelIndex);
        RefreshInfo();
    }

    void HandleHover()
    {
        bool hit = IsMouseOverThisObject();

        if (hit && !isHovered)
        {
            isHovered = true;
            OnHoverEnter();
        }
        else if (!hit && isHovered)
        {
            isHovered = false;
            OnHoverExit();
        }
    }

    void HandleInput()
    {
        if (!isHovered)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            isPressed = true;
            OnPressed();
        }

        if (Input.GetMouseButtonUp(0) && isPressed)
        {
            isPressed = false;
            OnReleased();
        }
    }

    bool IsMouseOverThisObject()
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return col.OverlapPoint(worldPos);
    }

    bool IsPointerOverUI()
    {
        return EventSystem.current != null &&
               EventSystem.current.IsPointerOverGameObject();
    }

    void ClearHover()
    {
        if (!isHovered)
            return;

        isHovered = false;
        isPressed = false;
        OnHoverExit();
    }

    // =========================
    // Overridable Callbacks
    // =========================

    protected virtual void OnHoverEnter()
    {
        // Mouse entered
        // Example: highlight
        print("entered");
        SetState(LevelStoneState.Highlight);
    }

    protected virtual void OnHoverExit()
    {
        // Mouse exited
        // Example: remove highlight
        print("exit");
        SetState(LevelStoneState.Default);
    }

    protected virtual void OnPressed()
    {
        // Mouse button down
        // Example: press animation
        print("pressed");
        SetState(LevelStoneState.Pressed);
    }

    protected virtual void OnReleased()
    {
        // Mouse button released
        // Example: open levelStone
        print("released");
        SetState(LevelStoneState.Release);
        OverworldLevelViewerUIPage.Instance.SetActive(true);
        OverworldLevelViewerUIPage.Instance.SetLevel(this);
    }

    public void SetState(LevelStoneState state)
    {
        if (sr) sr.sprite = spriteStages[(state == LevelStoneState.Release ? LevelStoneState.Default : state).ToInt()];

        if (speechbubbleAnim != null && state != LevelStoneState.Release) ;
        {
            switch (state)
            {
                case LevelStoneState.Default:
                    speechbubbleAnim.SetTrigger("Close");
                    break;
                case LevelStoneState.Pressed:
                    break;
                case LevelStoneState.Highlight:
                    speechbubbleAnim.SetTrigger("Show");
                    break;
            }
        }
    }

    public bool StartLevel()
    {
        var level = LevelDatabase.GetLevel(levelDef);

        if (level == null)
        {
            return false;
        }
        if (!PlayerParty.Instance.HasTeamMembers())
        {
            ErrorDisplayManager.ShowError("Please assign at least one foodling to your party!");
            return false;
        }

        

        GameMatchManager.SetLevel(level, levelDef);
        SceneTransitor.Instance.TransitionTo(SceneType.BattleScene);
        return true;
    }

    public void RefreshInfo()
    {
        var level = LevelDatabase.GetLevel(levelDef);

        if (level == null) return;

        enemyLevelText.text = $"{level.GetAverageLevel()}";
        levelNameText.text = $"{level.LevelName}";
        chapterText.text = $"{ChapterTitleText(levelDef)}";
    }

    private static string ChapterTitleText(LevelDefinition levelDef)
    {
        return $"Chapter {levelDef.region.ToInt() + 1} - {levelDef.index + 1}";
    }

}
