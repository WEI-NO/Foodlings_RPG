using CustomLibrary.References;
using CustomLibrary.SpriteExtra;
using UnityEngine;
using UnityEngine.UI;

public class CharacterRankDisplay : MonoBehaviour
{

    public Sprite activeStar;
    public Sprite inactiveStar;

    public float starPixelSize = 16f;
    public void Init(int star)
    {
        ClearStars();
        int s = Mathf.Clamp(star, 0, UnitRank.Count.ToInt());

        for (int i = 0; i < UnitRank.Count.ToInt(); i++)
        {
            Sprite spriteToUse = (i < s + 1) ? activeStar : inactiveStar;
            CreateStar(spriteToUse);
        }
    }

    private void CreateStar(Sprite sprite)
    {
        GameObject starObj = new GameObject("Star");
        starObj.transform.SetParent(transform, false);
        Image img = starObj.AddComponent<Image>();
        img.sprite = sprite;
        SpriteExtra.SetDynamicDimension(img, starPixelSize);
    }

    private void ClearStars()
    {
        for (int i = transform.childCount - 1; i >= 0; --i)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }
}
