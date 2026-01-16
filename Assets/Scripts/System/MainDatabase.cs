using CustomLibrary.References;
using UnityEngine;

public class MainDatabase : MonoBehaviour
{
    public static MainDatabase Instance;

    public ItemDatabase itemDatabase;
    public UpgradeDatabase upgradeDatabase;
    public SpriteDatabase spriteDatabase;

    private void Awake()
    {
        Initializer.SetInstance(this);
    }
}
