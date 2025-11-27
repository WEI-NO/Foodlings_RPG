using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class FusionCollectionPage : BaseUIPage
{
    [Header("Collection View")]
    public Transform content;
    public FuzeCharacterDisplay fuzeDisplayPrefab;
    public List<FuzeCharacterDisplay> displays = new();

    protected override void OnContentEnabled()
    {
        RefreshDisplay();
    }

    public void RefreshDisplay()
    {
        ClearDisplay();
        if (CharacterUpgradePage.Instance == null) return;

        var l = PlayerCollection.Instance.GetSorted(CollectionSortMode.Level);
        foreach (var i in l)
        {
            if (CharacterUpgradePage.Instance.IsDisplayedCharacter(i) || i.inPartyIndex <= -1) continue;

            AddDisplay(i);
        }
    }

    private void AddDisplay(CharacterInstance instance)
    {
        var selectedList = CharacterUpgradePage.Instance.selectedFusingUnit;
        if (selectedList == null) return;

        if (selectedList.Contains(instance))
        {
            FuzeCharacterDisplay newDisplay = Instantiate(fuzeDisplayPrefab, content);
            newDisplay.Initialize(instance, true);
        } else
        {
            FuzeCharacterDisplay newDisplay = Instantiate(fuzeDisplayPrefab, content);
            newDisplay.Initialize(instance, false);
        }
    }

    private void ClearDisplay()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            var t = content.GetChild(i);
            if (t)
            {
                Destroy(t.gameObject);
            }
        }
        displays.Clear();
    }

   
    
}
