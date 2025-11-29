using UnityEngine;

public class FusionFunction : CharacterDisplayFunction
{
    public Transform content;
    public FuzeSelectionDisplay displayPrefab;

    [SerializeField] private FusionCollectionPage fusionCollection;

    private void Start()
    {
        
    }

    private void ClearDisplay()
    {
        // Keeps first one

    }

    

    protected override void OnEnabled()
    {
        ClearDisplay();
        active = false;
    }

    protected override void OnDisabled()
    {
        
    }

}
