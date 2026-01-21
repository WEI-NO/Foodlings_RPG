using CustomLibrary.References;
using TMPro;
using UnityEngine;

public class OverworldLevelViewerUIPage : BaseUIPage
{
    public static OverworldLevelViewerUIPage Instance;

    public LevelStone levelStone;

    [Header("Components")]
    public TextMeshProUGUI levelName;
    public TextMeshProUGUI monsterLevelText;

    protected override void OnAwake()
    {
        Initializer.SetInstance(this);
    }

    public void SetLevel(LevelStone levelStone)
    {
        this.levelStone = levelStone;
        if (levelStone == null) return;

        var level = LevelDatabase.GetLevel(levelStone.region, levelStone.assignedIndex);

        if (level == null) return;

        if (levelName)
        {
            levelName.text = level.LevelName;
        }

        if (monsterLevelText)
        {
            monsterLevelText.text = $"{level.GetAverageLevel()}";
        }
    }

    protected override void OnContentEnabled()
    {

    }

    protected override void OnContentDisabled()
    {
        levelStone = null;
    }

    public void StartBattle()
    {
        if (levelStone == null)
        {
            ErrorDisplayManager.ShowError("Level is invalid");
            return;
        }

        levelStone.StartLevel();
    }
}
