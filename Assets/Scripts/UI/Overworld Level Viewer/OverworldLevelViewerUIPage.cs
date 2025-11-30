using CustomLibrary.References;
using UnityEngine;

public class OverworldLevelViewerUIPage : BaseUIPage
{
    public static OverworldLevelViewerUIPage Instance;

    protected override void OnAwake()
    {
        Initializer.SetInstance(this);
    }

    protected override void OnContentEnabled()
    {
        
    }

    protected override void OnContentDisabled()
    {
        
    }
}
