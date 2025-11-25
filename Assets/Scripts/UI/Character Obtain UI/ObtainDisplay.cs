using CustomLibrary.SpriteExtra;
using UnityEngine;
using UnityEngine.UI;

public class ObtainDisplay : MonoBehaviour
{
    public CharacterData heldData;

    public Image icon;
    public float iconSize;
    // Star Display


    public void AssignCharacter(CharacterData data)
    {
        heldData = data;
        Display();
    }

    public void Display()
    {
        if (heldData == null)
        {
            icon.gameObject.SetActive(false);
        } else
        {
            icon.gameObject.SetActive(true);
            var size = SpriteExtra.DynamicDimension(heldData.unitSprite, iconSize);
            icon.sprite = heldData.unitSprite;
            icon.rectTransform.sizeDelta = size;
        }
    }
}
