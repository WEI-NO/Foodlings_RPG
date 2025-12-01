using UnityEngine;

public enum FlavorTrait { Sweet, Salty, Rich, Spicy, Fresh }
public enum StyleTrait { Baked, Fried, Grilled, Roasted, Steamed, Boiled, Raw }

[System.Serializable]
public class CharacterTrait
{
    [SerializeField] private FlavorTrait primaryTrait;
    [SerializeField] private StyleTrait secondaryTrait;

    public FlavorTrait PrimaryTrait()
    {
        return primaryTrait;
    }

    public StyleTrait SecondaryTrait()
    {
        return secondaryTrait;
    }
}
