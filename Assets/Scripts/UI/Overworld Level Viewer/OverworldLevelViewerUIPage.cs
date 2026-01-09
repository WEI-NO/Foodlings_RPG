using CustomLibrary.References;
using TMPro;
using UnityEngine;

public class OverworldLevelViewerUIPage : BaseUIPage
{
    public static OverworldLevelViewerUIPage Instance;

    public OverworldLevelController level;

    [Header("Components")]
    public TextMeshProUGUI levelName;
    public TextMeshProUGUI monsterLevelText;

    protected override void OnAwake()
    {
        Initializer.SetInstance(this);
    }

    public void SetLevel(OverworldLevelController level)
    {
        this.level = level;

        if (level == null) return;

        if (levelName)
        {
            levelName.text = level.level.LevelName;
        }

        if (monsterLevelText)
        {
            monsterLevelText.text = $"{level.level.GetAverageLevel()}";
        }
    }

    protected override void OnContentEnabled()
    {

    }

    protected override void OnContentDisabled()
    {
        level = null;
    }

    public void StartBattle()
    {
        if (level == null)
        {
            ErrorDisplayManager.ShowError("Level is invalid");
            return;
        }

        level.StartLevel();
    }
}
